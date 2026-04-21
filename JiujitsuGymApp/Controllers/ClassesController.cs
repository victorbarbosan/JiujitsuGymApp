using JiujitsuGymApp.Data;
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

        public async Task<IActionResult> Index()
        {
            await _scheduleService.EnsureSessionsGeneratedAsync();

            // Fetch classes and include Teacher info so we can show the name
            var classes = await _context.Classes
                .Include(c => c.Teacher)
                .Where(c => c.DeletedAt == null) // Only show non-deleted classes
                .OrderBy(c => c.DateTime)
                .ToListAsync();

            return View(classes);
        }
    }
}