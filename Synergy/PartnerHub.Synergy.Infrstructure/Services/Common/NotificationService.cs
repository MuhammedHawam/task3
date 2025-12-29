using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Domain.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PartnersHub.Synergy.Infrastructure.Services.Common;

/// <summary>
/// Notification service implementation
/// TODO: Integrate with actual notification system (Email, SMS, Push notifications)
/// </summary>
public class NotificationService : INotificationService
{
    private readonly EmailParameters _emailParams;
    private readonly ILogger<NotificationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NotificationService(IOptions<EmailParameters> options, ILogger<NotificationService> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _emailParams = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    private async Task SendEmail(EmailNotificationModel emailDto)
    {
        // 1. Extract Token safely
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Authorization header is missing or invalid.");
            throw new UnauthorizedAccessException("Missing or invalid authorization token.");
        }

        // 2. Use HttpClient efficiently
        var httpClient = _httpClientFactory.CreateClient(Constants.NotificationClient);

        // Clear and set headers to avoid accumulation if the client is reused
        httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

        if (!httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        try
        {
            // 3. PostAsJsonAsync handles serialization and Content-Type headers automatically
            var response = await httpClient.PostAsJsonAsync(Constants.EmailNotificationPath, emailDto);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Email API returned {StatusCode}: {Error}", response.StatusCode, errorContent);
            }

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Network or timeout error calling notification service at {Uri}", Constants.EmailNotificationPath);
            throw;
        }
    }

    public string LoadTemplate(string templateName, Dictionary<string, string> placeholders)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "Emails", templateName);
        var templateContent = File.ReadAllText(templatePath);

        foreach (var placeholder in placeholders)
        {
            templateContent = templateContent.Replace(placeholder.Key, placeholder.Value);
        }

        return templateContent;
    }

    public async Task SendOpportunitySubmittedNotificationAsync(Guid opportunityId, Guid companyId, Guid submitterId,string opportunityName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Opportunity {OpportunityId} submitted. Notification sent.", opportunityId);

        // Prepare placeholders for both languages
        var placeholders = new Dictionary<string, string>
    {
        { "{PC Name}", opportunityName },
        { "{Submission Type}", "Collaboration" },  
        { "{Title}", opportunityName },  
        { "{Date}", DateTime.UtcNow.ToString("yyyy-MM-dd") },  
        { "{BaseURL}", _emailParams.BaseURL },  
        { "{module}", "opportunity" },  
        { "{request-id}", opportunityId.ToString() }  
    };

        // Load the email template and replace the placeholders
        var emailBody = LoadTemplate("OpportunitySubmitted.html", placeholders);

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { _emailParams.SynergyModuleReviever },
            cc = new List<string> { _emailParams.SynergyModuleCC },
            subject = _emailParams.OpportunitySubmittedSubject,
            body = emailBody,
            isHtml = true
        });
    }

    public async Task SendOpportunityApprovedByAssetManagerNotificationAsync(Guid opportunityId, Guid companyId, Guid approverId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Opportunity {OpportunityId} approved. Notification sent.", opportunityId);

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { _emailParams.SynergyModuleReviever },
            cc = new List<string> { _emailParams.SynergyModuleCC },
            subject = _emailParams.OpportunityApprovedSubject,
            body = _emailParams.OpportunityApprovedBody
        });
    }

    public async Task SendOpportunityPublishedNotificationAsync(Guid opportunityId, Guid companyId, Guid publisherId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Opportunity {OpportunityId} published. Notification sent.", opportunityId);

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { _emailParams.SynergyModuleReviever },
            cc = new List<string> { _emailParams.SynergyModuleCC },
            subject = _emailParams.OpportunityPublishedSubject,
            body = _emailParams.OpportunityPublishedBody
        });
    }

    public async Task SendOpportunityRejectedNotificationAsync(Guid opportunityId, Guid companyId, string rejectionReason, Guid rejecterId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Opportunity {OpportunityId} rejected. Reason: {RejectionReason}.", opportunityId, rejectionReason);

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { _emailParams.SynergyModuleReviever },
            cc = new List<string> { _emailParams.SynergyModuleCC },
            subject = _emailParams.OpportunityRejectedSubject,
            body = $"{_emailParams.OpportunityRejectedBody} {rejectionReason}"
        });
    }

    public async Task SendSuccessStorySubmittedNotificationAsync(Guid successStoryId, Guid companyId, Guid submitterId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Success Story {SuccessStoryId} submitted. Notification sent.", successStoryId);

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { _emailParams.SynergyModuleReviever },
            cc = new List<string> { _emailParams.SynergyModuleCC },
            subject = _emailParams.SuccessStorySubmittedSubject,
            body = _emailParams.SuccessStorySubmittedBody
        });
    }

    public async Task SendSuccessStoryApprovedByAssetManagerNotificationAsync(Guid successStoryId, Guid companyId, Guid approverId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Success Story {SuccessStoryId} approved. Notification sent.", successStoryId);

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { _emailParams.SynergyModuleReviever },
            cc = new List<string> { _emailParams.SynergyModuleCC },
            subject = _emailParams.SuccessStoryApprovedSubject,
            body = _emailParams.SuccessStoryApprovedBody
        });
    }

    public async Task SendSuccessStoryPublishedNotificationAsync(Guid successStoryId, Guid companyId, Guid publisherId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Success Story {SuccessStoryId} published. Notification sent.", successStoryId);

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { _emailParams.SynergyModuleReviever },
            cc = new List<string> { _emailParams.SynergyModuleCC },
            subject = _emailParams.SuccessStoryPublishedSubject,
            body = _emailParams.SuccessStoryPublishedBody
        });
    }

    public async Task SendSuccessStoryRejectedNotificationAsync(Guid successStoryId, Guid companyId, string rejectionReason, Guid rejecterId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Success Story {SuccessStoryId} rejected. Reason: {RejectionReason}.", successStoryId, rejectionReason);

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { _emailParams.SynergyModuleReviever },
            cc = new List<string> { _emailParams.SynergyModuleCC },
            subject = _emailParams.SuccessStoryRejectedSubject,
            body = $"{_emailParams.SuccessStoryRejectedBody} {rejectionReason}"
        });
    }
}
