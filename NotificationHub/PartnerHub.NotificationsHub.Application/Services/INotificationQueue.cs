using PartnerHub.NotificationsHub.Domain.Entities;

namespace PartnerHub.NotificationsHub.Application.Services;

public interface INotificationQueue
{
    Task EnqueueAsync(NotificationEntity notification, CancellationToken ct = default);
    Task EnqueueRetryAsync(NotificationEntity notification, TimeSpan delay, CancellationToken ct = default);
}