using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace JiujitsuGymApp.Models
{
	public enum BeltColor
	{
		White = 0,
		Grey = 1,
		Yellow = 2,
		Orange = 3,
		Green = 4,
		Blue = 5,
		Purple = 6,
		Brown = 7,
		Black = 8
	}

	public class User : IdentityUser
	{
		[Required, StringLength(100)]
		public string FirstName { get; set; } = string.Empty;
		[Required, StringLength(100)]
		public string LastName { get; set; } = string.Empty;
		[StringLength(30)]
		public BeltColor? Belt { get; set; } = BeltColor.White;
		[StringLength(200)]
		public string? Address { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? DeletedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }
		public string Name => $"{FirstName} {LastName}";
		public virtual ICollection<Class> TaughtClasses { get; set; } = new List<Class>();
	}
}
