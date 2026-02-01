using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Application.Options;
using PartnerHub.NotificationsHub.Domain.Enums;

namespace PartnerHub.NotificationsHub.Infrastructure.Workers;

public class NotificationRetryWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationRetryWorker> _logger;
    private readonly NotificationRetryOptions _options;

    public NotificationRetryWorker(
        IServiceProvider serviceProvider,
        ILogger<NotificationRetryWorker> logger,
        IOptions<NotificationRetryOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Retry Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(_options.ScanIntervalMinutes), stoppingToken);
                await ProcessFailedNotificationsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in retry worker");
            }
        }

        _logger.LogInformation("Notification Retry Worker stopped");
    }

    private async Task ProcessFailedNotificationsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();

        var failedNotifications = await repository.GetFailedNotificationsAsync(
            _options.MaxRetryCount,
            _options.BatchSize,
            ct);

        if (!failedNotifications.Any())
        {
            _logger.LogInformation("No failed notifications to retry");
            return;
        }

        _logger.LogInformation("Found {Count} failed notifications to retry", failedNotifications.Count);

        foreach (var notification in failedNotifications)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                _logger.LogInformation("Retrying notification {NotificationId}, attempt {AttemptCount}",
                    notification.Id, notification.AttemptCount + 1);

                var result = await dispatcher.DispatchAsync(notification, ct);

                notification.AttemptCount++;
                notification.LastAttemptAtUtc = DateTimeOffset.UtcNow;

                if (result.Success)
                {
                    notification.Status = NotificationStatus.Sent;
                    notification.LastError = null;
                    notification.NextAttemptAtUtc = null;
                    _logger.LogInformation("Notification {NotificationId} sent successfully on retry", notification.Id);
                }
                else
                {
                    notification.LastError = result.Message;

                    if (notification.AttemptCount >= _options.MaxRetryCount)
                    {
                        notification.Status = NotificationStatus.DeadLetter;
                        notification.NextAttemptAtUtc = null;
                        _logger.LogWarning("Notification {NotificationId} moved to dead letter after {AttemptCount} attempts",
                            notification.Id, notification.AttemptCount);
                    }
                    else
                    {
                        notification.Status = NotificationStatus.Failed;
                        var delayMinutes = GetRetryDelay(notification.AttemptCount);
                        notification.NextAttemptAtUtc = DateTimeOffset.UtcNow.AddMinutes(delayMinutes);
                        _logger.LogWarning("Notification {NotificationId} retry failed, next attempt in {Delay} minutes",
                            notification.Id, delayMinutes);
                    }
                }

                await repository.UpdateAsync(notification, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying notification {NotificationId}", notification.Id);
            }
        }
    }

    private int GetRetryDelay(int attemptCount)
    {
        var index = Math.Min(attemptCount - 1, _options.RetryDelaysMinutes.Length - 1);
        return _options.RetryDelaysMinutes[index];
    }
}