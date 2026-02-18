using MediatR;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Common.Helpers;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Services;
using PartnersHub.InfraBase.Domain.Events;

namespace PartnersHub.InfraBase.Application.Assets.EventHandlers;

public class AssetRejectedByPcAdminEventHandler : INotificationHandler<AssetRejectedByPcAdminEvent>
{
    private readonly INotificationService _notificationService;
    private readonly EmailTemplateService _emailTemplateService;
    private readonly ILogger<AssetRejectedByPcAdminEventHandler> _logger;

    public AssetRejectedByPcAdminEventHandler(
        INotificationService notificationService,
        EmailTemplateService emailTemplateService,
        ILogger<AssetRejectedByPcAdminEventHandler> logger)
    {
        _notificationService = notificationService;
        _emailTemplateService = emailTemplateService;
        _logger = logger;
    }

    public async Task Handle(AssetRejectedByPcAdminEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(notification.CreatedBy))
            {
                _logger.LogWarning(
                    "Cannot send email notification for asset {AssetId} rejection: CreatedBy is empty",
                    notification.AssetId);
                
                // Still send in-app notification
                await SendInAppNotification(notification, cancellationToken);
                return;
            }

            // Build email body with HTML
            var emailBody = _emailTemplateService.BuildAssetRejectedByPcAdminEmail(notification.AssetId);
            
            // Send email notification to contributor (creator)
            await _notificationService.SendEmailAsync(
                to: notification.CreatedBy,
                subject: "Asset Rejected",
                body: emailBody,
                cancellationToken: cancellationToken);
            
            _logger.LogInformation(
                "Email notification sent to contributor {CreatedBy} for asset {AssetId} rejection",
                notification.CreatedBy, notification.AssetId);
            
            // Send in-app notification to contributor
            await SendInAppNotification(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending notification for asset {AssetId} rejection: {Message}",
                notification.AssetId, ex.Message);
            
            // Try to send in-app notification even if email fails
            try
            {
                await SendInAppNotification(notification, cancellationToken);
            }
            catch (Exception inAppEx)
            {
                _logger.LogError(inAppEx,
                    "Failed to send in-app notification for asset {AssetId} rejection: {Message}",
                    notification.AssetId, inAppEx.Message);
            }
        }
    }

    private async Task SendInAppNotification(AssetRejectedByPcAdminEvent notification, CancellationToken cancellationToken)
    {
        var rejectorName = ResolveActorDisplayName(notification.RejectedBy, "PC Admin");
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.CreatedBy,
            title: "Asset Rejected by PC Admin",
            message: $"Asset {notification.AssetCode} was rejected by {rejectorName}. Reason: {notification.RejectionReason}",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetRejection",
            cancellationToken: cancellationToken);
    }

    private static string ResolveActorDisplayName(string? actorValue, string fallbackRoleName)
    {
        var normalized = actorValue?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || Guid.TryParse(normalized, out _))
        {
            return fallbackRoleName;
        }

        return normalized.Contains('@')
            ? EmailHelper.ExtractNameFromEmail(normalized)
            : normalized;
    }
}
