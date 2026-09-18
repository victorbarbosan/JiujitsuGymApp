namespace JiujitsuGymApp.Dtos
{
    public class AnnouncementReplyDto
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool CanDelete { get; set; }
    }

    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<AnnouncementReplyDto> Replies { get; set; } = [];
    }
}
