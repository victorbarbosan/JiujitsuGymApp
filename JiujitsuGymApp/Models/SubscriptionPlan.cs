using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JiujitsuGymApp.Models
{
    public enum BillingInterval
    {
        Month = 0,
        Year = 1
    }

    /// <summary>
    /// Represents a recurring billing plan offered to students (e.g. "Monthly Unlimited").
    /// Mirrors a Stripe Price object, which is the thing Stripe actually bills against.
    /// </summary>
    public class SubscriptionPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        public BillingInterval Interval { get; set; } = BillingInterval.Month;

        /// <summary>
        /// The Stripe Price ID (e.g. "price_123") this plan maps to.
        /// Stripe is the source of truth for the actual charge amount/schedule;
        /// this column lets us create a Checkout Session for the right price.
        /// </summary>
        [StringLength(100)]
        public string? StripePriceId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
