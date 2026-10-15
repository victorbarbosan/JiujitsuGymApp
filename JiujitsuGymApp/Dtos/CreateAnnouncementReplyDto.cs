using System.ComponentModel.DataAnnotations;

namespace JiujitsuGymApp.Dtos
{
    public class CreateAnnouncementReplyDto
    {
        [Required, StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }
}
