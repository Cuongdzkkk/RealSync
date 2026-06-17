using Microsoft.Extensions.Options;
using RealSync.Core.Interfaces;
using RealSync.Services.Options;

namespace RealSync.Api.HostedServices;

public class FollowUpReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FollowUpReminderOptions _options;
    private readonly ILogger<FollowUpReminderBackgroundService> _logger;

    public FollowUpReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<FollowUpReminderOptions> options,
        ILogger<FollowUpReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Follow-up reminder background service is disabled");
            return;
        }

        await ProcessAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollIntervalSeconds));
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await ProcessAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Follow-up reminder background service is stopping");
        }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IFollowUpReminderService>();
            var result = await service.ProcessDueRemindersAsync(cancellationToken);

            _logger.LogInformation(
                "Processed follow-up reminders. Scanned={Scanned}, Sent={Sent}, Skipped={Skipped}, Failed={Failed}",
                result.Scanned,
                result.Sent,
                result.Skipped,
                result.Failed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Follow-up reminder processing cycle failed");
        }
    }
}
