using System.ComponentModel.DataAnnotations;

namespace JiujitsuGymApp.Dtos
{
    public class CreateClassScheduleDto
    {
        [Required]
        public string TeacherId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public string DayOfWeek { get; set; } = string.Empty;

        [Required]
        public string TimeOfDay { get; set; } = string.Empty;
    }
}
