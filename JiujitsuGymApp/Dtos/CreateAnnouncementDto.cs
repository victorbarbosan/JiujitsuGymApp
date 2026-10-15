using System.ComponentModel.DataAnnotations;

namespace JiujitsuGymApp.Dtos
{
    public class CreateAnnouncementDto
    {
        [Required, StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }
}
