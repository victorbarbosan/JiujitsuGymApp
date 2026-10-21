namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// Strongly typed binding for the "Stripe" configuration section.
    /// SecretKey and WebhookSecret are sensitive and are supplied via
    /// user-secrets locally / environment variables in production -
    /// appsettings.json only carries empty placeholders, same pattern
    /// as Smtp:Password and ConnectionStrings:DefaultConnection.
    /// </summary>
    public class StripeOptions
    {
        public const string SectionName = "Stripe";

        public string PublishableKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Signing secret for verifying webhook payloads came from Stripe.
        /// Will be used in the upcoming webhook handling step.
        /// </summary>
        public string WebhookSecret { get; set; } = string.Empty;
    }
}
