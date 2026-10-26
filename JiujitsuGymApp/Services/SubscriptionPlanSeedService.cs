using JiujitsuGymApp.Data;
using JiujitsuGymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// Seed definition for a single subscription plan, bound from the
    /// "SubscriptionPlans" configuration section (appsettings, user-secrets,
    /// or SubscriptionPlans__0__Name-style environment variables).
    /// </summary>
    public class SubscriptionPlanSeedOptions
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Interval { get; set; } = "Month";
        public string StripePriceId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Startup seeding for subscription plans, same idea as IdentitySeedService's
    /// admin bootstrap: reads definitions from configuration and upserts them by
    /// name, so re-running on every restart is harmless and a plan's price can be
    /// corrected centrally (in config) without a manual SQL edit.
    /// </summary>
    public class SubscriptionPlanSeedService(ApplicationDbContext db, IConfiguration config, ILogger<SubscriptionPlanSeedService> logger)
    {
        public async Task SeedAsync()
        {
            var seedPlans = config.GetSection("SubscriptionPlans").Get<List<SubscriptionPlanSeedOptions>>();
            if (seedPlans is null || seedPlans.Count == 0)
            {
                logger.LogInformation("No SubscriptionPlans configured - skipping subscription plan seeding.");
                return;
            }

            foreach (var seed in seedPlans)
            {
                if (string.IsNullOrWhiteSpace(seed.StripePriceId))
                {
                    logger.LogWarning(
                        "Subscription plan '{PlanName}' has no StripePriceId configured - skipping.", seed.Name);
                    continue;
                }

                var interval = Enum.TryParse<BillingInterval>(seed.Interval, ignoreCase: true, out var parsed)
                    ? parsed
                    : BillingInterval.Month;

                var existing = await db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == seed.Name);
                if (existing is null)
                {
                    db.SubscriptionPlans.Add(new SubscriptionPlan
                    {
                        Name = seed.Name,
                        Description = seed.Description,
                        Price = seed.Price,
                        Interval = interval,
                        StripePriceId = seed.StripePriceId,
                        IsActive = true
                    });
                }
                else
                {
                    existing.Description = seed.Description;
                    existing.Price = seed.Price;
                    existing.Interval = interval;
                    existing.StripePriceId = seed.StripePriceId;
                    existing.IsActive = true;
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
