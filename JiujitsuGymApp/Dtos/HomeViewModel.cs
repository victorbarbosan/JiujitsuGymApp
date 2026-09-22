using JiujitsuGymApp.Dtos;

namespace JiujitsuGymApp.Models
{
    public class HomeViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string Belt { get; set; } = "White";
        public int TotalClassesAttended { get; set; }
        public List<ClassEventDto> TodayClasses { get; set; } = [];
        public List<AnnouncementDto> Announcements { get; set; } = [];
        public bool CanCreateAnnouncement { get; set; }
    }
}