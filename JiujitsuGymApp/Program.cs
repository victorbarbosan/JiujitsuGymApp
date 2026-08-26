using JiujitsuGymApp.Controllers;
using JiujitsuGymApp.Data;
using JiujitsuGymApp.Helpers;
using JiujitsuGymApp.Models;
using JiujitsuGymApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// The connection string carries a password, so it is deliberately absent from
// the committed appsettings.json. Locally it comes from user-secrets; deployed
// it comes from ConnectionStrings__DefaultConnection. Fail here with something
// actionable rather than letting Npgsql throw about a malformed empty string.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No database connection string configured. For local development run:\r\n\r\n" +
        "  dotnet user-secrets --project JiujitsuGymApp set \"ConnectionStrings:DefaultConnection\" " +
        "\"Host=localhost;Port=5432;Database=jiujitsugym;Username=postgres;Password=postgres\"\r\n\r\n" +
        "In Docker this is supplied as the ConnectionStrings__DefaultConnection environment variable.");
}

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Google external login. Credentials come from user-secrets/environment,
// same pattern as Smtp:Password - never committed to appsettings.json.
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            // Default callback path is /signin-google.
        });
}

// Behind Cloudflare -> nginx, both of which terminate/forward over the local
// network, so honour X-Forwarded-Proto/-For to recover the original https
// scheme and client IP. Only the local reverse proxy can reach the app, so
// the forwarding proxy is trusted rather than pinned to a fixed address.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<ClassService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AccountService>();
// Sending happens on a background worker so the forgot-password request can
// answer immediately instead of waiting on the relay handshake.
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddHostedService<EmailBackgroundService>();

// Without credentials SmtpEmailSender can only throw, which would leave the
// password reset flow untestable for anyone who has not set up a Gmail app
// password. Fall back to writing the mail to disk, but only in Development -
// the Pi runs as Production, so a missing variable there stays a loud failure
// rather than silently parking reset links in a folder.
if (builder.Environment.IsDevelopment() &&
    string.IsNullOrWhiteSpace(builder.Configuration["Smtp:Password"]))
{
    builder.Services.AddTransient<IEmailSender, DevFileEmailSender>();
}
else
{
    builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
}

// Forgot-password is the only anonymous endpoint that sends mail, and the Gmail
// relay behind it allows a few hundred messages a day. Cap it per client IP so
// nobody can exhaust the day's quota or flood a member's inbox by resubmitting
// the form. The limiter is in-memory, so it costs the Pi a dictionary entry per
// caller rather than a process.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(AccountController.ForgotPasswordPolicy, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            // UseForwardedHeaders runs first, so this is the real client IP
            // rather than the reverse proxy's.
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15)
            }));
});
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IdentitySeedService>();
builder.Services.AddScoped<DemoDataService>();

// Configure application cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    // The public endpoint is always https (Cloudflare), so never send the
    // auth cookie over plaintext.
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// MVC
builder.Services.AddControllersWithViews(options =>
{
    // Require authenticated user for EVERY controller action
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

var app = builder.Build();

// Apply pending migrations, then seed the roles and bootstrap administrator
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Run the async seeding synchronously at startup
    services.GetRequiredService<IdentitySeedService>().SeedAsync().GetAwaiter().GetResult();
}

// Pipeline
// Must run before anything that inspects the scheme or client IP so the
// rest of the pipeline sees the original https request from behind the proxy.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Branded pages for bodyless error responses (404 on unknown routes, etc.).
// Re-execute keeps the original status code, so clients still see the 404.
app.UseStatusCodePagesWithReExecute("/Home/StatusCodePage", "?code={0}");

// No UseHttpsRedirection: Cloudflare terminates TLS and enforces https, and
// the nginx -> app hop is intentionally plain http on the loopback.
app.UseStaticFiles();

app.UseRouting();

// After UseRouting so the endpoint's rate limit policy is known.
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
