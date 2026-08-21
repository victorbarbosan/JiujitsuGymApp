namespace JiujitsuGymApp.Services
{
    /// <summary>A composed email, ready to hand to an <see cref="IEmailSender"/>.</summary>
    public record EmailMessage(string To, string Subject, string HtmlBody);
}
