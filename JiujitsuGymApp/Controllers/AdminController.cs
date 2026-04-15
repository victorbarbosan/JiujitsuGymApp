using JiujitsuGymApp.Dtos;
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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        private static readonly HashSet<string> _allowedRoles = ["Admin", "Member", "Teacher"];

        public AdminController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

            return View(initialUsers.Select(u => ToDto(u)).ToList());
        }

        // GET : Admin/GetUsers
        [HttpGet]
        public async Task<IActionResult> GetUsers(int skip = 0, string? query = null)
        {
            var users = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                users = users.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(q) ||
                    u.Email!.ToLower().Contains(q));
            }

            var userList = await users
                .OrderBy(u => u.FirstName)
                .Skip(skip)
                .Take(_pageSize)
                .ToListAsync();

            return Json(userList.Select(u => ToDto(u)).ToList());
        }

        // POST : Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            if (!_allowedRoles.Contains(dto.Role))
                return BadRequest(new { errors = new[] { $"Invalid role '{dto.Role}'." } });

            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                return BadRequest(new { errors = new[] { "A user with this email already exists." } });

            var beltColor = Enum.TryParse<BeltColor>(dto.Belt, out var parsed) ? parsed : BeltColor.White;

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Belt = beltColor,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, dto.Role);
            _logger.LogInformation("Admin created new user: {Email} with role: {Role}", dto.Email, dto.Role);

            return Ok(ToDto(user, dto.Role));
        }

        private static UserDto ToDto(User u, string role = "Member") => new()
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email!,
            PhoneNumber = u.PhoneNumber,
            Belt = u.Belt.HasValue ? u.Belt.Value.ToString() : "Not Set",
            Role = role
        };
    }
}
