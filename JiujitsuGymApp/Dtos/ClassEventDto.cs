namespace JiujitsuGymApp.Dtos
{
    public class ClassEventDto
    {
        public int Id { get; set; }
        public string Location { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string DateTime { get; set; } = string.Empty;
        public int AttendanceCount { get; set; }
        public bool CheckedIn { get; set; }
    }
}