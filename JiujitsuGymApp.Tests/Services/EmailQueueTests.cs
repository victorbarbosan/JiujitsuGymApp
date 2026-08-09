using JiujitsuGymApp.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace JiujitsuGymApp.Tests.Services;

public sealed class EmailQueueTests
{
    private static EmailQueue CreateQueue() => new(NullLogger<EmailQueue>.Instance);

    [Fact]
    public async Task Enqueue_HandsTheMessageToTheReader()
    {
        var queue = CreateQueue();
        queue.Enqueue(new EmailMessage("member@example.com", "Subject", "<p>Body</p>"));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await foreach (var message in queue.DequeueAllAsync(cts.Token))
        {
            Assert.Equal("member@example.com", message.To);
            Assert.Equal("Subject", message.Subject);
            Assert.Equal("<p>Body</p>", message.HtmlBody);
            break;
        }
    }

    /// <summary>
    /// Enqueuing runs on the request thread, so it must never block or throw -
    /// a full queue has to degrade to a dropped email, not a failed reset page.
    /// </summary>
    [Fact]
    public void Enqueue_WhenTheQueueIsFull_DropsInsteadOfBlocking()
    {
        var queue = CreateQueue();

        var enqueueAll = Task.Run(() =>
        {
            for (var i = 0; i < 500; i++)
                queue.Enqueue(new EmailMessage($"member{i}@example.com", "Subject", "<p>Body</p>"));
        });

        Assert.True(enqueueAll.Wait(TimeSpan.FromSeconds(5)),
            "Enqueue blocked once the queue filled up instead of dropping.");
    }

    [Fact]
    public async Task DequeueAllAsync_StopsWhenCancelled()
    {
        var queue = CreateQueue();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in queue.DequeueAllAsync(cts.Token)) { }
        });
    }
}
