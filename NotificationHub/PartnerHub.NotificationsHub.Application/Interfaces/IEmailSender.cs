using PartnerHub.NotificationsHub.Application.Models;
using PartnerHub.NotificationsHub.Domain.Enums;
using PartnerHub.NotificationsHub.Domain.Entities;

namespace PartnerHub.NotificationsHub.Application.Interfaces;

public interface IEmailSender
{
    Task<SendResult> SendAsync(EmailMessage message, CancellationToken ct = default);
}

public interface ISmsSender
{
    Task<SendResult> SendAsync(string phoneNumber, string message, CancellationToken ct = default);
}

public interface IWebNotificationSender
{
    Task<SendResult> SendAsync(WebNotificationMessage message, CancellationToken ct = default);
}

public interface INotificationDispatcher
{
    Task<SendResult> DispatchAsync(NotificationEntity entity, CancellationToken ct = default);
}

public sealed record EmailAddress(string Value);

public sealed record EmailAttachment(
    string FileName,
    string ContentType,
    byte[] Content);

public sealed record EmailMessage(
    Guid Id,
    EmailTemplateType TemplateType,
    IReadOnlyDictionary<string, string>? TemplateData,
    EmailAddress? From,
    IReadOnlyCollection<EmailAddress> To,
    IReadOnlyCollection<EmailAddress> Cc,
    IReadOnlyCollection<EmailAddress> Bcc,
    string? Subject,
    string? Body,
    bool IsHtml,
    IReadOnlyCollection<EmailAttachment> Attachments);

public sealed record WebNotificationMessage(
    Guid Id,
    string UserId,
    string Title,
    string Message,
    string? Url,
    WebNotificationSeverity Severity,
    object? Payload);