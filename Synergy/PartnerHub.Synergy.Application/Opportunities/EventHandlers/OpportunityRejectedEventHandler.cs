using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.Events;

namespace PartnersHub.Synergy.Application.Opportunities.EventHandlers;

public class OpportunityRejectedEventHandler : INotificationHandler<OpportunityRejectedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public OpportunityRejectedEventHandler(
        INotificationService notificationService,
        IAuditService auditService)
    {
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task Handle(OpportunityRejectedEvent notification, CancellationToken cancellationToken)
    {
        // Determine who rejected
        string action;
        string comments;
        
        if (notification.NewStatus == OpportunityStatus.AssetManagerRejected)
        {
            action = "Rejected by Asset Manager";
            comments = $"Opportunity rejected by Asset Manager. Reason: {notification.RejectionReason}";
        }
        else if (notification.NewStatus == OpportunityStatus.AdminRejected)
        {
            action = "Rejected by Synergy Admin";
            comments = $"Opportunity rejected by Synergy Admin. Reason: {notification.RejectionReason}";
        }
        else
        {
            return; // Unknown status
        }

        // Log to audit trail
        await _auditService.LogActionAsync(
            "Opportunity",
            notification.OpportunityId,
            action,
            notification.RejectedBy,
            comments,
            cancellationToken);

        // Send notification to PC Representative
        await _notificationService.SendOpportunityRejectedNotificationAsync(
            notification.OpportunityId,
            notification.CompanyId,
            notification.RejectionReason,
            notification.RejectedBy,
            cancellationToken);
    }
}
