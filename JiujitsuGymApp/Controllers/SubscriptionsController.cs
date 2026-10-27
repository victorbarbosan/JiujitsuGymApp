using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JiujitsuGymApp.Controllers
{
    public class SubscriptionsController : Controller
    {
        private readonly SubscriptionService _subscriptionService;
        private readonly UserManager<User> _userManager;

        public SubscriptionsController(SubscriptionService subscriptionService, UserManager<User> userManager)
        {
            _subscriptionService = subscriptionService;
            _userManager = userManager;
        }

        // GET: Subscriptions
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            var plans = await _subscriptionService.GetActivePlansAsync();
            var status = await _subscriptionService.GetStatusAsync(user.Id);

            ViewBag.Status = status;
            return View(plans);
        }

        // POST: Subscriptions/Checkout
        // Returns JSON ({ clientSecret }) rather than redirecting - the
        // client-side script uses this to mount Stripe's embedded Checkout
        // iframe in place, instead of navigating away to a hosted page.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            // Stripe appends {CHECKOUT_SESSION_ID} itself; ReturnUrl must contain
            // that exact placeholder for Stripe to substitute the real session id.
            var returnUrl = Url.Action(nameof(Return), "Subscriptions", null, Request.Scheme)
                + "?session_id={CHECKOUT_SESSION_ID}";

            var clientSecret = await _subscriptionService.CreateEmbeddedCheckoutSessionAsync(
                user.Id,
                user.Email!,
                request.Id,
                returnUrl);

            return Json(new { clientSecret });
        }

        // GET: Subscriptions/Return
        // Stripe redirects the top-level window here after the embedded
        // Checkout form is done (success or cancellation). This page reads
        // the session back from Stripe only to show immediate feedback - the
        // webhook (not this request) is what actually confirms the payment
        // and updates our local Subscription row.
        public async Task<IActionResult> Return(string session_id)
        {
            var status = await _subscriptionService.GetCheckoutSessionStatusAsync(session_id);
            ViewBag.SessionStatus = status;
            return View();
        }

        public class CheckoutRequest
        {
            public int Id { get; set; }
        }
    }
}
