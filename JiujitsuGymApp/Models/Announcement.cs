using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JiujitsuGymApp.Models
{
    /// <summary>
    /// A single self-referencing model used for both announcements (ParentId is null)
    /// and replies (ParentId points to the root announcement). Kept as one table so
    /// replies share the same validation, auditing, and soft-delete behavior as their
    /// parent, at the cost of needing application-level rules to prevent deep nesting.
    /// </summary>
    public class Announcement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        [Required]
        [ForeignKey("AuthorId")]
        public User Author { get; set; } = null!;

        /// <summary>Null for a root announcement; set to the root announcement's Id for a reply.</summary>
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public Announcement? Parent { get; set; }

        [Required, StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<Announcement> Replies { get; set; } = new List<Announcement>();
    }
}
