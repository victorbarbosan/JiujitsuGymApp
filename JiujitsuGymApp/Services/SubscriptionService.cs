using JiujitsuGymApp.Data;
using JiujitsuGymApp.Dtos;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using Subscription = JiujitsuGymApp.Models.Subscription;
using SubscriptionStatus = JiujitsuGymApp.Models.SubscriptionStatus;

namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// Owns the Stripe Checkout flow for student subscriptions. Only ever
    /// talks to Stripe to *start* a subscription - the resulting state
    /// (active, past_due, canceled, ...) is written back by the webhook
    /// handler (next step), since Checkout's client-side redirect is not a
    /// reliable signal that payment actually succeeded.
    /// </summary>
    public class SubscriptionService(ApplicationDbContext db, StripeClient stripeClient)
    {
        public async Task<List<SubscriptionPlanDto>> GetActivePlansAsync()
        {
            return await db.SubscriptionPlans
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .Select(p => new SubscriptionPlanDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Interval = p.Interval.ToString()
                })
                .ToListAsync();
        }

        public async Task<SubscriptionStatusDto> GetStatusAsync(string userId)
        {
            var subscription = await db.Subscriptions
                .AsNoTracking()
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            if (subscription is null)
            {
                return new SubscriptionStatusDto { HasSubscription = false };
            }

            return new SubscriptionStatusDto
            {
                HasSubscription = true,
                PlanName = subscription.SubscriptionPlan?.Name,
                Status = subscription.Status.ToString(),
                CurrentPeriodEnd = subscription.CurrentPeriodEnd
            };
        }

        /// <summary>
        /// Creates a Stripe embedded Checkout Session (ui_mode: embedded_page)
        /// in subscription mode and returns the client secret used to mount
        /// Stripe's Checkout UI in an iframe on our own page. A local
        /// Subscription row is created up front in Incomplete status so the
        /// webhook step has a record to update once Stripe confirms payment.
        /// </summary>
        public async Task<string> CreateEmbeddedCheckoutSessionAsync(
            string userId,
            string userEmail,
            int planId,
            string returnUrl)
        {
            var plan = await db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == planId && p.IsActive)
                ?? throw new InvalidOperationException($"Subscription plan {planId} was not found or is inactive.");

            if (string.IsNullOrWhiteSpace(plan.StripePriceId))
            {
                throw new InvalidOperationException(
                    $"Subscription plan '{plan.Name}' has no StripePriceId configured.");
            }

            // Reuse an existing Stripe Customer for this user if we have already
            // started a subscription before, otherwise let Stripe create one.
            var existingCustomerId = await db.Subscriptions
                .Where(s => s.UserId == userId && s.StripeCustomerId != null)
                .Select(s => s.StripeCustomerId)
                .FirstOrDefaultAsync();

            // Guard against duplicate billing: a student who already has an
            // Active (or otherwise paid) subscription should manage it via the
            // Customer Portal, not start a second one from this page.
            var hasLiveSubscription = await db.Subscriptions.AnyAsync(s =>
                s.UserId == userId &&
                s.Status != SubscriptionStatus.Incomplete &&
                s.Status != SubscriptionStatus.IncompleteExpired &&
                s.Status != SubscriptionStatus.Canceled);
            if (hasLiveSubscription)
            {
                throw new InvalidOperationException(
                    "You already have an active subscription. Manage it from your billing portal instead of starting a new one.");
            }

            // Reuse a still-pending attempt for this same plan (e.g. the
            // student backed out of Checkout and clicked Subscribe again)
            // rather than piling up a fresh Incomplete row every click.
            var subscription = await db.Subscriptions.FirstOrDefaultAsync(s =>
                s.UserId == userId &&
                s.SubscriptionPlanId == plan.Id &&
                s.Status == SubscriptionStatus.Incomplete);

            var isNewSubscription = subscription is null;
            if (subscription is null)
            {
                subscription = new Subscription
                {
                    UserId = userId,
                    SubscriptionPlanId = plan.Id,
                    StripeCustomerId = existingCustomerId,
                    Status = SubscriptionStatus.Incomplete
                };
                db.Subscriptions.Add(subscription);
            }
            else
            {
                subscription.StripeCustomerId ??= existingCustomerId;
            }

            await db.SaveChangesAsync();

            var sessionOptions = new SessionCreateOptions
            {
                Mode = "subscription",
                UiMode = "embedded_page",
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = plan.StripePriceId,
                        Quantity = 1
                    }
                ],
                // return_url replaces success_url/cancel_url for embedded
                // sessions - Stripe appends {CHECKOUT_SESSION_ID} itself and
                // redirects the iframe's top window here once the student is
                // done, regardless of outcome. The page at this URL checks
                // the session status itself (see SubscriptionsController).
                ReturnUrl = returnUrl,
                // Lets the webhook correlate the Stripe session/subscription
                // back to our local Subscription row without guessing.
                ClientReferenceId = subscription.Id.ToString(),
                Metadata = new Dictionary<string, string>
                {
                    ["subscriptionId"] = subscription.Id.ToString(),
                    ["userId"] = userId
                }
            };

            if (existingCustomerId is not null)
            {
                sessionOptions.Customer = existingCustomerId;
            }
            else
            {
                sessionOptions.CustomerEmail = userEmail;
            }

            var sessionService = new SessionService(stripeClient);
            Session session;
            try
            {
                session = await sessionService.CreateAsync(sessionOptions);
            }
            catch (StripeException)
            {
                // Stripe rejected the session (e.g. bad Price ID). Only remove
                // the row if we created it just now - a reused pending row
                // from an earlier attempt should be left alone to retry later.
                if (isNewSubscription)
                {
                    db.Subscriptions.Remove(subscription);
                    await db.SaveChangesAsync();
                }
                throw;
            }

            return session.ClientSecret;
        }

        /// <summary>
        /// Looks up a Checkout Session's own status for the return page's
        /// immediate feedback. This is a convenience read only - the webhook
        /// handler remains the source of truth that actually flips the local
        /// Subscription row to Active.
        /// </summary>
        public async Task<string?> GetCheckoutSessionStatusAsync(string sessionId)
        {
            var sessionService = new SessionService(stripeClient);
            var session = await sessionService.GetAsync(sessionId);
            return session.Status;
        }
    }
}
