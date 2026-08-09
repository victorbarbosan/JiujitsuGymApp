namespace JiujitsuGymApp.Services
{
    /// <summary>
    /// Hands an email off to be sent in the background. Enqueuing returns
    /// immediately and never throws, so a request thread is never left waiting
    /// on a mail relay.
    /// </summary>
    public interface IEmailQueue
    {
        void Enqueue(EmailMessage message);

        IAsyncEnumerable<EmailMessage> DequeueAllAsync(CancellationToken cancellationToken);
    }
}
