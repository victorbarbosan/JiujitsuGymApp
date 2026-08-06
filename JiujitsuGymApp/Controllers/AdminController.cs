using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiujitsuGymApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly UserService _userService;
        private readonly ScheduleService _scheduleService;
        private readonly DemoDataService _demoDataService;

        public AdminController(
            ILogger<AdminController> logger,
            UserService userService,
            ScheduleService scheduleService,
            DemoDataService demoDataService)
        {
            _logger = logger;
            _userService = userService;
            _scheduleService = scheduleService;
            _demoDataService = demoDataService;
        }

        // GET : Admin/
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var initialUsers = await _userService.GetUsersAsync();

            ViewBag.InitialSchedules = await _scheduleService.GetSchedulesAsync();
            ViewBag.DemoDataStatus = await _demoDataService.GetStatusAsync();

            return View(initialUsers);
        }

        // GET : Admin/GetSchedules
        [HttpGet]
        public async Task<IActionResult> GetSchedules()
        {
            return Json(await _scheduleService.GetSchedulesAsync());
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

            var (scheduleDto, serviceErrors) = await _scheduleService.CreateScheduleAsync(dto);

            if (serviceErrors.Any())
                return BadRequest(new { errors = serviceErrors });

            _logger.LogInformation("Schedule created: {Day} {Time} at {Location}",
                scheduleDto!.DayOfWeek, scheduleDto.TimeOfDay, scheduleDto.Location);

            return Ok(scheduleDto);
        }

        // DELETE : Admin/DeleteSchedule/5
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var found = await _scheduleService.DeactivateScheduleAsync(id);
            return found ? Ok() : NotFound();
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

        // GET : Admin/GetDemoDataStatus
        [HttpGet]
        public async Task<IActionResult> GetDemoDataStatus()
        {
            return Json(await _demoDataService.GetStatusAsync());
        }

        // POST : Admin/SeedDemoData
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedDemoData()
        {
            var (result, errors) = await _demoDataService.SeedAsync();

            if (errors.Any())
                return BadRequest(new { errors });

            _logger.LogInformation("Admin seeded demo data: {Message}", result!.Message);
            return Ok(result);
        }

        // DELETE : Admin/PurgeDemoData
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PurgeDemoData()
        {
            var result = await _demoDataService.PurgeAsync();

            _logger.LogWarning("Admin purged demo data: {Message}", result.Message);
            return Ok(result);
        }
    }
}
