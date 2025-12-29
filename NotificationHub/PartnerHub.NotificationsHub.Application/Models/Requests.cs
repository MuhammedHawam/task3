using PartnerHub.NotificationsHub.Domain.Enums;

namespace PartnerHub.NotificationsHub.Application.Models;

public record SendEmailRequest(
    IReadOnlyCollection<string> To,
    IReadOnlyCollection<string>? Cc,
    IReadOnlyCollection<string>? Bcc,
    EmailTemplateType TemplateType,
    IReadOnlyDictionary<string, string>? TemplateData,
    string? Subject,
    string? Body,
    bool IsHtml,
    IReadOnlyCollection<EmailAttachmentRequest>? Attachments,
    string? CorrelationId,
    string? SourceService);

public record EmailAttachmentRequest(
    string FileName,
    string ContentType,
    byte[] Content);

public record SendSmsRequest(
    string PhoneNumber,
    string Message,
    string? CorrelationId,
    string? SourceService);

public record SendWebNotificationRequest(
    string UserId,
    string Title,
    string Message,
    string? Url,
    WebNotificationSeverity Severity,
    object? Payload,
    string? CorrelationId,
    string? SourceService);