using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Services;
using PartnersHub.InfraBase.Domain.Events;

namespace PartnersHub.InfraBase.Application.Assets.EventHandlers;

public class AssetAcceptedByPcAdminEventHandler : INotificationHandler<AssetAcceptedByPcAdminEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly IConfiguration _configuration;
    private readonly EmailTemplateService _emailTemplateService;
    private readonly ILogger<AssetAcceptedByPcAdminEventHandler> _logger;

    public AssetAcceptedByPcAdminEventHandler(
        INotificationService notificationService,
        IMiddlewareIntegrationService middlewareService,
        IConfiguration configuration,
        EmailTemplateService emailTemplateService,
        ILogger<AssetAcceptedByPcAdminEventHandler> logger)
    {
        _notificationService = notificationService;
        _middlewareService = middlewareService;
        _configuration = configuration;
        _emailTemplateService = emailTemplateService;
        _logger = logger;
    }

    public async Task Handle(AssetAcceptedByPcAdminEvent notification, CancellationToken cancellationToken)
    {
        // Send email to contributor (creator)
        await SendEmailToContributor(notification, cancellationToken);
        
        // Send email to Infrabase admin
        await SendEmailToInfrabaseAdmin(notification, cancellationToken);
    }

    private async Task SendEmailToContributor(AssetAcceptedByPcAdminEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(notification.CreatedBy))
            {
                _logger.LogWarning(
                    "Cannot send email notification to contributor for asset {AssetId} acceptance: CreatedBy is empty",
                    notification.AssetId);
                
                // Still send in-app notification
                await SendInAppNotification(notification, cancellationToken);
                return;
            }

            // Build email body with HTML
            var emailBody = _emailTemplateService.BuildAssetAcceptedByPcAdminEmail(notification.AssetId);
            
            // Send email notification to contributor (creator)
            await _notificationService.SendEmailAsync(
                to: notification.CreatedBy,
                subject: "Asset Accepted",
                body: emailBody,
                cancellationToken: cancellationToken);
            
            _logger.LogInformation(
                "Email notification sent to contributor {CreatedBy} for asset {AssetId} acceptance",
                notification.CreatedBy, notification.AssetId);
            
            // Send in-app notification to contributor
            await SendInAppNotification(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending notification to contributor for asset {AssetId} acceptance: {Message}",
                notification.AssetId, ex.Message);
            
            // Try to send in-app notification even if email fails
            try
            {
                await SendInAppNotification(notification, cancellationToken);
            }
            catch (Exception inAppEx)
            {
                _logger.LogError(inAppEx,
                    "Failed to send in-app notification for asset {AssetId} acceptance: {Message}",
                    notification.AssetId, inAppEx.Message);
            }
        }
    }

    private async Task SendEmailToInfrabaseAdmin(AssetAcceptedByPcAdminEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get Infrabase admin emails from configuration
            var infrabaseAdminEmails = _configuration["NotificationSettings:InfrabaseAdminEmails"];
            
            if (string.IsNullOrWhiteSpace(infrabaseAdminEmails))
            {
                _logger.LogWarning(
                    "Cannot send email notification to Infrabase admin for asset {AssetId}: InfrabaseAdminEmails configuration is missing",
                    notification.AssetId);
                return;
            }

            // Get company information to get company name
            var company = await _middlewareService.GetCompanyByIdAsync(notification.CompanyId);
            var companyName = company?.Name ?? "Unknown Company";
            
            // Build email body with HTML
            var emailBody = _emailTemplateService.BuildNewRequestSubmittedEmail(companyName, notification.AssetId);
            
            // Split emails by comma and send to all Infrabase admins
            var emailList = infrabaseAdminEmails
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .ToList();
            
            if (emailList.Count == 0)
            {
                _logger.LogWarning(
                    "No valid Infrabase admin emails found in configuration for asset {AssetId}",
                    notification.AssetId);
                return;
            }

            // Send email to all Infrabase admins
            await _notificationService.SendEmailToMultipleAsync(
                recipients: emailList,
                subject: "New request submitted",
                body: emailBody,
                cancellationToken: cancellationToken);
            
            _logger.LogInformation(
                "Email notification sent to Infrabase admin(s) {Emails} for asset {AssetId} accepted by PC admin",
                string.Join(", ", emailList), notification.AssetId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending notification to Infrabase admin for asset {AssetId} acceptance: {Message}",
                notification.AssetId, ex.Message);
        }
    }

    private async Task SendInAppNotification(AssetAcceptedByPcAdminEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.CreatedBy,
            title: "Asset Accepted by PC Admin",
            message: $"Asset {notification.AssetCode} has been accepted by PC Admin.",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetApproval",
            cancellationToken: cancellationToken);
    }
}
