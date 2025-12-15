using System.ComponentModel.DataAnnotations;
using JiujitsuGymApp.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JiujitsuGymApp.Models
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { set; get; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Belt Rank")]
        [StringLength(30)]
        public string? Belt { get; set; }

        [Display(Name = "Member Since")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Login")]
        [DataType(DataType.DateTime)]
        public DateTime? LastLoginAt { get; set; }

        [Display(Name = "Profile Picture URL")]
        [Url]
        public string? ProfilePictureUrl { get; set; }

        public List<SelectListItem> BeltOptions { get; set; } = EnumHelpers.ToSelectList<BeltColor>();

        // future ideas
        /*
		public int TotalClassesAttended { get; set; }
		public int ClassesThisMonth { get; set; }
		*/
    }
}
