using JiujitsuGymApp.Data;
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
        private readonly ApplicationDbContext _context;

        private static readonly HashSet<string> _allowedRoles = ["Admin", "Member", "Teacher"];

        public AdminController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger,
            IConfiguration config,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _pageSize = config.GetValue<int>("Pagination:DefaultPageSize", 50);
            _context = context;
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

            var initialSchedules = await _context.ClassSchedules
                .AsNoTracking()
                .Include(s => s.Teacher)
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.TimeOfDay)
                .ToListAsync();

            ViewBag.InitialSchedules = initialSchedules.Select(ToScheduleDto).ToList();

            return View(initialUsers.Select(u => ToDto(u)).ToList());
        }

        // GET : Admin/GetSchedules
        [HttpGet]
        public async Task<IActionResult> GetSchedules()
        {
            var schedules = await _context.ClassSchedules
                .AsNoTracking()
                .Include(s => s.Teacher)
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.TimeOfDay)
                .ToListAsync();

            return Json(schedules.Select(ToScheduleDto));
        }

        // POST : Admin/CreateSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateClassScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            if (!Enum.TryParse<DayOfWeek>(dto.DayOfWeek, out var day))
                return BadRequest(new { errors = new[] { "Invalid day of week." } });

            if (!TimeSpan.TryParse(dto.TimeOfDay, out var time))
                return BadRequest(new { errors = new[] { "Invalid time format. Use HH:mm." } });

            var teacher = await _userManager.FindByIdAsync(dto.TeacherId);
            if (teacher is null)
                return BadRequest(new { errors = new[] { "Teacher not found." } });

            var schedule = new ClassSchedule
            {
                TeacherId = dto.TeacherId,
                Location = dto.Location,
                DayOfWeek = day,
                TimeOfDay = time,
                IsActive = true
            };

            _context.ClassSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            schedule.Teacher = teacher;
            _logger.LogInformation("Schedule created: {Day} {Time} at {Location}", day, time, dto.Location);

            return Ok(ToScheduleDto(schedule));
        }

        // DELETE : Admin/DeleteSchedule/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.ClassSchedules.FindAsync(id);
            if (schedule is null) return NotFound();

            schedule.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET : Admin/GetTeachers
        [HttpGet]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            var eligible = teachers.Concat(admins)
                .GroupBy(u => u.Id) // If a user is in both roles, this groups them together
                .Select(g => g.First()) // Take one instance of the user
                .OrderBy(u => u.FirstName)
                .Select(u => new { id = u.Id, name = $"{u.FirstName} {u.LastName}" })
                .ToList();

            return Json(eligible);
        }

        // GET : Admin/GetUsers
        [HttpGet]
        public async Task<IActionResult> GetUsers(int skip = 0, string? query = null, string? sortBy = "firstName", string? sortDir = "asc")
        {
            var users = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                users = users.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(q) ||
                    u.Email!.ToLower().Contains(q));
            }

            var dir = sortDir?.ToLower() ?? "asc";

            users = dir switch
            {
                "asc" => users.OrderBy(u => u.FirstName),
                "desc" => users.OrderByDescending(u => u.FirstName),
                _ => users.OrderBy(u => u.FirstName)
            };

            var userList = await users.Skip(skip).Take(_pageSize).ToListAsync();
            return Json(userList.Select(u => ToDto(u)).ToList());
        }

        // POST : Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
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

        // GET : Admin/GetUser/abc123
        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            return Json(ToDto(user, roles.FirstOrDefault() ?? "Member"));
        }

        // PUT : Admin/UpdateUser/abc123
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            if (!_allowedRoles.Contains(dto.Role))
                return BadRequest(new { errors = new[] { $"Invalid role '{dto.Role}'." } });

            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound();

            // Update email/username if changed
            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManager.SetEmailAsync(user, dto.Email);
                if (!emailResult.Succeeded)
                    return BadRequest(new { errors = emailResult.Errors.Select(e => e.Description) });

                await _userManager.SetUserNameAsync(user, dto.Email);
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;

            if (Enum.TryParse<BeltColor>(dto.Belt, out var belt))
                user.Belt = belt;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(new { errors = updateResult.Errors.Select(e => e.Description) });

            // Sync role
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(dto.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            _logger.LogInformation("Admin updated user: {Email}", user.Email);
            return Ok(ToDto(user, dto.Role));
        }

        private static ClassScheduleDto ToScheduleDto(ClassSchedule s) => new()
        {
            Id = s.Id,
            TeacherId = s.TeacherId,
            TeacherName = s.Teacher?.Name ?? string.Empty,
            Location = s.Location,
            DayOfWeek = s.DayOfWeek.ToString(),
            TimeOfDay = s.TimeOfDay.ToString(@"hh\:mm"),
            IsActive = s.IsActive
        };

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

        private sealed class UserIdComparer : IEqualityComparer<User>
        {
            public bool Equals(User? x, User? y) => x?.Id == y?.Id;
            public int GetHashCode(User obj) => obj.Id.GetHashCode();
        }
    }
}