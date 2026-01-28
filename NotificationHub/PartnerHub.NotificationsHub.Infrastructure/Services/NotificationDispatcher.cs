using System.Text.Json;
using Microsoft.Extensions.Logging;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Application.Models;
using PartnerHub.NotificationsHub.Domain.Entities;
using PartnerHub.NotificationsHub.Domain.Enums;

namespace PartnerHub.NotificationsHub.Infrastructure.Services;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly IWebNotificationSender _webNotificationSender;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        IEmailSender emailSender,
        ISmsSender smsSender,
        IWebNotificationSender webNotificationSender,
        ILogger<NotificationDispatcher> logger)
    {
        _emailSender = emailSender;
        _smsSender = smsSender;
        _webNotificationSender = webNotificationSender;
        _logger = logger;
    }

    public async Task<SendResult> DispatchAsync(NotificationEntity entity, CancellationToken ct = default)
    {
        return entity.Channel switch
        {
            NotificationChannel.Email => await DispatchEmailAsync(entity, ct),
            NotificationChannel.Sms => await DispatchSmsAsync(entity, ct),
            NotificationChannel.Web => await DispatchWebAsync(entity, ct),
            _ => new SendResult(false, entity.Id.ToString(), "UNKNOWN_CHANNEL", "Unknown notification channel")
        };
    }

    private async Task<SendResult> DispatchEmailAsync(NotificationEntity entity, CancellationToken ct)
    {
        var request = JsonSerializer.Deserialize<SendEmailRequest>(entity.PayloadJson!);
        
        var message = new EmailMessage(
            Id: entity.Id,
            TemplateType: request!.TemplateType,
            TemplateData: request.TemplateData,
            From: null,
            To: request.To.Select(t => new EmailAddress(t)).ToList(),
            Cc: request.Cc?.Select(c => new EmailAddress(c)).ToList() ?? new List<EmailAddress>(),
            Bcc: request.Bcc?.Select(b => new EmailAddress(b)).ToList() ?? new List<EmailAddress>(),
            Subject: request.Subject,
            Body: request.Body,
            IsHtml: request.IsHtml,
            Attachments: request.Attachments?.Select(a => new EmailAttachment(a.FileName, a.ContentType, a.Content)).ToList() ?? new List<EmailAttachment>());

        return await _emailSender.SendAsync(message, ct);
    }

    private async Task<SendResult> DispatchSmsAsync(NotificationEntity entity, CancellationToken ct)
    {
        var request = JsonSerializer.Deserialize<SendSmsRequest>(entity.PayloadJson!);
        return await _smsSender.SendAsync(request!.PhoneNumber, request.Message, ct);
    }

    private async Task<SendResult> DispatchWebAsync(NotificationEntity entity, CancellationToken ct)
    {
        var request = JsonSerializer.Deserialize<SendWebNotificationRequest>(entity.PayloadJson!);
        
        var message = new WebNotificationMessage(
            Id: entity.Id,
            UserId: request!.UserId,
            Title: request.Title,
            Message: request.Message,
            Url: request.Url,
            Severity: request.Severity,
            Payload: request.Payload);

        return await _webNotificationSender.SendAsync(message, ct);
    }
}

