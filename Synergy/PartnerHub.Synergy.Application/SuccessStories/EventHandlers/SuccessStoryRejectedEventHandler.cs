using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.Events;

namespace PartnersHub.Synergy.Application.SuccessStories.EventHandlers;

public class SuccessStoryRejectedEventHandler : INotificationHandler<SuccessStoryRejectedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public SuccessStoryRejectedEventHandler(
        INotificationService notificationService,
        IAuditService auditService)
    {
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task Handle(SuccessStoryRejectedEvent notification, CancellationToken cancellationToken)
    {
        // Determine who rejected
        string action;
        string comments;
        
        if (notification.NewStatus == SuccessStoryStatus.AssetManagerRejected)
        {
            action = "Rejected by Asset Manager";
            comments = $"Success Story rejected by Asset Manager. Reason: {notification.RejectionReason}";
        }
        else if (notification.NewStatus == SuccessStoryStatus.AdminRejected)
        {
            action = "Rejected by Synergy Admin";
            comments = $"Success Story rejected by Synergy Admin. Reason: {notification.RejectionReason}";
        }
        else
        {
            return; // Unknown status
        }

        // Log to audit trail
        await _auditService.LogActionAsync(
            "SuccessStory",
            notification.SuccessStoryId,
            action,
            notification.RejectedBy,
            comments,
            cancellationToken);

        // Send notification to PC Representative
        await _notificationService.SendRejectedNotificationAsync(
            "SuccessStory",
            notification.SuccessStoryId,
            notification.CompanyId,
            notification.RejectionReason,
            notification.RejectedBy,
            notification.SuccessStoryName,
            notification.CompanyName,
            notification.CompanyEmail,
            cancellationToken);
    }
}
