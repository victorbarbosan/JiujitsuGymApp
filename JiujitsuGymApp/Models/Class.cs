using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace JiujitsuGymApp.Models
{
	public class Class
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[Required]
		public int TeacherId { get; set; }
		[Required]
		[ForeignKey("TeacherId")]
		public User Teacher { get; set; }
		public string Location { get; set; } = "Not set";
		public DateTime DateTime { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? DeletedAt { get; set; }
	}
}
