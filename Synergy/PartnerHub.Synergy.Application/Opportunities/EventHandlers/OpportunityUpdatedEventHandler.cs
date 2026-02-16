using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Domain.Events;

namespace PartnersHub.Synergy.Application.Opportunities.EventHandlers;

public class OpportunityUpdatedEventHandler : INotificationHandler<OpportunityUpdatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public OpportunityUpdatedEventHandler(
        INotificationService notificationService,
        IAuditService auditService)
    {
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task Handle(OpportunityUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // Log to audit trail
        await _auditService.LogActionAsync(
            "Opportunity",
            notification.OpportunityId,
            "Updated",
            notification.UpdatedBy,
            "Opportunity Updated.",
            cancellationToken);

        // Send notification to PC Company
        await _notificationService.SendUpdatedNotificationAsync(
            "opportunities",
            notification.OpportunityId,
            notification.CompanyId,
            notification.OpportunityName,
            notification.CompanyName,
            notification.CompanyEmail,
            cancellationToken);
    }
}
