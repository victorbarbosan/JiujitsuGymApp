using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly int _pageSize;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(UserManager<User> userManager, ILogger<AdminController> logger, IConfiguration config)
        {
            _userManager = userManager;
            _logger = logger;
            _pageSize = config.GetValue<int>("Pagination:DefaultPageSize", 50);
        }

        // GET : Admin/
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var initialUsers = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.FirstName)
                .Take(_pageSize)
                .ToListAsync();
            return View(initialUsers);
        }


        // GET : Admin/GetUsers
        [HttpGet]
        public async Task<IActionResult> GetUsers(int skip = 0)
        {

            var userList = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.FirstName)
                .Skip(skip)
                .Take(_pageSize)
                .ToListAsync();

            return Json(userList.Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                Belt = u.Belt.HasValue ? u.Belt.Value.ToString() : "Not Set"
            }));
        }
    }
}
