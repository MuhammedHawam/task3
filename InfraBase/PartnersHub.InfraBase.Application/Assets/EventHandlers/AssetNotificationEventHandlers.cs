using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Domain.Events;

namespace PartnersHub.InfraBase.Application.Assets.EventHandlers;

public class AssetSubmittedEventHandler : INotificationHandler<AssetSubmittedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly ILogger<AssetSubmittedEventHandler> _logger;

    public AssetSubmittedEventHandler(
        INotificationService notificationService,
        IMiddlewareIntegrationService middlewareService,
        ILogger<AssetSubmittedEventHandler> logger)
    {
        _notificationService = notificationService;
        _middlewareService = middlewareService;
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
                
                // Still send in-app notification even if email fails
                await SendInAppNotification(notification, cancellationToken);
                return;
            }

            var pcAdminEmail = company.Representative.Email;
            
            // Extract creator name from email (part before @) or use email
            var creatorName = ExtractNameFromEmail(notification.CreatedBy);
            
            // Build email body with HTML
            var emailBody = BuildEmailBody(notification, creatorName);
            
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
                    var creatorName = ExtractNameFromEmail(notification.CreatedBy);
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

    private string ExtractNameFromEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "User";
        
        // Extract name from email (part before @) and format it
        var parts = email.Split('@');
        if (parts.Length > 0)
        {
            var namePart = parts[0];
            // Replace dots and underscores with spaces, and capitalize
            namePart = namePart.Replace('.', ' ').Replace('_', ' ');
            // Capitalize first letter of each word
            var words = namePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }
            return string.Join(" ", words);
        }
        
        return email;
    }

    private string BuildEmailBody(AssetSubmittedEvent notification, string creatorName)
    {
        // Build the asset details URL - adjust the base URL as needed
        var assetDetailsUrl = $"/assets/{notification.AssetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New asset submitted</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #007bff;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #007bff; margin: 0;"">New asset submitted</h1>
        </div>
        <div class=""content"">
            <p>New asset submitted by ""{creatorName}"" and waiting your approval</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">View Asset</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }
}

public class AssetRejectedByPcAdminEventHandler : INotificationHandler<AssetRejectedByPcAdminEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<AssetRejectedByPcAdminEventHandler> _logger;

    public AssetRejectedByPcAdminEventHandler(
        INotificationService notificationService,
        ILogger<AssetRejectedByPcAdminEventHandler> logger)
    {
        _notificationService = notificationService;
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
            var emailBody = BuildRejectionEmailBody(notification);
            
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
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.CreatedBy,
            title: "Asset Rejected by PC Admin",
            message: $"Asset {notification.AssetCode} was rejected. Reason: {notification.RejectionReason}",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetRejection",
            cancellationToken: cancellationToken);
    }

    private string BuildRejectionEmailBody(AssetRejectedByPcAdminEvent notification)
    {
        var assetDetailsUrl = $"/assets/{notification.AssetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Asset Rejected</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #dc3545;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #dc3545; margin: 0;"">Asset Rejected</h1>
        </div>
        <div class=""content"">
            <p>Your asset has been Rejected</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">View Asset</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }
}

