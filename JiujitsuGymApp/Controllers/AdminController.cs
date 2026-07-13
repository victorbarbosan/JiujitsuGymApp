using JiujitsuGymApp.Data;
using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserService _userService;

        public AdminController(
            UserManager<User> userManager,
            ILogger<AdminController> logger,
            ApplicationDbContext context,
            UserService userService)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
            _userService = userService;
        }

        // GET : Admin/
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var initialUsers = await _userService.GetUsersAsync();

            var initialSchedules = await _context.ClassSchedules
                .AsNoTracking()
                .Include(s => s.Teacher)
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.TimeOfDay)
                .ToListAsync();

            ViewBag.InitialSchedules = initialSchedules.Select(ToScheduleDto).ToList();

            return View(initialUsers);
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
            return Json(await _userService.GetEligibleTeachersAsync());
        }

        // GET : Admin/GetUsers
        [HttpGet]
        public async Task<IActionResult> GetUsers(int skip = 0, string? query = null, string? sortBy = "firstName", string? sortDir = "asc")
        {
            return Json(await _userService.GetUsersAsync(skip, query, sortDir));
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

            var (userDto, serviceErrors) = await _userService.CreateUserAsync(dto);

            if (serviceErrors.Any())
                return BadRequest(new { errors = serviceErrors });

            _logger.LogInformation("Admin created new user: {Email} with role: {Role}", dto.Email, dto.Role);
            return Ok(userDto);
        }

        // GET : Admin/GetUser/abc123
        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            var userDto = await _userService.GetUserByIdAsync(id);
            if (userDto is null) return NotFound();
            return Json(userDto);
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

            var (userDto, serviceErrors) = await _userService.UpdateUserAsync(id, dto);

            if (serviceErrors.Any())
                return BadRequest(new { errors = serviceErrors });

            _logger.LogInformation("Admin updated user: {Email}", dto.Email);
            return Ok(userDto);
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
    }
}