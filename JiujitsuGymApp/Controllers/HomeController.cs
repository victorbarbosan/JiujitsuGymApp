using System.Diagnostics;
using JiujitsuGymApp.Data;
using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
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

            var todayClasses = await _context.Classes
                .AsNoTracking()
                .Include(c => c.Teacher)
                .Include(c => c.Attendances)
                .Where(c => c.DeletedAt == null && c.DateTime >= todayUtc && c.DateTime < tomorrowUtc)
                .OrderBy(c => c.DateTime)
                .Select(c => new ClassEventDto
                {
                    Id = c.Id,
                    Location = c.Location,
                    TeacherName = c.Teacher.FirstName + " " + c.Teacher.LastName,
                    DateTime = c.DateTime.ToString("o"),
                    AttendanceCount = c.Attendances.Count,
                    CheckedIn = c.Attendances.Any(a => a.UserId == user.Id)
                })
                .ToListAsync();

            var totalAttended = await _context.Attendances
                .CountAsync(a => a.UserId == user.Id);

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
