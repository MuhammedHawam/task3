using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.Events;

namespace PartnersHub.Synergy.Application.SuccessStories.EventHandlers;

public class SuccessStoryApprovedEventHandler : INotificationHandler<SuccessStoryApprovedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;

    public SuccessStoryApprovedEventHandler(
        INotificationService notificationService,
        IAuditService auditService)
    {
        _notificationService = notificationService;
        _auditService = auditService;
    }

    public async Task Handle(SuccessStoryApprovedEvent notification, CancellationToken cancellationToken)
    {
        // Determine the approval level and log accordingly
        string action;
        string comments;
        
        if (notification.NewStatus == SuccessStoryStatus.pending)
        {
            action = "Approved by Asset Manager";
            comments = "Success Story approved by Asset Manager, pending Synergy Admin review";
            
            // Notify PC Representative
            await _notificationService.SendApprovedByAssetManagerNotificationAsync(
                "SuccessStory",
                notification.SuccessStoryId,
                notification.CompanyId,
                notification.ApprovedBy,
                notification.SuccessStoryName,
                notification.CompanyName,
                cancellationToken);
        }
        else if (notification.NewStatus == SuccessStoryStatus.Published)
        {
            action = "Published";
            comments = "Success Story published by Synergy Admin";
            
            // Notify PC Representative
            await _notificationService.SendPublishedNotificationAsync(
                "SuccessStory",
                notification.SuccessStoryId,
                notification.CompanyId,
                notification.ApprovedBy,
                notification.SuccessStoryName,
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
            "SuccessStory",
            notification.SuccessStoryId,
            action,
            notification.ApprovedBy,
            comments,
            cancellationToken);
    }
}
