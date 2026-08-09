namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// Drains <see cref="IEmailQueue"/> and does the actual sending, off the
    /// request path. Connecting to Gmail, negotiating STARTTLS and authenticating
    /// costs a second or two before a message even starts moving, which the
    /// member should not sit and watch.
    ///
    /// It runs inside the existing web process - a hosted service, not another
    /// container - so it costs the Pi one idle task waiting on a channel.
    /// </summary>
    public sealed class EmailBackgroundService(
        IEmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailBackgroundService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var message in queue.DequeueAllAsync(stoppingToken))
                {
                    await SendAsync(message, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown.
            }
        }

        private async Task SendAsync(EmailMessage message, CancellationToken stoppingToken)
        {
            try
            {
                // IEmailSender is registered per-scope-friendly rather than as a
                // singleton, so take a scope per message instead of capturing one
                // for the lifetime of the service.
                using var scope = scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                await sender.SendAsync(message.To, message.Subject, message.HtmlBody);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // Nobody is waiting on this any more, so a failure can only be
                // reported to the log. It must not escape: an exception here
                // would end the loop and silently stop every later email.
                logger.LogError(ex, "Failed to send the queued email to {Recipient}.", message.To);
            }
        }
    }
}