public class AssetAcceptedByPcAdminEventHandler : INotificationHandler<AssetAcceptedByPcAdminEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AssetAcceptedByPcAdminEventHandler> _logger;

    public AssetAcceptedByPcAdminEventHandler(
        INotificationService notificationService,
        IMiddlewareIntegrationService middlewareService,
        IConfiguration configuration,
        ILogger<AssetAcceptedByPcAdminEventHandler> logger)
    {
        _notificationService = notificationService;
        _middlewareService = middlewareService;
        _configuration = configuration;
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
            var emailBody = BuildAcceptanceEmailBody(notification);
            
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
            var emailBody = BuildInfrabaseAdminEmailBody(notification, companyName);
            
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

    private string BuildAcceptanceEmailBody(AssetAcceptedByPcAdminEvent notification)
    {
        var assetDetailsUrl = $"/assets/{notification.AssetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Asset Accepted</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #28a745;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #28a745; margin: 0;"">Asset Accepted</h1>
        </div>
        <div class=""content"">
            <p>Your asset has been Approved</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">View Asset</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
    </html>";
    }

    private string BuildInfrabaseAdminEmailBody(AssetAcceptedByPcAdminEvent notification, string companyName)
    {
        var assetDetailsUrl = $"/assets/{notification.AssetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New request submitted</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #007bff;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #007bff; margin: 0;"">New request submitted</h1>
        </div>
        <div class=""content"">
            <p>New request submitted by ""{companyName}"" and waiting your approval</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">Approve Request</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }
}

public class AssetReturnedForCorrectionEventHandler : INotificationHandler<AssetReturnedForCorrectionByInfrabaseAdminEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly ILogger<AssetReturnedForCorrectionEventHandler> _logger;

    public AssetReturnedForCorrectionEventHandler(
        INotificationService notificationService,
        IMiddlewareIntegrationService middlewareService,
        ILogger<AssetReturnedForCorrectionEventHandler> logger)
    {
        _notificationService = notificationService;
        _middlewareService = middlewareService;
        _logger = logger;
    }

    public async Task Handle(AssetReturnedForCorrectionByInfrabaseAdminEvent notification, CancellationToken cancellationToken)
    {
        // Send email to PC contributor (creator)
        await SendEmailToContributor(notification, cancellationToken);
        
        // Send email to PC admin (Representative)
        await SendEmailToPcAdmin(notification, cancellationToken);
    }

    private async Task SendEmailToContributor(AssetReturnedForCorrectionByInfrabaseAdminEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(notification.CreatedBy))
            {
                _logger.LogWarning(
                    "Cannot send email notification to contributor for asset {AssetId} rejection: CreatedBy is empty",
                    notification.AssetId);
                
                // Still send in-app notification
                await SendInAppNotificationToContributor(notification, cancellationToken);
                return;
            }

            // Build email body with HTML
            var emailBody = BuildRejectionEmailBody(notification);
            
            // Send email notification to contributor (creator)
            await _notificationService.SendEmailAsync(
                to: notification.CreatedBy,
                subject: "Asset Rejected",
                body: emailBody,
                cancellationToken: cancellationToken);
            
            _logger.LogInformation(
                "Email notification sent to contributor {CreatedBy} for asset {AssetId} rejection by Infrabase admin",
                notification.CreatedBy, notification.AssetId);
            
            // Send in-app notification to contributor
            await SendInAppNotificationToContributor(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending notification to contributor for asset {AssetId} rejection: {Message}",
                notification.AssetId, ex.Message);
            
            // Try to send in-app notification even if email fails
            try
            {
                await SendInAppNotificationToContributor(notification, cancellationToken);
            }
            catch (Exception inAppEx)
            {
                _logger.LogError(inAppEx,
                    "Failed to send in-app notification for asset {AssetId} rejection: {Message}",
                    notification.AssetId, inAppEx.Message);
            }
        }
    }

    private async Task SendEmailToPcAdmin(AssetReturnedForCorrectionByInfrabaseAdminEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Get company information to find PC admin (Representative)
            var company = await _middlewareService.GetCompanyByIdAsync(notification.CompanyId);
            
            if (company?.Representative == null || string.IsNullOrWhiteSpace(company.Representative.Email))
            {
                _logger.LogWarning(
                    "Cannot send email notification to PC admin for asset {AssetId} rejection: Company {CompanyId} does not have a representative with email",
                    notification.AssetId, notification.CompanyId);
                return;
            }

            var pcAdminEmail = company.Representative.Email;
            
            // Build email body with HTML
            var emailBody = BuildRejectionEmailBody(notification);
            
            // Send email notification to PC admin
            await _notificationService.SendEmailAsync(
                to: pcAdminEmail,
                subject: "Asset Rejected",
                body: emailBody,
                cancellationToken: cancellationToken);
            
            _logger.LogInformation(
                "Email notification sent to PC admin {PcAdminEmail} for asset {AssetId} rejection by Infrabase admin",
                pcAdminEmail, notification.AssetId);
            
            // Send in-app notification to PC admin
            await SendInAppNotificationToPcAdmin(notification, pcAdminEmail, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending notification to PC admin for asset {AssetId} rejection: {Message}",
                notification.AssetId, ex.Message);
        }
    }

    private async Task SendInAppNotificationToContributor(AssetReturnedForCorrectionByInfrabaseAdminEvent notification, CancellationToken cancellationToken)
    {
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.CreatedBy,
            title: "Asset Returned for Correction",
            message: $"Asset {notification.AssetCode} needs corrections. Reason: {notification.CorrectionReason}",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetCorrection",
            cancellationToken: cancellationToken);
    }

    private async Task SendInAppNotificationToPcAdmin(AssetReturnedForCorrectionByInfrabaseAdminEvent notification, string pcAdminEmail, CancellationToken cancellationToken)
    {
        await _notificationService.CreateInAppNotificationAsync(
            userId: pcAdminEmail,
            title: "Asset Returned for Correction",
            message: $"Asset {notification.AssetCode} needs corrections. Reason: {notification.CorrectionReason}",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetCorrection",
            cancellationToken: cancellationToken);
    }

    private string BuildRejectionEmailBody(AssetReturnedForCorrectionByInfrabaseAdminEvent notification)
    {
        var assetDetailsUrl = $"/assets/{notification.AssetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Asset Rejected</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #dc3545;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #dc3545; margin: 0;"">Asset Rejected</h1>
        </div>
        <div class=""content"">
            <p>Your asset has been Rejected</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">View Asset</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }
}

