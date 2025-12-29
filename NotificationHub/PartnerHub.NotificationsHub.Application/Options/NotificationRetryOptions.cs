namespace PartnerHub.NotificationsHub.Application.Options;

public class NotificationRetryOptions
{
    public int ScanIntervalMinutes { get; set; } = 5; // More frequent scanning
    public int LookbackMinutes { get; set; } = 1440; // 24 hours
    public int MaxRetryCount { get; set; } = 5;
    public int BatchSize { get; set; } = 50; // Smaller batches for better performance
    public int[] RetryDelaysMinutes { get; set; } = new[] { 1, 5, 15, 60, 240 }; // Exponential backoff
    public int MaxConcurrentRetries { get; set; } = 10;
    public bool UseExponentialBackoff { get; set; } = true;
    public double BackoffMultiplier { get; set; } = 2.0;
    public int MaxDelayMinutes { get; set; } = 480; // 8 hours max delay
    public int CircuitBreakerThreshold { get; set; } = 10; // Failures before circuit opens
    public int CircuitBreakerTimeoutMinutes { get; set; } = 30;
}