using JiujitsuGymApp.Data;
using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Controllers
{
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ScheduleService _scheduleService;

        public ClassesController(ApplicationDbContext context, ScheduleService scheduleService)
        {
            _context = context;
            _scheduleService = scheduleService;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            await _scheduleService.EnsureSessionsGeneratedAsync();

            var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            var userId = await _context.Users
                .Where(u => u.UserName == User.Identity!.Name)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            var classes = await GetClassEventsAsync(weekStart, weekEnd, userId);

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

            var userId = await _context.Users
                .Where(u => u.UserName == User.Identity!.Name)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            var classes = await GetClassEventsAsync(fromDate, toDate, userId);
            return Json(classes);
        }

        private async Task<List<ClassEventDto>> GetClassEventsAsync(DateTime from, DateTime to, string? userId)
        {
            return await _context.Classes
                .AsNoTracking()
                .Include(c => c.Teacher)
                .Include(c => c.Attendances)
                .Where(c => c.DeletedAt == null && c.DateTime >= from && c.DateTime < to)
                .OrderBy(c => c.DateTime)
                .Select(c => new ClassEventDto
                {
                    Id = c.Id,
                    Location = c.Location,
                    TeacherName = c.Teacher.FirstName + " " + c.Teacher.LastName,
                    DateTime = c.DateTime.ToString("o"),
                    AttendanceCount = c.Attendances.Count,
                    CheckedIn = userId != null && c.Attendances.Any(a => a.UserId == userId)
                })
                .ToListAsync();
        }
    }
}