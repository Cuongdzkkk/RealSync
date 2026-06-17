using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using RealSync.Api.HostedServices;
using RealSync.Core.Interfaces;
using RealSync.Core.Models;
using RealSync.Services.Options;

namespace RealSync.UnitTests.FollowUpReminders;

[TestFixture]
public class FollowUpReminderBackgroundServiceTests
{
    [Test]
    public async Task DisabledOptions_ShouldNotProcess()
    {
        var reminderService = new Mock<IFollowUpReminderService>();
        await using var provider = BuildProvider(reminderService.Object);
        var worker = new FollowUpReminderBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new FollowUpReminderOptions { Enabled = false }),
            NullLogger<FollowUpReminderBackgroundService>.Instance);

        await worker.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        await worker.StopAsync(CancellationToken.None);

        reminderService.Verify(s => s.ProcessDueRemindersAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CycleFailure_ShouldNotStopBackgroundWorker()
    {
        var reminderService = new Mock<IFollowUpReminderService>();
        reminderService.SetupSequence(s => s.ProcessDueRemindersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("first cycle failed"))
            .ReturnsAsync(new FollowUpReminderProcessResult { Scanned = 1, Sent = 1 });
        await using var provider = BuildProvider(reminderService.Object);
        var options = new FollowUpReminderOptions { Enabled = true };
        SetPollIntervalSeconds(options, 1);
        var worker = new FollowUpReminderBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(options),
            NullLogger<FollowUpReminderBackgroundService>.Instance);

        await worker.StartAsync(CancellationToken.None);
        await Task.Delay(1300);
        await worker.StopAsync(CancellationToken.None);

        reminderService.Verify(s => s.ProcessDueRemindersAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    private static ServiceProvider BuildProvider(IFollowUpReminderService reminderService)
    {
        return new ServiceCollection()
            .AddScoped(_ => reminderService)
            .BuildServiceProvider();
    }

    private static void SetPollIntervalSeconds(FollowUpReminderOptions options, int value)
    {
        var field = typeof(FollowUpReminderOptions).GetField("_pollIntervalSeconds", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field!.SetValue(options, value);
    }
}
