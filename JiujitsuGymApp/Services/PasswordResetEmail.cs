using System.Net;

namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// Builds the password reset email. The markup lives here rather than in the
    /// controller so the action stays about the HTTP flow, and so the template
    /// can be rendered in a test without standing up MVC.
    ///
    /// It is deliberately table-based with inline styles: mail clients strip
    /// stylesheets and have patchy flex/grid support, so the layout rules that
    /// would live in a stylesheet on the web have to travel with the elements.
    /// </summary>
    public static class PasswordResetEmail
    {
        public const string Subject = "Reset your Nexus BJJ password";

        /// <param name="recipient">Address the message is addressed to.</param>
        /// <param name="resetLink">
        /// Absolute reset URL, already percent-encoded by Url.Action. It is HTML
        /// encoded here so the '&amp;' joining the query parameters survives as a
        /// single '&amp;' once a mail client parses the href.
        /// </param>
        public static EmailMessage Create(string recipient, string resetLink)
        {
            var href = WebUtility.HtmlEncode(resetLink);

            return new EmailMessage(recipient, Subject, $"""
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>Reset your Nexus BJJ password</title>
                </head>
                <body style="margin: 0; padding: 0; background-color: #f4f5f7; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased;">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color: #f4f5f7; padding: 40px 0;">
                        <tr>
                            <td align="center">
                                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width: 520px; background-color: #ffffff; border-radius: 8px; border: 1px solid #e1e4e8; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.04);">

                                    <!-- Header -->
                                    <tr>
                                        <td style="background-color: #111827; padding: 24px 32px; text-align: center;">
                                            <span style="color: #ffffff; font-size: 20px; font-weight: 700; letter-spacing: 1px; text-transform: uppercase;">Nexus BJJ</span>
                                        </td>
                                    </tr>

                                    <!-- Body Content -->
                                    <tr>
                                        <td style="padding: 32px; color: #374151; font-size: 15px; line-height: 1.6;">
                                            <h1 style="margin: 0 0 16px 0; font-size: 20px; font-weight: 600; color: #111827;">Password Reset Request</h1>

                                            <p style="margin: 0 0 16px 0;">Hi,</p>
                                            <p style="margin: 0 0 24px 0;">We received a request to reset the password for your <strong>Nexus BJJ</strong> account.</p>

                                            <!-- CTA Button -->
                                            <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin: 28px 0;">
                                                <tr>
                                                    <td align="center" style="border-radius: 6px; background-color: #2563eb;">
                                                        <a href="{href}" target="_blank" style="display: inline-block; padding: 12px 24px; color: #ffffff; font-weight: 600; font-size: 15px; text-decoration: none; border-radius: 6px;">Reset Password</a>
                                                    </td>
                                                </tr>
                                            </table>

                                            <p style="margin: 0 0 16px 0; font-size: 13px; color: #6b7280;">This link will expire in <strong>24 hours</strong>.</p>
                                            <p style="margin: 0; font-size: 13px; color: #6b7280;">If you didn't request a password reset, you can safely ignore this email &mdash; your password will remain unchanged.</p>
                                        </td>
                                    </tr>

                                    <!-- Divider & Fallback Link -->
                                    <tr>
                                        <td style="padding: 0 32px;">
                                            <hr style="border: none; border-top: 1px solid #e5e7eb; margin: 0;">
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="padding: 20px 32px 32px 32px; font-size: 12px; color: #9ca3af; word-break: break-all;">
                                            If the button above doesn't work, copy and paste this URL into your browser:
                                            <br>
                                            <a href="{href}" style="color: #2563eb; text-decoration: underline;">{href}</a>
                                        </td>
                                    </tr>

                                </table>

                                <!-- Footer -->
                                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width: 520px; margin-top: 16px;">
                                    <tr>
                                        <td align="center" style="font-size: 12px; color: #9ca3af;">
                                            &copy; Nexus BJJ. All rights reserved.
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>
                """);
        }
    }
}
