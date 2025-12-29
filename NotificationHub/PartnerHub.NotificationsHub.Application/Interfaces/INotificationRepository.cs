using PartnerHub.NotificationsHub.Domain.Entities;
using PartnerHub.NotificationsHub.Domain.Enums;

namespace PartnerHub.NotificationsHub.Application.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(NotificationEntity entity, CancellationToken ct = default);
    Task UpdateAsync(NotificationEntity entity, CancellationToken ct = default);
    Task<IEnumerable<NotificationEntity>> GetPendingNotificationsAsync(DateTimeOffset cutoff, int batchSize, int maxRetryCount, CancellationToken ct = default);
}