using JiujitsuGymApp.Data;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using JiujitsuGymApp.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace JiujitsuGymApp.Tests.Services;

public sealed class DemoDataServiceTests : IDisposable
{
    private readonly SqliteTestDatabase _db = new();

    public DemoDataServiceTests()
    {
        // AddToRoleAsync and GetUsersInRoleAsync both throw on a role that does
        // not exist, so stand in for what Program.cs seeds at startup.
        using var context = _db.CreateContext();
        foreach (var role in new[] { "Admin", "Member", "Teacher" })
            context.Roles.Add(new IdentityRole(role) { NormalizedName = role.ToUpperInvariant() });
        context.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    /// <summary>
    /// Builds a service over a context the UserManager also writes through, so
    /// the seed's transaction covers the Identity writes the way it does in the
    /// running app.
    /// </summary>
    private (DemoDataService service, ApplicationDbContext context, UserManager<User> users) CreateService()
    {
        var context = _db.CreateContext();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(context);
        services.AddIdentityCore<User>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        var userManager = services.BuildServiceProvider().GetRequiredService<UserManager<User>>();
        return (new DemoDataService(context, userManager, NullLogger<DemoDataService>.Instance), context, userManager);
    }

    // The roster, timetable and product list are meant to be edited, so these
    // assert the shape and the internal consistency of the seed rather than the
    // specific counts - otherwise renaming an instructor or adding a class slot
    // fails the suite for no reason.
    [Fact]
    public async Task SeedAsync_OnEmptyDatabase_CreatesTheFullGym()
    {
        var (service, context, _) = CreateService();

        var (result, errors) = await service.SeedAsync();

        Assert.Empty(errors);
        Assert.NotNull(result);
        Assert.True(result.Status.IsSeeded);
        Assert.True(result.Status.Teachers > 0);
        Assert.True(result.Status.Members > 0);
        Assert.True(result.Status.Schedules > 0);
        Assert.True(result.Status.Products > 0);
        Assert.True(result.Status.Classes > 0);
        Assert.True(result.Status.Attendances > 0);

        using var assert = _db.CreateContext();
        // The status the admin UI shows has to match the database exactly - that
        // is the whole basis for trusting the purge confirmation dialog.
        Assert.Equal(result.Status.Teachers + result.Status.Members, await assert.Users.CountAsync());
        Assert.Equal(result.Status.Schedules, await assert.ClassSchedules.CountAsync());
        Assert.Equal(result.Status.Classes, await assert.Classes.CountAsync());
        Assert.Equal(result.Status.Attendances, await assert.Attendances.CountAsync());
        Assert.Equal(result.Status.Products, await assert.Products.CountAsync());

        // Every class must hang off the schedule that produced it, otherwise the
        // purge's teacher-based reachability would miss it.
        Assert.Equal(0, await assert.Classes.CountAsync(c => c.ClassScheduleId == null));
        context.Dispose();
    }

    /// <summary>
    /// The timetable references instructors by index into the roster array, so a
    /// hand-edit that adds a slot pointing past the end of the roster would throw
    /// at runtime. Seeding at all proves the indexes resolve; this pins that every
    /// slot landed on a real instructor rather than some other account.
    /// </summary>
    [Fact]
    public async Task SeedAsync_PointsEverySlotAtASeededInstructor()
    {
        var (service, context, users) = CreateService();
        await service.SeedAsync();

        var teacherIds = (await users.GetUsersInRoleAsync("Teacher")).Select(t => t.Id).ToHashSet();

        using var assert = _db.CreateContext();
        var scheduleTeachers = await assert.ClassSchedules.Select(s => s.TeacherId).ToListAsync();
        var classTeachers = await assert.Classes.Select(c => c.TeacherId).ToListAsync();

        Assert.NotEmpty(scheduleTeachers);
        Assert.All(scheduleTeachers, id => Assert.Contains(id, teacherIds));
        Assert.All(classTeachers, id => Assert.Contains(id, teacherIds));
        context.Dispose();
    }

    [Fact]
    public async Task SeedAsync_GivesEveryAccountTheDocumentedPassword()
    {
        var (service, context, users) = CreateService();
        await service.SeedAsync();

        using var assert = _db.CreateContext();
        var seeded = await assert.Users.ToListAsync();

        // Pins the shared-hash optimisation in CreateUsersAsync: reusing one
        // hash across accounts must still verify against the real password.
        foreach (var user in seeded)
        {
            var verified = users.PasswordHasher.VerifyHashedPassword(
                user, user.PasswordHash!, DemoDataService.DemoPassword);
            Assert.NotEqual(PasswordVerificationResult.Failed, verified);
        }

        context.Dispose();
    }

    [Fact]
    public async Task SeedAsync_GivesEveryAccountADistinctEmailOnTheDemoDomain()
    {
        var (service, context, _) = CreateService();
        await service.SeedAsync();

        using var assert = _db.CreateContext();
        var emails = await assert.Users.Select(u => u.Email!).ToListAsync();

        Assert.Equal(emails.Count, emails.Distinct().Count());
        Assert.All(emails, e => Assert.EndsWith("@" + DemoDataService.DemoEmailDomain, e));
        context.Dispose();
    }

    [Fact]
    public async Task SeedAsync_CreatesClassesOnBothSidesOfToday()
    {
        var (service, context, _) = CreateService();
        await service.SeedAsync();

        using var assert = _db.CreateContext();
        var now = DateTime.UtcNow;

        // History is what the profile totals count; the future is what the
        // class calendar opens onto. Both have to be there.
        Assert.True(await assert.Classes.AnyAsync(c => c.DateTime < now));
        Assert.True(await assert.Classes.AnyAsync(c => c.DateTime >= now));
        // Attendance is only plausible for classes that already happened.
        Assert.Equal(0, await assert.Attendances.CountAsync(a => a.Class.DateTime >= now));
        context.Dispose();
    }

    [Fact]
    public async Task SeedAsync_AssignsTheTeacherAndMemberRoles()
    {
        var (service, context, users) = CreateService();
        await service.SeedAsync();

        var teachers = await users.GetUsersInRoleAsync("Teacher");
        var members = await users.GetUsersInRoleAsync("Member");

        Assert.NotEmpty(teachers);
        Assert.NotEmpty(members);
        // The seed must never mint an administrator - that is the bootstrap
        // account's job, and a purgeable admin would be a way to lose access.
        Assert.Empty(await users.GetUsersInRoleAsync("Admin"));

        using var assert = _db.CreateContext();
        Assert.Equal(teachers.Count + members.Count, await assert.Users.CountAsync());
        context.Dispose();
    }

    [Fact]
    public async Task SeedAsync_WhenAlreadySeeded_ReturnsErrorAndAddsNothing()
    {
        var (first, firstContext, _) = CreateService();
        await first.SeedAsync();
        firstContext.Dispose();

        var countBefore = await _db.CreateContext().Users.CountAsync();

        var (second, secondContext, _) = CreateService();
        var (result, errors) = await second.SeedAsync();

        Assert.Null(result);
        Assert.Contains("Demo data is already present. Remove it first if you want to reseed.", errors);

        using var assert = _db.CreateContext();
        Assert.Equal(countBefore, await assert.Users.CountAsync());
        secondContext.Dispose();
    }

    [Fact]
    public async Task PurgeAsync_RemovesEverythingTheSeedCreated()
    {
        var (seedService, seedContext, _) = CreateService();
        await seedService.SeedAsync();
        seedContext.Dispose();

        var (purgeService, purgeContext, _) = CreateService();
        var result = await purgeService.PurgeAsync();

        Assert.False(result.Status.IsSeeded);

        using var assert = _db.CreateContext();
        Assert.Equal(0, await assert.Users.CountAsync());
        Assert.Equal(0, await assert.Classes.CountAsync());
        Assert.Equal(0, await assert.ClassSchedules.CountAsync());
        Assert.Equal(0, await assert.Attendances.CountAsync());
        Assert.Equal(0, await assert.Products.CountAsync());
        // Roles are startup state, not demo data, and must survive.
        Assert.Equal(3, await assert.Roles.CountAsync());
        purgeContext.Dispose();
    }

    [Fact]
    public async Task PurgeAsync_LeavesRealDataUntouched()
    {
        var (seedService, seedContext, _) = CreateService();
        await seedService.SeedAsync();
        seedContext.Dispose();

        // A real member who has been checking in to the seeded timetable - the
        // case where a naive "delete demo rows" would either miss the join row
        // or take the real account down with it.
        string realUserId;
        using (var arrange = _db.CreateContext())
        {
            var real = new User
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Victor",
                LastName = "Barbosa",
                UserName = "victor@nexusbjj.com",
                Email = "victor@nexusbjj.com"
            };
            arrange.Users.Add(real);
            arrange.Products.Add(new Product
            {
                Name = "Real Membership",
                Price = 60m,
                Description = "A genuine product",
                Category = "membership"
            });
            await arrange.SaveChangesAsync();

            var demoClassId = await arrange.Classes.MinAsync(c => c.Id);
            arrange.Attendances.Add(new Attendance { ClassId = demoClassId, UserId = real.Id });
            arrange.Classes.Add(new Class
            {
                TeacherId = real.Id,
                Location = "Real Seminar",
                DateTime = DateTime.UtcNow.AddDays(3)
            });
            await arrange.SaveChangesAsync();
            realUserId = real.Id;
        }

        var (purgeService, purgeContext, _) = CreateService();
        await purgeService.PurgeAsync();

        using var assert = _db.CreateContext();
        Assert.Equal(realUserId, await assert.Users.Select(u => u.Id).SingleAsync());
        Assert.Equal("Real Seminar", await assert.Classes.Select(c => c.Location).SingleAsync());
        Assert.Equal("Real Membership", await assert.Products.Select(p => p.Name).SingleAsync());
        // The real member's check-in pointed at a demo class, so it has to go
        // with it rather than survive as a dangling row.
        Assert.Equal(0, await assert.Attendances.CountAsync());
        purgeContext.Dispose();
    }

