using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.Events;

namespace PartnersHub.Synergy.Application.Opportunities.EventHandlers;

public class OpportunityApprovedEventHandler : INotificationHandler<OpportunityApprovedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public OpportunityApprovedEventHandler(
        INotificationService notificationService,
        IAuditService auditService)
    {
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task Handle(OpportunityApprovedEvent notification, CancellationToken cancellationToken)
    {
        // Determine the approval level and log accordingly
        string action;
        string comments;
        
        if (notification.NewStatus == OpportunityStatus.Pending)
        {
            action = "Approved by Asset Manager";
            comments = "Opportunity approved by Asset Manager, pending Synergy Admin review";
            
            // Notify PC Representative
            await _notificationService.SendApprovedByAssetManagerNotificationAsync(
                "opportunities",
                notification.OpportunityId,
                notification.CompanyId,
                notification.ApprovedBy,
                notification.OpportunityName,
                notification.CompanyName,
                cancellationToken);
        }
        else if (notification.NewStatus == OpportunityStatus.Published)
        {
            action = "Published";
            comments = "Opportunity published by Synergy Admin";
            
            // Notify PC Representative and eligible companies
            await _notificationService.SendPublishedNotificationAsync(
                "opportunities",
                notification.OpportunityId,
                notification.CompanyId,
                notification.ApprovedBy,
                notification.OpportunityName,
                notification.CompanyName,
                notification.CompanyEmail,
                cancellationToken);
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
            notification.ApprovedBy,
            comments,
            cancellationToken);
    }
}
