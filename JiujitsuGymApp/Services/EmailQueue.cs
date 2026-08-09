using System.Threading.Channels;

namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// In-memory queue between the request and <see cref="EmailBackgroundService"/>.
    /// A channel rather than a table: a password reset that is lost to a restart
    /// costs the member one more click on "Forgot password", which is not worth
    /// a database round trip and a drain job on the Pi.
    /// </summary>
    public sealed class EmailQueue : IEmailQueue
    {
        // Bounded so a flood cannot grow the queue without limit. The
        // forgot-password rate limit already caps the realistic inflow, so a
        // full queue means something is badly wrong - drop the message and say
        // so, rather than block the request thread waiting for space.
        private readonly Channel<EmailMessage> _channel =
            Channel.CreateBounded<EmailMessage>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true
            });

        private readonly ILogger<EmailQueue> _logger;

        public EmailQueue(ILogger<EmailQueue> logger) => _logger = logger;

        public void Enqueue(EmailMessage message)
        {
            if (!_channel.Writer.TryWrite(message))
            {
                _logger.LogError(
                    "The email queue is full, so the message to {Recipient} was dropped.", message.To);
            }
        }

        public IAsyncEnumerable<EmailMessage> DequeueAllAsync(CancellationToken cancellationToken) =>
            _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
