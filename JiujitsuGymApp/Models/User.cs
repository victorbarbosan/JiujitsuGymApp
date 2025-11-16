using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JiujitsuGymApp.Models
{
	public enum UserRole
	{
		Admin = 1,
		Teacher = 2,
		Student = 3,
		Staff = 4
	}

	public class User
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[Required, StringLength(100)]
		public string Name { get; set; }
		[Required, StringLength(50)]
		public UserRole Role { get; set; }
		[StringLength(30)]
		public string? Belt { get; set; } = "White";
		[Phone]
		[StringLength(20)]
		public string? PhoneNumber { get; set; }
		[EmailAddress, StringLength(100)]
		public string? Email { get; set; }
		[StringLength(200)]
		public string? Address { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? DeletedAt { get; set; }
	}
}
