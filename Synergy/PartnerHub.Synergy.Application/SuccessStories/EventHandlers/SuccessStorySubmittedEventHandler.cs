using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Domain.Events;

namespace PartnersHub.Synergy.Application.SuccessStories.EventHandlers;

public class SuccessStorySubmittedEventHandler : INotificationHandler<SuccessStorySubmittedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public SuccessStorySubmittedEventHandler(
        INotificationService notificationService,
        IAuditService auditService)
    {
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task Handle(SuccessStorySubmittedEvent notification, CancellationToken cancellationToken)
    {
        // Log to audit trail
        await _auditService.LogActionAsync(
            "SuccessStory",
            notification.SuccessStoryId,
            "Submitted",
            notification.SubmittedBy,
            "Success Story submitted for Asset Manager review",
            cancellationToken);

        // Send notification to Asset Manager
        await _notificationService.SendSubmittedNotificationAsync(
            "success-stories",
            notification.SuccessStoryId,
            notification.CompanyId,
            notification.SubmittedBy,
            notification.SuccessStoryName,
            notification.CompanyName,
            cancellationToken);
    }
}
