using System.Collections.Concurrent;
using JiujitsuGymApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace JiujitsuGymApp.Tests.Services;

public sealed class EmailBackgroundServiceTests
{
    /// <summary>Records what it was asked to send, and can be told to fail.</summary>
    private sealed class FakeEmailSender : IEmailSender
    {
        public ConcurrentQueue<string> Sent { get; } = new();
        public HashSet<string> FailFor { get; } = [];

        public Task SendAsync(string to, string subject, string htmlBody)
        {
            if (FailFor.Contains(to))
                throw new InvalidOperationException($"Simulated relay failure for {to}.");

            Sent.Enqueue(to);
            return Task.CompletedTask;
        }
    }

    private static (EmailBackgroundService service, EmailQueue queue, FakeEmailSender sender) Create()
    {
        var sender = new FakeEmailSender();

        var services = new ServiceCollection();
        services.AddScoped<IEmailSender>(_ => sender);
        var provider = services.BuildServiceProvider();

        var queue = new EmailQueue(NullLogger<EmailQueue>.Instance);
        var service = new EmailBackgroundService(
            queue,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<EmailBackgroundService>.Instance);

        return (service, queue, sender);
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!condition())
        {
            cts.Token.ThrowIfCancellationRequested();
            await Task.Delay(10, cts.Token);
        }
    }

    [Fact]
    public async Task QueuedMessagesAreSentInTheBackground()
    {
        var (service, queue, sender) = Create();
        await service.StartAsync(CancellationToken.None);

        queue.Enqueue(new EmailMessage("first@example.com", "Subject", "<p>Body</p>"));
        queue.Enqueue(new EmailMessage("second@example.com", "Subject", "<p>Body</p>"));

        await WaitForAsync(() => sender.Sent.Count == 2);
        await service.StopAsync(CancellationToken.None);

        Assert.Equal(["first@example.com", "second@example.com"], sender.Sent);
    }

    /// <summary>
    /// The loop must survive a send that throws. If one bad address could end it,
    /// a single failure would silently stop every password reset that followed -
    /// with nothing waiting on the request side to notice.
    /// </summary>
    [Fact]
    public async Task AFailedSendDoesNotStopLaterMessages()
    {
        var (service, queue, sender) = Create();
        sender.FailFor.Add("broken@example.com");

        await service.StartAsync(CancellationToken.None);

        queue.Enqueue(new EmailMessage("broken@example.com", "Subject", "<p>Body</p>"));
        queue.Enqueue(new EmailMessage("after@example.com", "Subject", "<p>Body</p>"));

        await WaitForAsync(() => sender.Sent.Contains("after@example.com"));
        await service.StopAsync(CancellationToken.None);

        Assert.DoesNotContain("broken@example.com", sender.Sent);
        Assert.Contains("after@example.com", sender.Sent);
    }

    [Fact]
    public async Task StopAsync_ShutsDownWithoutFaulting()
    {
        var (service, _, _) = Create();

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        // A cancelled channel read must be treated as a normal stop, not an error.
        Assert.True(service.ExecuteTask!.IsCompletedSuccessfully);
    }
}
