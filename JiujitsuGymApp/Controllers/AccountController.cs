using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiujitsuGymApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            AccountService accountService,
            IEmailQueue emailQueue,
            ILogger<AccountController> logger
            )
        {
            _accountService = accountService;
            _emailQueue = emailQueue;
            _logger = logger;
        }

        // GET : /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST : /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var succeeded = await _accountService.LoginAsync(model.Email, model.Password, model.RememberMe);

                if (succeeded)
                {
                    _logger.LogInformation("User logged in: {Email}", model.Email);
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt");
            }
            return View(model);
        }

        // GET : Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var model = new RegisterViewModel();
            return View(model);
        }

        // POST : Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var dto = new CreateUserDto
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Belt = model.Belt?.ToString() ?? "White",
                    Password = model.Password,
                    Role = "Member"
                };

                var errors = await _accountService.RegisterAsync(dto);

                if (!errors.Any())
                {
                    _logger.LogInformation("User created a new account: {Email}", model.Email);
                    return RedirectToLocal(returnUrl);
                }

                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            return View(model);
        }

        // GET : /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST : /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = await _accountService.GeneratePasswordResetTokenAsync(model.Email);
            if (token is not null)
            {
                // Url.Action percent-encodes the token so it survives the query
                // string; PasswordResetEmail HTML encodes it for the href.
                var link = Url.Action(nameof(ResetPassword), "Account",
                    new { email = model.Email, token }, protocol: Request.Scheme)!;

                // Queued rather than sent inline. The relay handshake costs a
                // second or two, and doing it here also leaked whether the
                // address was registered: a hit waited for Gmail, a miss
                // returned instantly. Queuing makes both responses equally fast.
                _emailQueue.Enqueue(PasswordResetEmail.Create(model.Email, link));

                _logger.LogInformation("Password reset link generated and queued for {Email}", model.Email);
            }

            // Same page either way - never reveal whether the address is registered
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // GET : /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET : /Account/ResetPassword (link target from the email)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? email = null, string? token = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction(nameof(ForgotPassword));

            var model = new ResetPasswordViewModel { Email = email, Token = token };
            return View(model);
        }

        // POST : /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var errors = await _accountService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);

            if (!errors.Any())
            {
                _logger.LogInformation("Password reset completed for {Email}", model.Email);
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        // GET : /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // POST : Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            _logger.LogInformation("User logged out: {Name}", User.Identity?.Name);
            return RedirectToAction("Index", "Home");
        }

        // GET : Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
