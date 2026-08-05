using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JiujitsuGymApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<ProfileController> _logger;
        private readonly ClassService _classService;
        private readonly UserService _userService;

        public ProfileController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<ProfileController> logger,
            ClassService classService,
            UserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _classService = classService;
            _userService = userService;
        }

        // GET: Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var model = ToProfileViewModel(user);
            model.TotalClassesAttended = await _classService.GetTotalAttendedAsync(user.Id);

            return View(model);
        }

        // GET : Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            return View(ToProfileViewModel(user));
        }

        // POST : Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound("Unable to load the current user.");

            var dto = new UpdateProfileDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Belt = model.Belt
            };

            var errors = await _userService.UpdateProfileAsync(userId, dto);
            if (errors.Any())
            {
                foreach (var error in errors)
                    ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            // The navbar reads FirstName, LastName and Belt from claims stamped
            // into the auth cookie, so an edit would not reach it until the next
            // sign-in - the page you were just on would show the new belt while
            // the navbar kept the old one. Re-issuing the cookie runs the claims
            // factory again and picks the new values up on the next request.
            //
            // It also keeps the session alive: changing the email rotates the
            // security stamp, which would otherwise sign the user out the next
            // time that stamp is validated.
            var updated = await _userManager.FindByIdAsync(userId);
            if (updated is not null)
                await _signInManager.RefreshSignInAsync(updated);

            _logger.LogInformation("User updated profile: {Email}", dto.Email);
            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET : Profile/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST : Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);
            if (userId == null) return NotFound("Unable to load the current user.");

            var errors = await _userService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
            if (errors.Any())
            {
                foreach (var error in errors)
                    ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            TempData["SuccessMessage"] = "Your password has been changed!";
            return RedirectToAction(nameof(Index));
        }

        private static ProfileViewModel ToProfileViewModel(User user) => new()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            Belt = user.Belt,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
        };
    }
}
