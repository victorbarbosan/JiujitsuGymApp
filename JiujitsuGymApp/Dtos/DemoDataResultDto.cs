namespace JiujitsuGymApp.Dtos
{
    /// <summary>
    /// Outcome of a seed or purge run: a line to show the admin, plus the
    /// resulting status so the tab can repaint without a second round trip.
    /// </summary>
    public class DemoDataResultDto
    {
        public string Message { get; set; } = string.Empty;
        public DemoDataStatusDto Status { get; set; } = new();
    }
}
