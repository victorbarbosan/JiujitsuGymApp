using System.Diagnostics;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JiujitsuGymApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ClassService _classService;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<User> userManager,
            ClassService classService)
        {
            _logger = logger;
            _userManager = userManager;
            _classService = classService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return View();

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
                return View();

            var todayUtc = DateTime.UtcNow.Date;
            var tomorrowUtc = todayUtc.AddDays(1);

            var todayClasses = await _classService.GetClassEventsAsync(todayUtc, tomorrowUtc, user.Id);
            var totalAttended = await _classService.GetTotalAttendedAsync(user.Id);

            var model = new HomeViewModel
            {
                FirstName = user.FirstName,
                Belt = user.Belt?.ToString() ?? "White",
                TotalClassesAttended = totalAttended,
                TodayClasses = todayClasses,
            };

            return View(model);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
