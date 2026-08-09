using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// Sends mail through an external SMTP relay (Gmail by default), so nothing
    /// mail-shaped has to run on the Pi itself. Credentials come from
    /// Smtp:Username / Smtp:Password — for Gmail that is the address plus an
    /// app password (https://myaccount.google.com/apppasswords), never the
    /// account password.
    /// </summary>
    public class SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger) : IEmailSender
    {
        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var username = config["Smtp:Username"];
            var password = config["Smtp:Password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "No SMTP credentials configured. For local development run:\r\n\r\n" +
                    "  dotnet user-secrets --project JiujitsuGymApp set \"Smtp:Username\" \"you@gmail.com\"\r\n" +
                    "  dotnet user-secrets --project JiujitsuGymApp set \"Smtp:Password\" \"<gmail app password>\"\r\n\r\n" +
                    "In Docker these are supplied as the Smtp__Username and Smtp__Password environment variables.");
            }

            var host = config["Smtp:Host"] ?? "smtp.gmail.com";
            var port = config.GetValue("Smtp:Port", 587);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                config["Smtp:FromName"] ?? "Nexus BJJ",
                config["Smtp:From"] ?? username));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);

            // The one log line that means the relay actually accepted the
            // message, as opposed to it being written to disk by the dev sender.
            logger.LogInformation("Sent email to {Recipient} via {Host} as {Sender}.",
                to, host, message.From.ToString());
        }
    }
}
