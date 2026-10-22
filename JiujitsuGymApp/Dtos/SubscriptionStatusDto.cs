namespace JiujitsuGymApp.Dtos
{
    /// <summary>
    /// Current subscription state for the signed-in student, shaped for display.
    /// </summary>
    public class SubscriptionStatusDto
    {
        public bool HasSubscription { get; set; }
        public string? PlanName { get; set; }
        public string? Status { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
    }
}
