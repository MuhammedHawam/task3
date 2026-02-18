using MediatR;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Common.Helpers;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Services;
using PartnersHub.InfraBase.Domain.Events;

namespace PartnersHub.InfraBase.Application.Assets.EventHandlers;

public class AssetCheckedByInfrabaseAdminEventHandler : INotificationHandler<AssetCheckedByInfrabaseAdminEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly EmailTemplateService _emailTemplateService;
    private readonly ILogger<AssetCheckedByInfrabaseAdminEventHandler> _logger;

    public AssetCheckedByInfrabaseAdminEventHandler(
        INotificationService notificationService,
        IMiddlewareIntegrationService middlewareService,
        EmailTemplateService emailTemplateService,
        ILogger<AssetCheckedByInfrabaseAdminEventHandler> logger)
    {
        _notificationService = notificationService;
        _middlewareService = middlewareService;
        _emailTemplateService = emailTemplateService;
        _logger = logger;
    }

    public async Task Handle(AssetCheckedByInfrabaseAdminEvent notification, CancellationToken cancellationToken)
    {
        // Send email to PC contributor (creator)
        await SendEmailToContributor(notification, cancellationToken);
        
        // Send email to PC admin (Representative)
        await SendEmailToPcAdmin(notification, cancellationToken);
    }

    private async Task SendEmailToContributor(AssetCheckedByInfrabaseAdminEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(notification.CreatedBy))
            {
                _logger.LogWarning(
                    "Cannot send email notification to contributor for asset {AssetId} acceptance: CreatedBy is empty",
                    notification.AssetId);
                
                // Still send in-app notification
                await SendInAppNotificationToContributor(notification, cancellationToken);
                return;
            }

            // Build email body with HTML
            var emailBody = _emailTemplateService.BuildAssetAcceptedByInfrabaseAdminEmail(notification.AssetId);
            
            // Send email notification to contributor (creator)
            await _notificationService.SendEmailAsync(
                to: notification.CreatedBy,
                subject: "Asset Accepted",
                body: emailBody,
                cancellationToken: cancellationToken);
            
            _logger.LogInformation(
                "Email notification sent to contributor {CreatedBy} for asset {AssetId} acceptance by Infrabase admin",
                notification.CreatedBy, notification.AssetId);
            
            // Send in-app notification to contributor
            await SendInAppNotificationToContributor(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending notification to contributor for asset {AssetId} acceptance: {Message}",
                notification.AssetId, ex.Message);
            
            // Try to send in-app notification even if email fails
            try
            {
                await SendInAppNotificationToContributor(notification, cancellationToken);
            }
            catch (Exception inAppEx)
            {
                _logger.LogError(inAppEx,
                    "Failed to send in-app notification for asset {AssetId} acceptance: {Message}",
                    notification.AssetId, inAppEx.Message);
            }
        }
    }

    private async Task SendEmailToPcAdmin(AssetCheckedByInfrabaseAdminEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get company information to find PC admin (Representative)
            var company = await _middlewareService.GetCompanyByIdAsync(notification.CompanyId);
            
            if (company?.Representative == null || string.IsNullOrWhiteSpace(company.Representative.Email))
            {
                _logger.LogWarning(
                    "Cannot send email notification to PC admin for asset {AssetId} acceptance: Company {CompanyId} does not have a representative with email",
                    notification.AssetId, notification.CompanyId);
                return;
            }

            var pcAdminEmail = company.Representative.Email;
            
            // Build email body with HTML
            var emailBody = _emailTemplateService.BuildAssetAcceptedByInfrabaseAdminEmail(notification.AssetId);
            
            // Send email notification to PC admin
            await _notificationService.SendEmailAsync(
                to: pcAdminEmail,
                subject: "Asset Accepted",
                body: emailBody,
                cancellationToken: cancellationToken);
            
            _logger.LogInformation(
                "Email notification sent to PC admin {PcAdminEmail} for asset {AssetId} acceptance by Infrabase admin",
                pcAdminEmail, notification.AssetId);
            
            // Send in-app notification to PC admin
            await SendInAppNotificationToPcAdmin(notification, pcAdminEmail, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending notification to PC admin for asset {AssetId} acceptance: {Message}",
                notification.AssetId, ex.Message);
        }
    }

    private async Task SendInAppNotificationToContributor(AssetCheckedByInfrabaseAdminEvent notification, CancellationToken cancellationToken)
    {
        var checkerName = ResolveActorDisplayName(notification.CheckedBy, "Infrabase Admin");
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.CreatedBy,
            title: "Asset Approved",
            message: $"Asset {notification.AssetCode} has been approved by {checkerName}.",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetFinalApproval",
            cancellationToken: cancellationToken);
    }

    private async Task SendInAppNotificationToPcAdmin(AssetCheckedByInfrabaseAdminEvent notification, string pcAdminEmail, CancellationToken cancellationToken)
    {
        var checkerName = ResolveActorDisplayName(notification.CheckedBy, "Infrabase Admin");
        await _notificationService.CreateInAppNotificationAsync(
            userId: pcAdminEmail,
            title: "Asset Approved",
            message: $"Asset {notification.AssetCode} has been approved by {checkerName}.",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetFinalApproval",
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
