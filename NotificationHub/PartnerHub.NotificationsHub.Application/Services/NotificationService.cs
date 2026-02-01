using System.Text.Json;
using Microsoft.Extensions.Logging;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Application.Models;
using PartnerHub.NotificationsHub.Application.Services;
using PartnerHub.NotificationsHub.Domain.Entities;
using PartnerHub.NotificationsHub.Domain.Enums;

namespace PartnerHub.NotificationsHub.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly INotificationDispatcher _dispatcher;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        INotificationDispatcher dispatcher,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task<SendResult> SendEmailAsync(SendEmailRequest request, string sourceIp, string userAgent, string requestPath, string httpMethod, CancellationToken ct = default)
    {
        if (request.TemplateType == EmailTemplateType.Generic
            && string.IsNullOrWhiteSpace(request.Subject)
            && string.IsNullOrWhiteSpace(request.Body))
        {
            return new SendResult(false, null, "VALIDATION_ERROR", "Either a template or a subject/body must be provided.");
        }

        return await ProcessNotificationAsync(request, NotificationChannel.Email, sourceIp, userAgent, requestPath, httpMethod, ct);
    }

    public async Task<SendResult> SendSmsAsync(SendSmsRequest request, string sourceIp, string userAgent, string requestPath, string httpMethod, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new SendResult(false, null, "VALIDATION_ERROR", "Message is required.");
        }

        return await ProcessNotificationAsync(request, NotificationChannel.Sms, sourceIp, userAgent, requestPath, httpMethod, ct);
    }

    public async Task<SendResult> SendWebNotificationAsync(SendWebNotificationRequest request, string sourceIp, string userAgent, string requestPath, string httpMethod, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Message))
        {
            return new SendResult(false, null, "VALIDATION_ERROR", "UserId and Message are required.");
        }

        return await ProcessNotificationAsync(request, NotificationChannel.Web, sourceIp, userAgent, requestPath, httpMethod, ct);
    }

    private async Task<SendResult> ProcessNotificationAsync<T>(T request, NotificationChannel channel, string sourceIp, string userAgent, string requestPath, string httpMethod, CancellationToken ct)
    {
        var sourceService = GetSourceService(request);
        var correlationId = GetCorrelationId(request);

        var entity = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            Channel = channel,
            Status = NotificationStatus.Pending,
            AttemptCount = 0,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            NextAttemptAtUtc = DateTimeOffset.UtcNow,
            CorrelationId = correlationId,
            SourceService = sourceService ?? "Unknown",
            SourceIp = sourceIp,
            SourceUserAgent = userAgent,
            RequestPath = requestPath,
            HttpMethod = httpMethod,
            PayloadJson = JsonSerializer.Serialize(request)
        };

        _logger.LogInformation("Creating {Channel} notification {NotificationId} from {SourceService}",
            channel, entity.Id, entity.SourceService);

        await _repository.AddAsync(entity, ct);

        _logger.LogInformation("Dispatching {Channel} notification {NotificationId}", channel, entity.Id);
        var result = await _dispatcher.DispatchAsync(entity, ct);

        entity.Status = result.Success ? NotificationStatus.Sent : NotificationStatus.Failed;
        entity.AttemptCount = 1;
        entity.LastAttemptAtUtc = DateTimeOffset.UtcNow;
        entity.LastError = result.Success ? null : result.Message;
        entity.NextAttemptAtUtc = result.Success ? null : DateTimeOffset.UtcNow.AddMinutes(5);

        await _repository.UpdateAsync(entity, ct);

        if (result.Success)
        {
            _logger.LogInformation("{Channel} notification {NotificationId} sent successfully", channel, entity.Id);
        }
        else
        {
            _logger.LogError("{Channel} notification {NotificationId} failed: {Error}",
                channel, entity.Id, result.Message);
        }

        return result;
    }

    private static string? GetSourceService<T>(T request)
    {
        return request?.GetType().GetProperty("SourceService")?.GetValue(request) as string;
    }

    private static string? GetCorrelationId<T>(T request)
    {
        return request?.GetType().GetProperty("CorrelationId")?.GetValue(request) as string;
    }
}