public class AssetCheckedByInfrabaseAdminEventHandler : INotificationHandler<AssetCheckedByInfrabaseAdminEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly ILogger<AssetCheckedByInfrabaseAdminEventHandler> _logger;

    public AssetCheckedByInfrabaseAdminEventHandler(
        INotificationService notificationService,
        IMiddlewareIntegrationService middlewareService,
        ILogger<AssetCheckedByInfrabaseAdminEventHandler> logger)
    {
        _notificationService = notificationService;
        _middlewareService = middlewareService;
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
            var emailBody = BuildAcceptanceEmailBody(notification);
            
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
            var emailBody = BuildAcceptanceEmailBody(notification);
            
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
        await _notificationService.CreateInAppNotificationAsync(
            userId: notification.CreatedBy,
            title: "Asset Approved",
            message: $"Asset {notification.AssetCode} has been approved by Infrabase Admin.",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetFinalApproval",
            cancellationToken: cancellationToken);
    }

    private async Task SendInAppNotificationToPcAdmin(AssetCheckedByInfrabaseAdminEvent notification, string pcAdminEmail, CancellationToken cancellationToken)
    {
        await _notificationService.CreateInAppNotificationAsync(
            userId: pcAdminEmail,
            title: "Asset Approved",
            message: $"Asset {notification.AssetCode} has been approved by Infrabase Admin.",
            link: $"/assets/{notification.AssetId}",
            notificationType: "AssetFinalApproval",
            cancellationToken: cancellationToken);
    }

    private string BuildAcceptanceEmailBody(AssetCheckedByInfrabaseAdminEvent notification)
    {
        var assetDetailsUrl = $"/assets/{notification.AssetId}";
        
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Asset Accepted</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .email-container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #28a745;
            padding-bottom: 20px;
            margin-bottom: 20px;
        }}
        .content {{
            margin: 20px 0;
        }}
        .button-container {{
            text-align: center;
            margin: 30px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            font-size: 16px;
        }}
        .button:hover {{
            background-color: #0056b3;
        }}
        .footer {{
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h1 style=""color: #28a745; margin: 0;"">Asset Accepted</h1>
        </div>
        <div class=""content"">
            <p>Your asset has been Approved</p>
        </div>
        <div class=""button-container"">
            <a href=""{assetDetailsUrl}"" class=""button"">View Asset</a>
        </div>
        <div class=""footer"">
            <p>Regards.<br>Infrabase team</p>
        </div>
    </div>
</body>
</html>";
    }
}
