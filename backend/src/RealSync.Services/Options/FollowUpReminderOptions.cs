namespace RealSync.Services.Options;

public class FollowUpReminderOptions
{
    private int _pollIntervalSeconds = 60;
    private int _batchSize = 100;

    public bool Enabled { get; set; } = true;

    public int PollIntervalSeconds
    {
        get => _pollIntervalSeconds;
        set => _pollIntervalSeconds = Math.Max(10, value);
    }

    public int BatchSize
    {
        get => _batchSize;
        set => _batchSize = Math.Clamp(value, 1, 500);
    }
}
