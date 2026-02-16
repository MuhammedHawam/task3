using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Domain.Events;

namespace PartnersHub.Synergy.Application.Opportunities.EventHandlers;

public class OpportunitySubmittedEventHandler : INotificationHandler<OpportunitySubmittedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public OpportunitySubmittedEventHandler(
        INotificationService notificationService,
        IAuditService auditService)
    {
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task Handle(OpportunitySubmittedEvent notification, CancellationToken cancellationToken)
    {
        // Log to audit trail
        await _auditService.LogActionAsync(
            "Opportunity",
            notification.OpportunityId,
            "Submitted",
            notification.SubmittedBy,
            "Opportunity submitted for Asset Manager review",
            cancellationToken);

        // Send notification to Asset Manager
        await _notificationService.SendSubmittedNotificationAsync(
            "opportunities",
            notification.OpportunityId,
            notification.CompanyId,
            notification.SubmittedBy,
            notification.OpportunityName,
            notification.CompanyName,
            cancellationToken);
    }
}
