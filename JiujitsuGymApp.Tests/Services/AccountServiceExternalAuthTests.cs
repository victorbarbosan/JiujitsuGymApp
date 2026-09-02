using System.Security.Claims;
using JiujitsuGymApp.Data;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using JiujitsuGymApp.Tests.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JiujitsuGymApp.Tests.Services;

public sealed class AccountServiceExternalAuthTests : IDisposable
{
    private readonly SqliteTestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private (AccountService service, UserManager<User> users) CreateService()
    {
        var context = _db.CreateContext();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddHttpContextAccessor();
        services.AddAuthentication()
            .AddCookie(IdentityConstants.ApplicationScheme);
        services.AddSingleton(context);
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        services.AddScoped<UserService>();
        services.AddScoped<AccountService>();

        context.Roles.Add(new IdentityRole
        {
            Name = "Member",
            NormalizedName = "MEMBER"
        });
        context.SaveChanges();

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            RequestServices = scope.ServiceProvider
        };

        return (scope.ServiceProvider.GetRequiredService<AccountService>(),
                scope.ServiceProvider.GetRequiredService<UserManager<User>>());
    }

    private static ExternalLoginInfo CreateExternalLoginInfo(
        string? email = "alex@example.com",
        string givenName = "Alex",
        string surname = "Example")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "google-user-123"),
            new(ClaimTypes.GivenName, givenName),
            new(ClaimTypes.Surname, surname)
        };

        if (email is not null)
            claims.Add(new Claim(ClaimTypes.Email, email));

        return new ExternalLoginInfo(
            new ClaimsPrincipal(new ClaimsIdentity(claims, "Google")),
            "Google",
            "google-user-123",
            "Google");
    }

    [Fact]
    public async Task ProvisionExternalUser_WithoutEmail_ReturnsValidationError()
    {
        var (service, _) = CreateService();

        var errors = await service.ProvisionExternalUserAsync(CreateExternalLoginInfo(email: null));

        var error = Assert.Single(errors);
        Assert.Equal("The external provider did not supply an email address.", error);
    }

    [Fact]
    public async Task ProvisionExternalUser_NewUser_CreatesMemberAndLinksLogin()
    {
        var (service, users) = CreateService();
        var info = CreateExternalLoginInfo();

        var errors = await service.ProvisionExternalUserAsync(info);

        Assert.Empty(errors);
        var user = await users.FindByEmailAsync("alex@example.com");
        Assert.NotNull(user);
        Assert.Equal("Alex", user.FirstName);
        Assert.Equal("Example", user.LastName);
        Assert.Equal(BeltColor.White, user.Belt);
        Assert.Contains("Member", await users.GetRolesAsync(user));
        Assert.NotNull(await users.FindByLoginAsync("Google", "google-user-123"));
        Assert.NotNull(user.LastLoginAt);
    }

    [Fact]
    public async Task ProvisionExternalUser_ExistingEmail_LinksExistingUserWithoutCreatingAnother()
    {
        var (service, users) = CreateService();
        var existingUser = new User
        {
            FirstName = "Existing",
            LastName = "Member",
            UserName = "alex@example.com",
            Email = "alex@example.com",
            Belt = BeltColor.Blue
        };
        var createResult = await users.CreateAsync(existingUser, "Existing1");
        Assert.True(createResult.Succeeded);

        var errors = await service.ProvisionExternalUserAsync(CreateExternalLoginInfo());

        Assert.Empty(errors);
        Assert.Equal(existingUser.Id, (await users.FindByLoginAsync("Google", "google-user-123"))?.Id);
        Assert.Equal(existingUser.Id, (await users.FindByEmailAsync("alex@example.com"))?.Id);
        Assert.Equal(BeltColor.Blue, (await users.FindByEmailAsync("alex@example.com"))?.Belt);
    }

    [Fact]
    public async Task ProvisionExternalUser_WhenLoginAlreadyLinked_ReturnsIdentityError()
    {
        var (service, users) = CreateService();
        var user = new User
        {
            FirstName = "Existing",
            LastName = "Member",
            UserName = "alex@example.com",
            Email = "alex@example.com"
        };
        var createResult = await users.CreateAsync(user, "Existing1");
        Assert.True(createResult.Succeeded);
        var firstLink = await users.AddLoginAsync(user, CreateExternalLoginInfo());
        Assert.True(firstLink.Succeeded);

        var errors = await service.ProvisionExternalUserAsync(CreateExternalLoginInfo());

        var error = Assert.Single(errors);
        Assert.Equal("A user with this login already exists.", error);
    }
}