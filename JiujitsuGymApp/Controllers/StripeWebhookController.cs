using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace JiujitsuGymApp.Controllers
{
    /// <summary>
    /// Receives Stripe's signed webhook events - the only reliable source of
    /// truth for subscription state. Checkout's browser redirect (see
    /// SubscriptionsController.Return) only tells the *student* what
    /// happened; this endpoint is what Stripe's servers call directly to
    /// tell *us*, and it's what actually updates the local Subscription row.
    ///
    /// Kept as its own controller (rather than an action on
    /// SubscriptionsController) because it has a fundamentally different
    /// caller - Stripe's servers, not a signed-in student - and therefore a
    /// different auth story ([AllowAnonymous] + signature verification
    /// instead of cookie auth).
    /// </summary>
    [ApiController]
    [Route("webhooks/stripe")]
    [AllowAnonymous]
    public class StripeWebhookController(
        JiujitsuGymApp.Services.SubscriptionService subscriptionService,
        IOptions<StripeOptions> stripeOptions) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            // Signature verification needs the *exact* raw bytes Stripe sent -
            // model binding/JSON deserialization would reformat the body and
            // break the signature check, so we read it manually here.
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    stripeOptions.Value.WebhookSecret);
            }
            catch (StripeException)
            {
                // Bad/missing signature - reject rather than trust an
                // unverified payload claiming to be from Stripe.
                return BadRequest();
            }

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    if (stripeEvent.Data.Object is Session session)
                    {
                        await subscriptionService.HandleCheckoutSessionCompletedAsync(session);
                    }
                    break;

                case "customer.subscription.updated":
                    if (stripeEvent.Data.Object is Subscription updatedSubscription)
                    {
                        await subscriptionService.HandleSubscriptionUpdatedAsync(updatedSubscription);
                    }
                    break;

                case "customer.subscription.deleted":
                    if (stripeEvent.Data.Object is Subscription deletedSubscription)
                    {
                        await subscriptionService.HandleSubscriptionDeletedAsync(deletedSubscription);
                    }
                    break;

                // Other event types are ignored - Stripe sends many more
                // events than we currently act on, and unhandled ones should
                // still return 200 so Stripe doesn't keep retrying them.
            }

            return Ok();
        }
    }
}
