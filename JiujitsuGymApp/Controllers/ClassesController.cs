using JiujitsuGymApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Controllers
{
    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
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