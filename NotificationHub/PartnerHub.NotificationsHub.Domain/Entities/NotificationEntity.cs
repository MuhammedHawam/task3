using PartnerHub.NotificationsHub.Domain.Enums;

namespace PartnerHub.NotificationsHub.Domain.Entities;

public class NotificationEntity
{
    public Guid Id { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastAttemptAtUtc { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string? SourceService { get; set; }
    public string? SourceIp { get; set; }
    public string? SourceUserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? HttpMethod { get; set; }
    public string? PayloadJson { get; set; }
    public string? LastError { get; set; }
}