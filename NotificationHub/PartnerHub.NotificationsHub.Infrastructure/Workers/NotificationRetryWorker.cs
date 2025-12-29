using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Application.Options;
using PartnerHub.NotificationsHub.Domain.Entities;
using PartnerHub.NotificationsHub.Domain.Enums;
using PartnerHub.NotificationsHub.Infrastructure.Persistence;

namespace PartnerHub.NotificationsHub.Infrastructure.Workers;

public class NotificationRetryWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationRetryWorker> _logger;
    private readonly NotificationRetryOptions _options;

    public NotificationRetryWorker(
        IServiceProvider serviceProvider,
        IOptions<NotificationRetryOptions> options,
        ILogger<NotificationRetryWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationRetryWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing pending notifications");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(_options.ScanIntervalMinutes),
                stoppingToken);
        }
    }

    private async Task ProcessPendingNotificationsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();

        var now = DateTimeOffset.UtcNow;
        var cutoff = now.AddMinutes(-_options.LookbackMinutes);

        while (!ct.IsCancellationRequested)
        {
            var batch = await repository.GetPendingNotificationsAsync(cutoff, _options.BatchSize, _options.MaxRetryCount, ct);

            if (!batch.Any())
            {
                break;
            }

            foreach (var entity in batch)
            {
                await ProcessSingleNotificationAsync(entity, repository, dispatcher, ct);
            }
        }
    }

    private async Task ProcessSingleNotificationAsync(
        NotificationEntity entity,
        INotificationRepository repository,
        INotificationDispatcher dispatcher,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        entity.AttemptCount++;
        entity.LastAttemptAtUtc = now;

        try
        {
            var result = await dispatcher.DispatchAsync(entity, ct);

            if (result.Success)
            {
                entity.Status = NotificationStatus.Sent;
                entity.LastError = null;
                entity.NextAttemptAtUtc = null;
            }
            else
            {
                HandleFailure(entity, result.Message ?? result.ErrorCode ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            HandleFailure(entity, ex.Message);
        }

        await repository.UpdateAsync(entity, ct);
    }

    private void HandleFailure(NotificationEntity entity, string error)
    {
        entity.LastError = error;

        if (entity.AttemptCount >= _options.MaxRetryCount)
        {
            entity.Status = NotificationStatus.DeadLetter;
            entity.NextAttemptAtUtc = null;
        }
        else
        {
            entity.Status = NotificationStatus.Failed;
            entity.NextAttemptAtUtc = CalculateNextAttempt(entity.AttemptCount);
        }
    }

    private DateTimeOffset? CalculateNextAttempt(int attemptCount)
    {
        if (attemptCount >= _options.MaxRetryCount)
        {
            return null;
        }

        var index = Math.Min(attemptCount - 1, _options.RetryDelaysMinutes.Length - 1);
        var delayMinutes = _options.RetryDelaysMinutes[index];
        return DateTimeOffset.UtcNow.AddMinutes(delayMinutes);
    }
}