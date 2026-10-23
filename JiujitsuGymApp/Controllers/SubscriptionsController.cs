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

        // POST: Subscriptions/Checkout/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Challenge();

            var successUrl = Url.Action(nameof(Success), "Subscriptions", null, Request.Scheme)!;
            var cancelUrl = Url.Action(nameof(Index), "Subscriptions", null, Request.Scheme)!;

            var checkoutUrl = await _subscriptionService.CreateCheckoutSessionAsync(
                user.Id,
                user.Email!,
                id,
                successUrl,
                cancelUrl);

            return Redirect(checkoutUrl);
        }

        // GET: Subscriptions/Success
        public IActionResult Success()
        {
            // The real confirmation of payment comes from the Stripe webhook,
            // not from this redirect - this page is just a friendly landing
            // spot while the webhook (processed asynchronously) catches up.
            return View();
        }
    }
}
