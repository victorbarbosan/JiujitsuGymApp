using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JiujitsuGymApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
                UserManager<User> userManager,
                ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Map User to ProfileViewModel
            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                Belt = user.Belt.ToString(),
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
            };

            return View(model);
        }
    }
}
