using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Domain.Events;

namespace PartnersHub.Synergy.Application.SuccessStories.EventHandlers;

public class SuccessStoryUpdatedEventHandler : INotificationHandler<SuccessStoryUpdatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public SuccessStoryUpdatedEventHandler(
        INotificationService notificationService,
        IAuditService auditService)
    {
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task Handle(SuccessStoryUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // Log to audit trail
        await _auditService.LogActionAsync(
            "SuccessStory",
            notification.SuccessStoryId,
            "Updated",
            notification.UpdatedBy,
            "SuccessStory Updated.",
            cancellationToken);

        // Send notification to PC Company
        await _notificationService.SendUpdatedNotificationAsync(
            "success-stories",
            notification.SuccessStoryId,
            notification.CompanyId,
            notification.SuccessStoryName,
            notification.CompanyName,
            notification.CompanyEmail,
            cancellationToken);
    }
}
