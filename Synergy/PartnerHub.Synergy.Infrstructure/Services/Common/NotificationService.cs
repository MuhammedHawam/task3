using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.ValueObjects;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;

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

    public async Task SendSubmittedNotificationAsync(string moduleName, Guid Id, Guid companyId, Guid submitterId,string name,string? companyName ,CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{moduleName} {Id} submitted. Notification sent.", moduleName, Id);

        var placeholders = new Dictionary<string, string>
                              {
                                { "{PC Name}", companyName },
                                { "{Submission Type}", moduleName },  
                                { "{Title}", name },  
                                { "{Date}", DateTime.UtcNow.ToString("yyyy-MM-dd") },  
                                { "{BaseURL}", _emailParams.BaseURL },  
                                { "{module}", moduleName },  
                                { "{request-id}", Id.ToString() }  
                              };

        var emailBody = LoadTemplate("Submitted.html", placeholders);

        var assetManager = _emailParams.AssetManagersList.FirstOrDefault(x => x.PCName.Equals(companyName, StringComparison.OrdinalIgnoreCase));

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { assetManager?.AssetManagerEmail },
            subject = _emailParams.OpportunitySubmittedSubject,
            body = emailBody,
            isHtml = true
        });
    }

    public async Task SendApprovedByAssetManagerNotificationAsync(string moduleName, Guid Id, Guid companyId, Guid approverId,string name, string? companyName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{moduleName} {Id} approved. Notification sent.", moduleName, Id);

        var placeholders = new Dictionary<string, string>
                         {
                           { "{PC Name}", companyName },
                           { "{Submission Type}", moduleName }, 
                           { "{Title}", name },
                           { "{BaseURL}", _emailParams.BaseURL },
                           { "{module}", moduleName},
                           { "{request-id}", Id.ToString() }
                         };

        var emailBody = LoadTemplate("PendingFinalApproval.html", placeholders);


        await SendEmail(new EmailNotificationModel
        {
            to = _emailParams.SynergyTeam.Select(e => e.Email).ToList(),
            subject = _emailParams.OpportunityApprovedSubject,
            body = emailBody,
            isHtml = true
        });
    }

    public async Task SendPublishedNotificationAsync(string moduleName, Guid Id, Guid companyId, Guid publisherId, string name, string? companyName,string? companyEmail, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{moduleName} {Id} published. Notification sent.", moduleName, Id);
        var placeholders = new Dictionary<string, string>
                            {
                               { "{PC Name}", companyName },
                               { "{Submission Type}", moduleName }, 
                               { "{Title}", name },
                               { "{BaseURL}", _emailParams.BaseURL },
                               { "{module}", moduleName},
                               { "{request-id}", Id.ToString() }
                            };

        var emailBody = LoadTemplate("FinalApproved.html", placeholders);
        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { companyEmail ?? ""},
            subject = _emailParams.OpportunityPublishedSubject,
            body = emailBody,
            isHtml = true
        });
    }

    public async Task SendRejectedNotificationAsync(string moduleName, Guid Id, Guid companyId, string rejectionReason, Guid rejecterId, string name, string? companyName, string? companyEmail, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{moduleName} {Id} rejected. Reason: {RejectionReason}.", moduleName, Id, rejectionReason);


        var placeholders = new Dictionary<string, string>
                         {
                           { "{PC Name}", companyName },
                           { "{Submission Type}", moduleName },
                           { "{Title}", name },
                           { "{Reason}", rejectionReason ?? "—" },
                           { "{BaseURL}", _emailParams.BaseURL },
                           { "{module}", moduleName},
                           { "{request-id}", Id.ToString() }
                         };

        var emailBody = LoadTemplate("Rejected.html", placeholders);

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { companyEmail ?? "" },
            subject = _emailParams.OpportunityRejectedSubject,
            body = emailBody,
            isHtml = true
        });
    }


    public async Task SendUpdatedNotificationAsync(string moduleName, Guid Id, Guid companyId,  string title, string companyName, string companyEmail, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{moduleName} {Id} updated.", moduleName, Id);


        var placeholders = new Dictionary<string, string>
                         {
                           { "{PC Name}", companyName },
                           { "{Submission Type}", moduleName },
                           { "{Title}", title },
                           { "{BaseURL}", _emailParams.BaseURL },
                           { "{module}", moduleName},
                           { "{request-id}", Id.ToString() }
                         };

        var emailBody = LoadTemplate("Updated.html", placeholders);

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { companyEmail  },
            subject = _emailParams.OpportunityRejectedSubject,
            body = emailBody,
            isHtml = true
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
