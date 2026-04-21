using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JiujitsuGymApp.Models
{
    public class ClassSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string TeacherId { get; set; } = string.Empty;

        [ForeignKey("TeacherId")]
        public User Teacher { get; set; } = null!;

        [Required, StringLength(100)]
        public string Location { get; set; } = "Not set";

        /// <summary>0 = Sunday ... 6 = Saturday</summary>
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>Time of day, e.g. 07:00 or 19:00</summary>
        [Required]
        public TimeSpan TimeOfDay { get; set; }

        public bool IsActive { get; set; } = true;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Class> Sessions { get; set; } = new List<Class>();
    }
}