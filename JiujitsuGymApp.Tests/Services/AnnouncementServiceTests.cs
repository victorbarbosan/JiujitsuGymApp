using JiujitsuGymApp.Data;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using JiujitsuGymApp.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JiujitsuGymApp.Tests.Services;

public sealed class AnnouncementServiceTests : IDisposable
{
    private readonly SqliteTestDatabase _db = new();

    public AnnouncementServiceTests()
    {
        using var context = _db.CreateContext();
        foreach (var role in new[] { "Admin", "Member", "Teacher" })
            context.Roles.Add(new IdentityRole(role) { NormalizedName = role.ToUpperInvariant() });
        context.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private (AnnouncementService service, ApplicationDbContext context, UserManager<User> users) CreateService()
    {
        var context = _db.CreateContext();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(context);
        services.AddIdentityCore<User>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        var userManager = services.BuildServiceProvider().GetRequiredService<UserManager<User>>();
        return (new AnnouncementService(context, userManager), context, userManager);
    }

    private static async Task<User> CreateUserAsync(UserManager<User> users, string userName, string role)
    {
        var user = new User { UserName = userName, Email = $"{userName}@test.local", FirstName = userName, LastName = "Test" };
        var createResult = await users.CreateAsync(user, "Password1!");
        Assert.True(createResult.Succeeded);
        await users.AddToRoleAsync(user, role);
        return user;
    }

    [Fact]
    public async Task CreateReplyAsync_OnRootAnnouncement_Succeeds()
    {
        var (service, _, users) = CreateService();
        var teacher = await CreateUserAsync(users, "teacher1", "Teacher");
        var student = await CreateUserAsync(users, "student1", "Member");

        var announcementId = await service.CreateAnnouncementAsync(teacher.Id, "Class canceled tomorrow");
        var result = await service.CreateReplyAsync(announcementId, student.Id, "Got it, thanks!");

        Assert.Equal(ReplyResult.Success, result);
    }

    [Fact]
    public async Task CreateReplyAsync_OnAReply_IsRejected()
    {
        var (service, _, users) = CreateService();
        var teacher = await CreateUserAsync(users, "teacher1", "Teacher");
        var student = await CreateUserAsync(users, "student1", "Member");

        var announcementId = await service.CreateAnnouncementAsync(teacher.Id, "Class canceled tomorrow");
        await service.CreateReplyAsync(announcementId, student.Id, "Got it, thanks!");

        var (service2, context, _) = CreateService();
        var replyId = await context.Announcements.Where(a => a.ParentId == announcementId).Select(a => a.Id).FirstAsync();

        var result = await service2.CreateReplyAsync(replyId, student.Id, "Reply to a reply");

        Assert.Equal(ReplyResult.CannotReplyToReply, result);
    }

    [Fact]
    public async Task DeleteReplyAsync_AsAdmin_Succeeds()
    {
        var (service, _, users) = CreateService();
        var teacher = await CreateUserAsync(users, "teacher1", "Teacher");
        var student = await CreateUserAsync(users, "student1", "Member");
        var admin = await CreateUserAsync(users, "admin1", "Admin");

        var announcementId = await service.CreateAnnouncementAsync(teacher.Id, "Announcement");
        await service.CreateReplyAsync(announcementId, student.Id, "A reply");

        var (service2, context, _) = CreateService();
        var replyId = await context.Announcements.Where(a => a.ParentId == announcementId).Select(a => a.Id).FirstAsync();

        var result = await service2.DeleteReplyAsync(replyId, admin.Id);

        Assert.Equal(DeleteReplyResult.Success, result);
    }

    [Fact]
    public async Task DeleteReplyAsync_AsOwningTeacher_Succeeds()
    {
        var (service, _, users) = CreateService();
        var teacher = await CreateUserAsync(users, "teacher1", "Teacher");
        var student = await CreateUserAsync(users, "student1", "Member");

        var announcementId = await service.CreateAnnouncementAsync(teacher.Id, "Announcement");
        await service.CreateReplyAsync(announcementId, student.Id, "A reply");

        var (service2, context, _) = CreateService();
        var replyId = await context.Announcements.Where(a => a.ParentId == announcementId).Select(a => a.Id).FirstAsync();

        var result = await service2.DeleteReplyAsync(replyId, teacher.Id);

        Assert.Equal(DeleteReplyResult.Success, result);
    }

    [Fact]
    public async Task DeleteReplyAsync_AsUnrelatedTeacher_IsForbidden()
    {
        var (service, _, users) = CreateService();
        var teacher = await CreateUserAsync(users, "teacher1", "Teacher");
        var otherTeacher = await CreateUserAsync(users, "teacher2", "Teacher");
        var student = await CreateUserAsync(users, "student1", "Member");

        var announcementId = await service.CreateAnnouncementAsync(teacher.Id, "Announcement");
        await service.CreateReplyAsync(announcementId, student.Id, "A reply");

        var (service2, context, _) = CreateService();
        var replyId = await context.Announcements.Where(a => a.ParentId == announcementId).Select(a => a.Id).FirstAsync();

        var result = await service2.DeleteReplyAsync(replyId, otherTeacher.Id);

        Assert.Equal(DeleteReplyResult.Forbidden, result);
    }

    [Fact]
    public async Task GetAnnouncementsAsync_ExcludesDeletedReplies()
    {
        var (service, _, users) = CreateService();
        var teacher = await CreateUserAsync(users, "teacher1", "Teacher");
        var student = await CreateUserAsync(users, "student1", "Member");

        var announcementId = await service.CreateAnnouncementAsync(teacher.Id, "Announcement");
        await service.CreateReplyAsync(announcementId, student.Id, "A reply");

        var (service2, context, _) = CreateService();
        var replyId = await context.Announcements.Where(a => a.ParentId == announcementId).Select(a => a.Id).FirstAsync();
        await service2.DeleteReplyAsync(replyId, teacher.Id);

        var (service3, _, _) = CreateService();
        var announcements = await service3.GetAnnouncementsAsync(student.Id);

        Assert.Single(announcements);
        Assert.Empty(announcements[0].Replies);
    }
}
