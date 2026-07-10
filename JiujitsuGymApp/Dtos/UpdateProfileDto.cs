using System.ComponentModel.DataAnnotations;
using JiujitsuGymApp.Models;

namespace JiujitsuGymApp.Dtos
{
    public class UpdateProfileDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public BeltColor? Belt { get; set; }
    }
}