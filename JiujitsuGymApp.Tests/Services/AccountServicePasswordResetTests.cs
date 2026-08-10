using JiujitsuGymApp.Data;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using JiujitsuGymApp.Tests.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JiujitsuGymApp.Tests.Services;

public sealed class AccountServicePasswordResetTests : IDisposable
{
    private readonly SqliteTestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    /// <summary>
    /// Builds the real AccountService over the real Identity stack (data
    /// protection token provider included), so these tests exercise the same
    /// token generation and validation the running app performs.
    /// </summary>
    private (AccountService service, UserManager<User> users) CreateService()
    {
        var context = _db.CreateContext();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddHttpContextAccessor();
        services.AddAuthentication();
        services.AddSingleton(context);
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddIdentityCore<User>(options =>
        {
            // Mirror Program.cs so a password the app would accept is also
            // accepted here.
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

        var provider = services.BuildServiceProvider().CreateScope().ServiceProvider;
        return (provider.GetRequiredService<AccountService>(),
                provider.GetRequiredService<UserManager<User>>());
    }

    private static async Task<User> CreateUserAsync(UserManager<User> users, string email, string password)
    {
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            UserName = email,
            Email = email,
            Belt = BeltColor.White,
            CreatedAt = DateTime.UtcNow
        };
        var result = await users.CreateAsync(user, password);
        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));
        return user;
    }

    [Fact]
    public async Task GeneratePasswordResetToken_UnknownEmail_ReturnsNull()
    {
        var (service, _) = CreateService();

        Assert.Null(await service.GeneratePasswordResetTokenAsync("nobody@example.com"));
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ChangesThePassword()
    {
        var (service, users) = CreateService();
        var user = await CreateUserAsync(users, "member@example.com", "OldPass1");

        var token = await service.GeneratePasswordResetTokenAsync("member@example.com");
        Assert.NotNull(token);

        var errors = await service.ResetPasswordAsync("member@example.com", token!, "NewPass1");

        Assert.Empty(errors);
        Assert.True(await users.CheckPasswordAsync(user, "NewPass1"));
        Assert.False(await users.CheckPasswordAsync(user, "OldPass1"));
    }

    [Fact]
    public async Task ResetPassword_WithTamperedToken_FailsAndKeepsOldPassword()
    {
        var (service, users) = CreateService();
        var user = await CreateUserAsync(users, "member@example.com", "OldPass1");

        var errors = await service.ResetPasswordAsync("member@example.com", "not-a-real-token", "NewPass1");

        Assert.NotEmpty(errors);
        Assert.True(await users.CheckPasswordAsync(user, "OldPass1"));
    }

    [Fact]
    public async Task ResetPassword_TokenCannotBeUsedTwice()
    {
        var (service, users) = CreateService();
        await CreateUserAsync(users, "member@example.com", "OldPass1");

        var token = await service.GeneratePasswordResetTokenAsync("member@example.com");
        Assert.Empty(await service.ResetPasswordAsync("member@example.com", token!, "NewPass1"));

        // The security stamp changed with the first reset, so replaying the
        // same link must not let anyone change the password again.
        var errors = await service.ResetPasswordAsync("member@example.com", token!, "OtherPass1");
        Assert.NotEmpty(errors);
    }

    [Fact]
    public async Task ResetPassword_UnknownEmail_ReturnsGenericError()
    {
        var (service, _) = CreateService();

        var errors = await service.ResetPasswordAsync("nobody@example.com", "whatever", "NewPass1");

        var message = Assert.Single(errors);
        // The wording must not confirm that the account doesn't exist.
        Assert.DoesNotContain("exist", message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("found", message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Identity tokens are Base64 and routinely contain '+' and '/', which a
    /// query string mangles unless every hop encodes properly ('+' decoding
    /// back to a space is the classic way reset links break). This drives the
    /// token through the same encoder Url.Action uses and the same parser the
    /// incoming request uses, then spends it - so a regression shows up here
    /// rather than as a link that only fails for some users.
    /// </summary>
    [Fact]
    public async Task ResetPassword_TokenSurvivesTheQueryStringRoundTrip()
    {
        var (service, users) = CreateService();
        var user = await CreateUserAsync(users, "member@example.com", "OldPass1");

        var token = await service.GeneratePasswordResetTokenAsync("member@example.com");

        var url = QueryHelpers.AddQueryString(
            "https://gym.example.com/Account/ResetPassword",
            new Dictionary<string, string?>
            {
                ["email"] = "member@example.com",
                ["token"] = token
            });

        var parsed = QueryHelpers.ParseQuery(new Uri(url).Query);
        var roundTrippedToken = parsed["token"].ToString();
        var roundTrippedEmail = parsed["email"].ToString();

        Assert.Equal(token, roundTrippedToken);

        var errors = await service.ResetPasswordAsync(roundTrippedEmail, roundTrippedToken, "NewPass1");

        Assert.Empty(errors);
        Assert.True(await users.CheckPasswordAsync(user, "NewPass1"));
    }

    [Fact]
    public async Task ResetPassword_RejectsPasswordBreakingTheRules()
    {
        var (service, users) = CreateService();
        var user = await CreateUserAsync(users, "member@example.com", "OldPass1");

        var token = await service.GeneratePasswordResetTokenAsync("member@example.com");
        var errors = await service.ResetPasswordAsync("member@example.com", token!, "weak");

        Assert.NotEmpty(errors);
        Assert.True(await users.CheckPasswordAsync(user, "OldPass1"));
    }
}
