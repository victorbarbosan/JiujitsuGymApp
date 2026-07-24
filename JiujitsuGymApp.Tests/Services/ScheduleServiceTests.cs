using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using JiujitsuGymApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Tests.Services;

public sealed class ScheduleServiceTests : IDisposable
{
    private readonly SqliteTestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<User> SeedTeacherAsync()
    {
        using var context = _db.CreateContext();
        var teacher = new User
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "John",
            LastName = "Danaher",
            UserName = "john@gym.com",
            Email = "john@gym.com"
        };
        context.Users.Add(teacher);
        await context.SaveChangesAsync();
        return teacher;
    }

    private async Task<ClassSchedule> SeedScheduleAsync(string teacherId, DayOfWeek day = DayOfWeek.Monday, bool isActive = true)
    {
        using var context = _db.CreateContext();
        var schedule = new ClassSchedule
        {
            TeacherId = teacherId,
            Location = "Main mat",
            DayOfWeek = day,
            TimeOfDay = new TimeSpan(19, 0, 0),
            IsActive = isActive
        };
        context.ClassSchedules.Add(schedule);
        await context.SaveChangesAsync();
        return schedule;
    }

    [Theory]
    [InlineData("Funday")]
    [InlineData("")]
    public async Task CreateScheduleAsync_WithInvalidDayOfWeek_ReturnsErrorAndSavesNothing(string invalidDay)
    {
        using var context = _db.CreateContext();
        var service = new ScheduleService(context);
        var dto = new CreateClassScheduleDto
        {
            TeacherId = "irrelevant",
            Location = "Main mat",
            DayOfWeek = invalidDay,
            TimeOfDay = "19:00"
        };

        var (schedule, errors) = await service.CreateScheduleAsync(dto);

        Assert.Null(schedule);
        Assert.Contains("Invalid day of week.", errors);
        using var assertContext = _db.CreateContext();
        Assert.Equal(0, await assertContext.ClassSchedules.CountAsync());
    }

    [Theory]
    [InlineData("not-a-time")]
    [InlineData("25:99")]
    public async Task CreateScheduleAsync_WithInvalidTime_ReturnsErrorAndSavesNothing(string invalidTime)
    {
        using var context = _db.CreateContext();
        var service = new ScheduleService(context);
        var dto = new CreateClassScheduleDto
        {
            TeacherId = "irrelevant",
            Location = "Main mat",
            DayOfWeek = "Monday",
            TimeOfDay = invalidTime
        };

        var (schedule, errors) = await service.CreateScheduleAsync(dto);

        Assert.Null(schedule);
        Assert.Contains("Invalid time format. Use HH:mm.", errors);
        using var assertContext = _db.CreateContext();
        Assert.Equal(0, await assertContext.ClassSchedules.CountAsync());
    }

    [Fact]
    public async Task CreateScheduleAsync_WhenTeacherDoesNotExist_ReturnsError()
    {
        using var context = _db.CreateContext();
        var service = new ScheduleService(context);
        var dto = new CreateClassScheduleDto
        {
            TeacherId = "missing-teacher-id",
            Location = "Main mat",
            DayOfWeek = "Monday",
            TimeOfDay = "19:00"
        };

        var (schedule, errors) = await service.CreateScheduleAsync(dto);

        Assert.Null(schedule);
        Assert.Contains("Teacher not found.", errors);
    }

    [Fact]
    public async Task CreateScheduleAsync_WithValidData_PersistsActiveSchedule()
    {
        var teacher = await SeedTeacherAsync();

        using var context = _db.CreateContext();
        var service = new ScheduleService(context);
        var dto = new CreateClassScheduleDto
        {
            TeacherId = teacher.Id,
            Location = "Main mat",
            DayOfWeek = "Wednesday",
            TimeOfDay = "19:00"
        };

        var (schedule, errors) = await service.CreateScheduleAsync(dto);

        Assert.Empty(errors);
        Assert.NotNull(schedule);
        Assert.Equal("Wednesday", schedule.DayOfWeek);
        Assert.Equal("19:00", schedule.TimeOfDay);
        Assert.Equal("John Danaher", schedule.TeacherName);
        Assert.True(schedule.IsActive);

        using var assertContext = _db.CreateContext();
        var saved = await assertContext.ClassSchedules.SingleAsync(s => s.Id == schedule.Id);
        Assert.Equal(DayOfWeek.Wednesday, saved.DayOfWeek);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task DeactivateScheduleAsync_WhenScheduleExists_SetsIsActiveFalse()
    {
        var teacher = await SeedTeacherAsync();
        var seeded = await SeedScheduleAsync(teacher.Id);

        using var context = _db.CreateContext();
        var service = new ScheduleService(context);

        var result = await service.DeactivateScheduleAsync(seeded.Id);

        Assert.True(result);
        using var assertContext = _db.CreateContext();
        var saved = await assertContext.ClassSchedules.SingleAsync(s => s.Id == seeded.Id);
        Assert.False(saved.IsActive);
    }

    [Fact]
    public async Task DeactivateScheduleAsync_WhenScheduleDoesNotExist_ReturnsFalse()
    {
        using var context = _db.CreateContext();
        var service = new ScheduleService(context);

        Assert.False(await service.DeactivateScheduleAsync(999));
    }

    [Fact]
    public async Task EnsureSessionsGeneratedAsync_GeneratesOneSessionPerWeekInsideTheWindow()
    {
        var teacher = await SeedTeacherAsync();
        // Use today's weekday so the expected count is deterministic: the 4-week window
        // is inclusive on both ends, giving sessions on day 0, 7, 14, 21 and 28.
        var seeded = await SeedScheduleAsync(teacher.Id, DateTime.UtcNow.DayOfWeek);

        using var context = _db.CreateContext();
        var service = new ScheduleService(context);

        await service.EnsureSessionsGeneratedAsync();

        using var assertContext = _db.CreateContext();
        Assert.Equal(5, await assertContext.Classes.CountAsync(c => c.ClassScheduleId == seeded.Id));
    }

    [Fact]
    public async Task EnsureSessionsGeneratedAsync_WhenCalledTwice_DoesNotDuplicateSessions()
    {
        var teacher = await SeedTeacherAsync();
        var seeded = await SeedScheduleAsync(teacher.Id, DateTime.UtcNow.DayOfWeek);

        using var context = _db.CreateContext();
        var service = new ScheduleService(context);

        await service.EnsureSessionsGeneratedAsync();
        var countAfterFirstRun = await _db.CreateContext().Classes.CountAsync(c => c.ClassScheduleId == seeded.Id);

        using var secondContext = _db.CreateContext();
        await new ScheduleService(secondContext).EnsureSessionsGeneratedAsync();

        using var assertContext = _db.CreateContext();
        Assert.Equal(countAfterFirstRun, await assertContext.Classes.CountAsync(c => c.ClassScheduleId == seeded.Id));
    }

    [Fact]
    public async Task EnsureSessionsGeneratedAsync_SkipsInactiveSchedules()
    {
        var teacher = await SeedTeacherAsync();
        var seeded = await SeedScheduleAsync(teacher.Id, DateTime.UtcNow.DayOfWeek, isActive: false);

        using var context = _db.CreateContext();
        var service = new ScheduleService(context);

        await service.EnsureSessionsGeneratedAsync();

        using var assertContext = _db.CreateContext();
        Assert.Equal(0, await assertContext.Classes.CountAsync(c => c.ClassScheduleId == seeded.Id));
    }
}
