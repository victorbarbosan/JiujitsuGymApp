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
        /// Creates a Stripe Checkout Session in subscription mode and returns
        /// the hosted URL to redirect the student to. A local Subscription
        /// row is created up front in Incomplete status so the webhook step
        /// has a record to update once Stripe confirms payment.
        /// </summary>
        public async Task<string> CreateCheckoutSessionAsync(
            string userId,
            string userEmail,
            int planId,
            string successUrl,
            string cancelUrl)
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

            var subscription = new Subscription
            {
                UserId = userId,
                SubscriptionPlanId = plan.Id,
                StripeCustomerId = existingCustomerId,
                Status = SubscriptionStatus.Incomplete
            };
            db.Subscriptions.Add(subscription);
            await db.SaveChangesAsync();

            var sessionOptions = new SessionCreateOptions
            {
                Mode = "subscription",
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = plan.StripePriceId,
                        Quantity = 1
                    }
                ],
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
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
            var session = await sessionService.CreateAsync(sessionOptions);

            return session.Url;
        }
    }
}
