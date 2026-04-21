namespace JiujitsuGymApp.Dtos
{
    public class ClassScheduleDto
    {
        public int Id { get; set; }
        public string TeacherId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string TimeOfDay { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
