namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// Writes mail to disk instead of sending it, so the password reset flow can
    /// be exercised locally without a Gmail app password. Registered only when
    /// the app is in Development *and* no SMTP credentials are configured, so it
    /// cannot stand in for the real sender on the Pi, which runs as Production.
    ///
    /// The files contain live reset links, hence App_Data (gitignored) rather
    /// than anywhere served by UseStaticFiles.
    /// </summary>
    public class DevFileEmailSender(IWebHostEnvironment env, ILogger<DevFileEmailSender> logger) : IEmailSender
    {
        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var directory = Path.Combine(env.ContentRootPath, "App_Data", "sent-emails");
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory,
                $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}-{SanitiseForFileName(to)}.html");

            await File.WriteAllTextAsync(path,
                $"""
                <!doctype html>
                <meta charset="utf-8">
                <title>{subject}</title>
                <p style="font:14px system-ui;color:#666">
                    To: {to}<br>Subject: {subject}
                </p>
                <hr>
                {htmlBody}
                """);

            logger.LogWarning(
                "SMTP is not configured, so the email to {Recipient} was written to {Path} instead of being sent.",
                to, path);
        }

        private static string SanitiseForFileName(string value) =>
            string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
    }
}
