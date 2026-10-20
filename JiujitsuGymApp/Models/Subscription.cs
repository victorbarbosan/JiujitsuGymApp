using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JiujitsuGymApp.Models
{
    /// <summary>
    /// Links a local User to a Stripe Customer/Subscription pair and caches
    /// the current billing state. Stripe webhooks are what keep this record
    /// up to date (see upcoming webhook handling step) - this table is a
    /// read-optimized local mirror, never the source of truth itself.
    /// </summary>
    public class Subscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual User? User { get; set; }

        public int SubscriptionPlanId { get; set; }
        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }

        /// <summary>Stripe Customer ID, e.g. "cus_123".</summary>
        [StringLength(100)]
        public string? StripeCustomerId { get; set; }

        /// <summary>Stripe Subscription ID, e.g. "sub_123".</summary>
        [StringLength(100)]
        public string? StripeSubscriptionId { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Incomplete;

        public DateTime? CurrentPeriodEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CanceledAt { get; set; }
    }
}
