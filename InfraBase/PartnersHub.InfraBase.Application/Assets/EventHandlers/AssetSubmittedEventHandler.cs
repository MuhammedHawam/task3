using MediatR;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Common.Helpers;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Services;
using PartnersHub.InfraBase.Domain.Events;

namespace PartnersHub.InfraBase.Application.Assets.EventHandlers;

public class AssetSubmittedEventHandler : INotificationHandler<AssetSubmittedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly EmailTemplateService _emailTemplateService;
    private readonly ILogger<AssetSubmittedEventHandler> _logger;

    public AssetSubmittedEventHandler(
        INotificationService notificationService,
        IMiddlewareIntegrationService middlewareService,
        EmailTemplateService emailTemplateService,
        ILogger<AssetSubmittedEventHandler> logger)
    {
        _notificationService = notificationService;
        _middlewareService = middlewareService;
        _emailTemplateService = emailTemplateService;
        _logger = logger;
    }

    public async Task Handle(AssetSubmittedEvent notification, CancellationToken cancellationToken)
    {
        // Only send email notification when a contributor submits (not when PC admin submits)
        if (!notification.IsContributorSubmission)
        {
            _logger.LogInformation(
                "Skipping email notification for asset {AssetId}: Submitted by PC admin, not a contributor submission",
                notification.AssetId);
            return;
        }

        try
        {
            // Get company information to find PC admin (Representative)
            var company = await _middlewareService.GetCompanyByIdAsync(notification.CompanyId);
            
            if (company?.Representative == null || string.IsNullOrWhiteSpace(company.Representative.Email))
            {
                _logger.LogWarning(
                    "Cannot send email notification for asset {AssetId}: Company {CompanyId} does not have a representative with email",
                    notification.AssetId, notification.CompanyId);
                return;
            }

            var pcAdminEmail = company.Representative.Email;
            
            // Extract creator name from email (part before @) or use email
            var creatorName = EmailHelper.ExtractNameFromEmail(notification.CreatedBy);
            
            // Build email body with HTML
            var emailBody = _emailTemplateService.BuildAssetSubmittedEmail(creatorName, notification.AssetId);
            
            // Send email notification to PC admin
            await _notificationService.SendEmailAsync(
                to: pcAdminEmail,
                subject: "New asset submitted",
                body: emailBody,
                cancellationToken: cancellationToken);
            
            _logger.LogInformation(
                "Email notification sent to PC admin {PcAdminEmail} for asset {AssetId} submitted by {CreatedBy}",
                pcAdminEmail, notification.AssetId, notification.CreatedBy);
            
            // Send in-app notification to PC admin
            await SendInAppNotificationToPcAdmin(notification, pcAdminEmail, creatorName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending notification for asset {AssetId} submission: {Message}",
                notification.AssetId, ex.Message);
            
            // Try to send in-app notification even if email fails
            try
            {
                var company = await _middlewareService.GetCompanyByIdAsync(notification.CompanyId);
                if (company?.Representative != null && !string.IsNullOrWhiteSpace(company.Representative.Email))
                {
                    var creatorName = EmailHelper.ExtractNameFromEmail(notification.CreatedBy);
                    await SendInAppNotificationToPcAdmin(notification, company.Representative.Email, creatorName, cancellationToken);
                }
            }
            catch (Exception inAppEx)
            {
                _logger.LogError(inAppEx,
                    "Failed to send in-app notification for asset {AssetId}: {Message}",
                    notification.AssetId, inAppEx.Message);
            }
        }
    }

    private async Task SendInAppNotificationToPcAdmin(AssetSubmittedEvent notification, string pcAdminEmail, string creatorName, CancellationToken cancellationToken)
    {
        await _notificationService.CreateInAppNotificationAsync(
            userId: pcAdminEmail,
            title: "New Asset Submitted for Review",
            message: $"Asset {notification.AssetCode} has been submitted by {creatorName} and requires your approval.",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetSubmission",
            cancellationToken: cancellationToken);
    }
}
