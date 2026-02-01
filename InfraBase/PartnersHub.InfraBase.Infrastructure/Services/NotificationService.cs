using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Domain.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PartnersHub.InfraBase.Infrastructure.Services;

/// <summary>
/// Placeholder notification service implementation
/// Will be replaced with actual middleware integration
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly EmailParameters _emailParams;

    public NotificationService(ILogger<NotificationService> logger, IOptions<EmailParameters> options, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _emailParams = options.Value;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private async Task SendEmail(EmailNotificationModel emailDto)
    {
        _logger.LogInformation("Sending email notification. Payload: {@EmailDto}", emailDto);
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

    public async Task SendEmailAsync(
        string to, 
        string subject, 
        string body, 
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual email sending via middleware
        _logger.LogInformation(
            "Email notification (not sent - placeholder): To={To}, Subject={Subject}",
            to, subject);

        var email = to.Contains('@') ? to : $"{to}@pif.gov.sa";

        await SendEmail(new EmailNotificationModel
        {
            to = new List<string> { email, _emailParams.InfraBaseModuleCC }
                                  .Where(email => !string.IsNullOrWhiteSpace(email)).ToList(),
            //cc = new List<string> { _emailParams.InfraBaseModuleCC },
            subject = subject,
            body = body,
            isHtml = true
        });
    }

    public async Task SendEmailToMultipleAsync(
        IEnumerable<string> recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual bulk email sending via middleware
        _logger.LogInformation(
            "Bulk email notification (not sent - placeholder): Recipients={Count}, Subject={Subject}",
            recipients.Count(), subject);

        await SendEmail(new EmailNotificationModel
        {
            to = (recipients ?? Enumerable.Empty<string>()).Append(_emailParams.InfraBaseModuleCC).ToList(),
            cc = new List<string> { _emailParams.InfraBaseModuleCC },
            subject = subject,
            body = body,
            isHtml = true
        });
    }

    public async Task SendPushNotificationAsync(
        string userId, 
        string title, 
        string message, 
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual push notification via middleware
        _logger.LogInformation(
            "Push notification (not sent - placeholder): UserId={UserId}, Title={Title}",
            userId, title);
        
        await Task.CompletedTask;
    }

    public async Task SendSmsAsync(
        string phoneNumber, 
        string message, 
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual SMS sending via middleware
        _logger.LogInformation(
            "SMS notification (not sent - placeholder): Phone={Phone}, Message={Message}",
            phoneNumber, message);
        
        await Task.CompletedTask;
    }

    public async Task CreateInAppNotificationAsync(
        string userId, 
        string title, 
        string message, 
        string? link = null,
        string? notificationType = "Info",
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual in-app notification via middleware
        _logger.LogInformation(
            "In-app notification (not sent - placeholder): UserId={UserId}, Title={Title}, Type={Type}, Link={Link}",
            userId, title, notificationType, link ?? "none");
        
        await Task.CompletedTask;
    }

    public async Task CreateBulkInAppNotificationAsync(
        IEnumerable<string> userIds,
        string title,
        string message,
        string? link = null,
        string? notificationType = "Info",
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual bulk in-app notification via middleware
        _logger.LogInformation(
            "Bulk in-app notification (not sent - placeholder): UserCount={Count}, Title={Title}, Type={Type}",
            userIds.Count(), title, notificationType);
        
        await Task.CompletedTask;
    }
}
