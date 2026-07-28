using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using JiujitsuGymApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Tests.Services;

public sealed class ClassServiceTests : IDisposable
{
    // A fixed "now" keeps every date-window assertion deterministic no matter when the tests run.
    private static readonly DateTime BaseDate = new(2026, 8, 3, 19, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<User> SeedUserAsync(string firstName = "John", string lastName = "Danaher")
    {
        using var context = _db.CreateContext();
        var email = $"{Guid.NewGuid():N}@gym.com";
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = firstName,
            LastName = lastName,
            UserName = email,
            Email = email
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private async Task<Class> SeedClassAsync(string teacherId, DateTime dateTime, DateTime? deletedAt = null)
    {
        using var context = _db.CreateContext();
        var cls = new Class
        {
            TeacherId = teacherId,
            Location = "Main mat",
            DateTime = dateTime,
            DeletedAt = deletedAt
        };
        context.Classes.Add(cls);
        await context.SaveChangesAsync();
        return cls;
    }

    private async Task SeedAttendanceAsync(int classId, string userId)
    {
        using var context = _db.CreateContext();
        context.Attendances.Add(new Attendance { ClassId = classId, UserId = userId });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetClassEventsAsync_ReturnsOnlyClassesInsideTheWindowOrderedByDate()
    {
        var teacher = await SeedUserAsync();
        var from = BaseDate;
        var to = BaseDate.AddDays(7);
        await SeedClassAsync(teacher.Id, BaseDate.AddDays(-1));      // before the window
        var later = await SeedClassAsync(teacher.Id, BaseDate.AddDays(3));
        var atStart = await SeedClassAsync(teacher.Id, from);        // boundary: included
        await SeedClassAsync(teacher.Id, to);                        // boundary: excluded (half-open window)

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        var events = await service.GetClassEventsAsync(from, to, userId: null);

        Assert.Equal(new[] { atStart.Id, later.Id }, events.Select(e => e.Id));
    }

    [Fact]
    public async Task GetClassEventsAsync_ExcludesSoftDeletedClasses()
    {
        var teacher = await SeedUserAsync();
        await SeedClassAsync(teacher.Id, BaseDate, deletedAt: BaseDate.AddDays(-1));

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        var events = await service.GetClassEventsAsync(BaseDate.AddDays(-7), BaseDate.AddDays(7), userId: null);

        Assert.Empty(events);
    }

    [Fact]
    public async Task GetClassEventsAsync_ReportsAttendanceCountAndCheckedInForCurrentUser()
    {
        var teacher = await SeedUserAsync();
        var student = await SeedUserAsync("Ana", "Silva");
        var otherStudent = await SeedUserAsync("Bruno", "Costa");
        var attended = await SeedClassAsync(teacher.Id, BaseDate);
        var notAttended = await SeedClassAsync(teacher.Id, BaseDate.AddDays(1));
        await SeedAttendanceAsync(attended.Id, student.Id);
        await SeedAttendanceAsync(attended.Id, otherStudent.Id);
        await SeedAttendanceAsync(notAttended.Id, otherStudent.Id);

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        var events = await service.GetClassEventsAsync(BaseDate, BaseDate.AddDays(7), student.Id);

        var first = events.Single(e => e.Id == attended.Id);
        Assert.Equal("John Danaher", first.TeacherName);
        Assert.Equal(2, first.AttendanceCount);
        Assert.True(first.CheckedIn);
        Assert.False(events.Single(e => e.Id == notAttended.Id).CheckedIn);
    }

    [Fact]
    public async Task GetClassEventsAsync_WithAnonymousUser_MarksNothingCheckedIn()
    {
        var teacher = await SeedUserAsync();
        var student = await SeedUserAsync("Ana", "Silva");
        var cls = await SeedClassAsync(teacher.Id, BaseDate);
        await SeedAttendanceAsync(cls.Id, student.Id);

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        var events = await service.GetClassEventsAsync(BaseDate, BaseDate.AddDays(1), userId: null);

        Assert.False(events.Single().CheckedIn);
    }

    [Fact]
    public async Task GetTotalAttendedAsync_CountsOnlyTheGivenUsersAttendances()
    {
        var teacher = await SeedUserAsync();
        var student = await SeedUserAsync("Ana", "Silva");
        var otherStudent = await SeedUserAsync("Bruno", "Costa");
        var first = await SeedClassAsync(teacher.Id, BaseDate);
        var second = await SeedClassAsync(teacher.Id, BaseDate.AddDays(1));
        await SeedAttendanceAsync(first.Id, student.Id);
        await SeedAttendanceAsync(second.Id, student.Id);
        await SeedAttendanceAsync(first.Id, otherStudent.Id);

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        Assert.Equal(2, await service.GetTotalAttendedAsync(student.Id));
    }

    [Fact]
    public async Task CheckInAsync_WithValidClassAndUser_PersistsAttendance()
    {
        var teacher = await SeedUserAsync();
        var student = await SeedUserAsync("Ana", "Silva");
        var cls = await SeedClassAsync(teacher.Id, BaseDate);

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        var result = await service.CheckInAsync(cls.Id, student.Id);

        Assert.Equal(CheckInResult.Success, result);
        using var assertContext = _db.CreateContext();
        var saved = await assertContext.Attendances.SingleAsync(a => a.ClassId == cls.Id && a.UserId == student.Id);
        Assert.NotEqual(default, saved.CheckedInAt);
    }

    [Fact]
    public async Task CheckInAsync_WhenClassDoesNotExist_ReturnsNotFound()
    {
        var student = await SeedUserAsync("Ana", "Silva");

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        Assert.Equal(CheckInResult.NotFound, await service.CheckInAsync(999, student.Id));
    }

    [Fact]
    public async Task CheckInAsync_WhenClassIsSoftDeleted_ReturnsNotFoundAndSavesNothing()
    {
        var teacher = await SeedUserAsync();
        var student = await SeedUserAsync("Ana", "Silva");
        var cls = await SeedClassAsync(teacher.Id, BaseDate, deletedAt: BaseDate);

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        var result = await service.CheckInAsync(cls.Id, student.Id);

        Assert.Equal(CheckInResult.NotFound, result);
        using var assertContext = _db.CreateContext();
        Assert.Equal(0, await assertContext.Attendances.CountAsync());
    }

    [Fact]
    public async Task CheckInAsync_WhenAlreadyCheckedIn_ReturnsAlreadyCheckedInAndDoesNotDuplicate()
    {
        var teacher = await SeedUserAsync();
        var student = await SeedUserAsync("Ana", "Silva");
        var cls = await SeedClassAsync(teacher.Id, BaseDate);
        await SeedAttendanceAsync(cls.Id, student.Id);

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        var result = await service.CheckInAsync(cls.Id, student.Id);

        Assert.Equal(CheckInResult.AlreadyCheckedIn, result);
        using var assertContext = _db.CreateContext();
        Assert.Equal(1, await assertContext.Attendances.CountAsync(a => a.ClassId == cls.Id && a.UserId == student.Id));
    }

    [Fact]
    public async Task UndoCheckInAsync_WhenCheckedIn_RemovesAttendance()
    {
        var teacher = await SeedUserAsync();
        var student = await SeedUserAsync("Ana", "Silva");
        var cls = await SeedClassAsync(teacher.Id, BaseDate);
        await SeedAttendanceAsync(cls.Id, student.Id);

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        var result = await service.UndoCheckInAsync(cls.Id, student.Id);

        Assert.Equal(CheckInResult.Success, result);
        using var assertContext = _db.CreateContext();
        Assert.Equal(0, await assertContext.Attendances.CountAsync());
    }

    [Fact]
    public async Task UndoCheckInAsync_WhenNotCheckedIn_ReturnsNotFound()
    {
        var teacher = await SeedUserAsync();
        var student = await SeedUserAsync("Ana", "Silva");
        var cls = await SeedClassAsync(teacher.Id, BaseDate);

        using var context = _db.CreateContext();
        var service = new ClassService(context);

        Assert.Equal(CheckInResult.NotFound, await service.UndoCheckInAsync(cls.Id, student.Id));
    }

    [Fact]
    public async Task Database_RejectsDuplicateAttendanceThroughUniqueIndex()
    {
        // The service guards against double check-ins with a query, but two simultaneous
        // requests could both pass that check. The (ClassId, UserId) unique index is the
        // real safety net, and only a relational test database can prove it fires.
        var teacher = await SeedUserAsync();
        var student = await SeedUserAsync("Ana", "Silva");
        var cls = await SeedClassAsync(teacher.Id, BaseDate);
        await SeedAttendanceAsync(cls.Id, student.Id);

        await Assert.ThrowsAsync<DbUpdateException>(() => SeedAttendanceAsync(cls.Id, student.Id));
    }
}
