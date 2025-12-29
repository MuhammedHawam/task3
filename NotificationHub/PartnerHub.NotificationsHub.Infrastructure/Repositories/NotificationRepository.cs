using Microsoft.EntityFrameworkCore;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Domain.Entities;
using PartnerHub.NotificationsHub.Domain.Enums;
using PartnerHub.NotificationsHub.Infrastructure.Persistence;

namespace PartnerHub.NotificationsHub.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(NotificationEntity entity, CancellationToken ct = default)
    {
        _context.Notifications.Add(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(NotificationEntity entity, CancellationToken ct = default)
    {
        _context.Notifications.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<NotificationEntity>> GetPendingNotificationsAsync(DateTimeOffset cutoff, int batchSize, int maxRetryCount, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.Notifications
            .Where(n =>
                (n.Status == NotificationStatus.Pending || n.Status == NotificationStatus.Failed)
                && n.AttemptCount < maxRetryCount
                && (n.NextAttemptAtUtc == null || n.NextAttemptAtUtc <= now)
                && n.CreatedAtUtc >= cutoff)
            .OrderBy(n => n.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync(ct);
    }
}