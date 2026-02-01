using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.ValueObjects;
using System.Net.Http;
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
    private readonly ISynergyCompanyRepository _synergyCompanyRepository;
    private const string TemplateFolderName = "Templates/Emails/Images";


    public NotificationService(IOptions<EmailParameters> options,
                               ILogger<NotificationService> logger, 
                               IHttpClientFactory httpClientFactory,
                               IHttpContextAccessor httpContextAccessor,
                               ISynergyCompanyRepository synergyCompanyRepository)
    {
        _emailParams = options.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _synergyCompanyRepository = synergyCompanyRepository;
    }

    private async Task SendEmail(EmailNotificationModel emailDto)
    {
        _logger.LogInformation("Sending email notification. Payload: {@EmailDto}",emailDto);

        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Authorization header is missing or invalid.");
            throw new UnauthorizedAccessException("Missing or invalid authorization token.");
        }

        var httpClient = _httpClientFactory.CreateClient(Constants.NotificationClient);

        httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

        if (!httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        try
        {
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

        templateContent = InlineImage(templateContent, "BG.png");
        templateContent = InlineImage(templateContent, "PIF_Logo.png");
        templateContent = InlineImage(templateContent, "PIF.png");
        templateContent = InlineImage(templateContent, "Platform_Logo.png");

        return templateContent;
    }

    private static string InlineImage(string html, string fileName)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, TemplateFolderName, fileName);
        if (!File.Exists(filePath))
        {
            return html;
        }

        var base64 = Convert.ToBase64String(File.ReadAllBytes(filePath));
        var dataUri = $"data:image/png;base64,{base64}";

        return html.Replace($"./Images/{fileName}", dataUri, StringComparison.Ordinal);
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

        var assetManager = _synergyCompanyRepository.GetByIdAsync(companyId);

       var subjectTitle =  moduleName.ToLowerInvariant() switch
                             {
                                 "opportunity"  => _emailParams.OpportunitySubmittedSubject,
                                 "successstory" => _emailParams.SuccessStorySubmittedSubject,
                                              _ => _emailParams.OpportunitySubmittedSubject
                             };

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string>{assetManager?.Result?.CompanyPIFOwnerEmail, assetManager?.Result?.CompanyPIFOwnerSupervisorEmail, _emailParams.SynergyModuleCC}
                                  .Where(email => !string.IsNullOrWhiteSpace(email)).ToList(),
            subject = subjectTitle,
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

        var subjectTitle = moduleName.ToLowerInvariant() switch
        {
            "opportunity" => _emailParams.OpportunityApprovedSubject,
            "successstory" => _emailParams.SuccessStoryApprovedSubject,
            _ => _emailParams.OpportunityApprovedSubject
        };

        await SendEmail(new EmailNotificationModel
        {
            to = (_emailParams.SynergyTeam.Select(e => e.Email).ToList()),
            subject = subjectTitle,
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

        var subjectTitle = moduleName.ToLowerInvariant() switch
        {
            "opportunity" => _emailParams.OpportunityPublishedSubject,
            "successstory" => _emailParams.SuccessStoryPublishedSubject,
            _ => _emailParams.OpportunityPublishedSubject
        };

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { companyEmail, _emailParams.SynergyModuleCC }.Where(email => !string.IsNullOrWhiteSpace(email)).ToList(),
            subject = subjectTitle,
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

        var subjectTitle = moduleName.ToLowerInvariant() switch
        {
            "opportunity" => _emailParams.OpportunityRejectedSubject,
            "successstory" => _emailParams.SuccessStoryRejectedSubject,
            _ => _emailParams.OpportunityRejectedSubject
        };

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { companyEmail, _emailParams.SynergyModuleCC }.Where(email => !string.IsNullOrWhiteSpace(email)).ToList(),
            subject = subjectTitle,
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

        var subjectTitle = moduleName.ToLowerInvariant() switch
        {
            "opportunity" => _emailParams.OpportunityUpdatedSubject,
            "successstory" => _emailParams.SuccessStoryUpdatedSubject,
            _ => _emailParams.OpportunityUpdatedSubject
        };

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { companyEmail, _emailParams.SynergyModuleCC }.Where(email => !string.IsNullOrWhiteSpace(email)).ToList(),
            subject = subjectTitle,
            body = emailBody,
            isHtml = true
        });
    }


}
