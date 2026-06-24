using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace JiujitsuGymApp.Controllers
{
    public class ClassesController : Controller
    {
        private readonly ClassService _classService;
        private readonly ScheduleService _scheduleService;
        private readonly ICurrentUserService _currentUserService;

        public ClassesController(ClassService classService, ScheduleService scheduleService, ICurrentUserService currentUserService)
        {
            _classService = classService;
            _scheduleService = scheduleService;
            _currentUserService = currentUserService;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            await _scheduleService.EnsureSessionsGeneratedAsync();

            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            var userId = await _currentUserService.GetUserIdAsync();
            var classes = await _classService.GetClassEventsAsync(weekStart, weekEnd, userId);

            ViewBag.WeekStart = weekStart.ToString("yyyy-MM-dd");
            ViewBag.InitialClasses = classes;

            return View();
        }

        // GET: Classes/GetClasses?from=2025-01-06&to=2025-01-13
        [HttpGet]
        public async Task<IActionResult> GetClasses(string from, string to)
        {
            if (!DateTime.TryParse(from, out var fromDate) || !DateTime.TryParse(to, out var toDate))
                return BadRequest("Invalid date range.");

            var userId = await _currentUserService.GetUserIdAsync();
            var classes = await _classService.GetClassEventsAsync(fromDate, toDate, userId);
            return Json(classes);
        }

        // POST: Classes/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var userId = await _currentUserService.GetUserIdAsync();
            if (userId is null) return Unauthorized();

            var result = await _classService.CheckInAsync(id, userId);

            return result switch
            {
                CheckInResult.NotFound => NotFound(),
                CheckInResult.AlreadyCheckedIn => Conflict(new { error = "Already checked in to this class." }),
                _ => Ok()
            };
        }

        // DELETE: Classes/UndoCheckIn/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UndoCheckIn(int id)
        {
            var userId = await _currentUserService.GetUserIdAsync();
            if (userId is null) return Unauthorized();

            var result = await _classService.UndoCheckInAsync(id, userId);

            return result switch
            {
                CheckInResult.NotFound => NotFound(),
                _ => Ok()
            };
        }
    }
}