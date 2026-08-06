using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Identity;

namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// Startup seeding for the things the app cannot function without: the
    /// three roles, and a first administrator to sign in as on a database that
    /// has no accounts yet.
    /// </summary>
    public class IdentitySeedService(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config,
        ILogger<IdentitySeedService> logger)
    {
        private static readonly string[] Roles = ["Admin", "Member", "Teacher"];

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedAdminAsync();
        }

        private async Task SeedRolesAsync()
        {
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        /// <summary>
        /// Creates the bootstrap administrator from SeedAdmin:Email and
        /// SeedAdmin:Password (in Docker: SeedAdmin__Email / SeedAdmin__Password).
        ///
        /// Idempotent by email, so leaving the variables set across redeploys is
        /// harmless — an existing account is only topped up with the Admin role
        /// if it somehow lost it, never re-created and never re-passworded. That
        /// last part matters: a password changed through the UI must not be
        /// silently reverted to the compose file's value on the next restart.
        /// </summary>
        private async Task SeedAdminAsync()
        {
            var email = config["SeedAdmin:Email"];
            var password = config["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogInformation(
                    "SeedAdmin:Email / SeedAdmin:Password not configured - skipping admin bootstrap.");
                return;
            }

            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                if (!await userManager.IsInRoleAsync(existing, "Admin"))
                {
                    await userManager.AddToRoleAsync(existing, "Admin");
                    logger.LogWarning("Restored the Admin role on the bootstrap account {Email}.", email);
                }

                return;
            }

            var admin = new User
            {
                FirstName = config["SeedAdmin:FirstName"] ?? "Site",
                LastName = config["SeedAdmin:LastName"] ?? "Administrator",
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Belt = BeltColor.Black,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                // Loud but non-fatal: a rejected password should not stop the
                // app from booting for everyone who already has an account.
                logger.LogError("Could not create the bootstrap admin {Email}: {Errors}",
                    email, string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(admin, "Admin");
            logger.LogInformation("Created the bootstrap admin account {Email}.", email);
        }
    }
}