    [Fact]
    public async Task PurgeAsync_OnACleanDatabase_IsANoOp()
    {
        var (service, context, _) = CreateService();

        var result = await service.PurgeAsync();

        Assert.False(result.Status.IsSeeded);
        using var assert = _db.CreateContext();
        Assert.Equal(0, await assert.Users.CountAsync());
        context.Dispose();
    }

    [Fact]
    public async Task SeedAsync_AfterAPurge_Succeeds()
    {
        var (first, firstContext, _) = CreateService();
        await first.SeedAsync();
        firstContext.Dispose();

        var (purge, purgeContext, _) = CreateService();
        await purge.PurgeAsync();
        purgeContext.Dispose();

        var (second, secondContext, _) = CreateService();
        var (result, errors) = await second.SeedAsync();

        Assert.Empty(errors);
        Assert.NotNull(result);
        Assert.True(result.Status.IsSeeded);
        Assert.True(result.Status.Teachers > 0);
        Assert.True(result.Status.Members > 0);
        secondContext.Dispose();
    }

    [Fact]
    public async Task GetStatusAsync_OnACleanDatabase_ReportsNothingToRemove()
    {
        var (service, context, _) = CreateService();

        var status = await service.GetStatusAsync();

        Assert.False(status.IsSeeded);
        Assert.Equal(0, status.Teachers);
        Assert.Equal(0, status.Members);
        Assert.Equal(0, status.Classes);
        Assert.Equal(0, status.Attendances);
        Assert.Equal(DemoDataService.DemoPassword, status.DemoPassword);
        Assert.Equal(DemoDataService.DemoEmailDomain, status.DemoEmailDomain);
        context.Dispose();
    }
}
