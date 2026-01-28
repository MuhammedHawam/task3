using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Application.Services;
using PartnerHub.NotificationsHub.Domain.Entities;

namespace PartnerHub.NotificationsHub.Infrastructure.Services;

public class InMemoryNotificationQueue : INotificationQueue, IHostedService
{
    private readonly Channel<NotificationEntity> _channel;
    private readonly ChannelWriter<NotificationEntity> _writer;
    private readonly ChannelReader<NotificationEntity> _reader;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryNotificationQueue> _logger;
    private readonly ConcurrentDictionary<Guid, Timer> _delayedNotifications = new();
    private Task? _processingTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public InMemoryNotificationQueue(IServiceProvider serviceProvider, ILogger<InMemoryNotificationQueue> logger)
    {
        var options = new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<NotificationEntity>(options);
        _writer = _channel.Writer;
        _reader = _channel.Reader;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task EnqueueAsync(NotificationEntity notification, CancellationToken ct = default)
    {
        await _writer.WriteAsync(notification, ct);
        _logger.LogDebug("Notification {NotificationId} enqueued for immediate processing", notification.Id);
    }

    public Task EnqueueRetryAsync(NotificationEntity notification, TimeSpan delay, CancellationToken ct = default)
    {
        var timer = new Timer(async _ =>
        {
            await EnqueueAsync(notification, ct);
            if (_delayedNotifications.TryRemove(notification.Id, out var removedTimer))
            {
                removedTimer.Dispose();
            }
        }, null, delay, Timeout.InfiniteTimeSpan);

        _delayedNotifications.TryAdd(notification.Id, timer);
        _logger.LogDebug("Notification {NotificationId} scheduled for retry in {Delay}", notification.Id, delay);
        
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _processingTask = ProcessNotificationsAsync(_cancellationTokenSource.Token);
        _logger.LogInformation("Notification queue processor started");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _writer.Complete();
        _cancellationTokenSource.Cancel();

        if (_processingTask != null)
        {
            await _processingTask;
        }

        foreach (var timer in _delayedNotifications.Values)
        {
            timer.Dispose();
        }

        _logger.LogInformation("Notification queue processor stopped");
    }

    private async Task ProcessNotificationsAsync(CancellationToken ct)
    {
        await foreach (var notification in _reader.ReadAllAsync(ct))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();
                var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                await ProcessNotificationAsync(notification, dispatcher, repository, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification {NotificationId}", notification.Id);
            }
        }
    }

    private async Task ProcessNotificationAsync(
        NotificationEntity notification,
        INotificationDispatcher dispatcher,
        INotificationRepository repository,
        CancellationToken ct)
    {
        try
        {
            var result = await dispatcher.DispatchAsync(notification, ct);

            if (result.Success)
            {
                notification.Status = Domain.Enums.NotificationStatus.Sent;
                notification.LastError = null;
                notification.NextAttemptAtUtc = null;
                await repository.UpdateAsync(notification, ct);
                
                _logger.LogInformation("Notification {NotificationId} sent successfully", notification.Id);
            }
            else
            {
                await HandleFailureAsync(notification, result.Message ?? "Unknown error", repository, ct);
            }
        }
        catch (Exception ex)
        {
            await HandleFailureAsync(notification, ex.Message, repository, ct);
        }
    }

    private async Task HandleFailureAsync(
        NotificationEntity notification,
        string error,
        INotificationRepository repository,
        CancellationToken ct)
    {
        notification.AttemptCount++;
        notification.LastAttemptAtUtc = DateTimeOffset.UtcNow;
        notification.LastError = error;

        if (notification.AttemptCount >= 5) // Max retry count
        {
            notification.Status = Domain.Enums.NotificationStatus.DeadLetter;
            notification.NextAttemptAtUtc = null;
            _logger.LogWarning("Notification {NotificationId} moved to dead letter after {AttemptCount} attempts", 
                notification.Id, notification.AttemptCount);
        }
        else
        {
            notification.Status = Domain.Enums.NotificationStatus.Failed;
            var delay = CalculateExponentialBackoff(notification.AttemptCount);
            notification.NextAttemptAtUtc = DateTimeOffset.UtcNow.Add(delay);
            
            await EnqueueRetryAsync(notification, delay, ct);
            _logger.LogWarning("Notification {NotificationId} failed, retry scheduled in {Delay}", 
                notification.Id, delay);
        }

        await repository.UpdateAsync(notification, ct);
    }

    private static TimeSpan CalculateExponentialBackoff(int attemptCount)
    {
        var baseDelay = TimeSpan.FromMinutes(1);
        var maxDelay = TimeSpan.FromHours(8);
        
        var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attemptCount - 1));
        return delay > maxDelay ? maxDelay : delay;
    }
}