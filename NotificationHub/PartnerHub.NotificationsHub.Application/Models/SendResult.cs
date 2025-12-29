namespace PartnerHub.NotificationsHub.Application.Models;

public sealed record SendResult(
    bool Success,
    string NotificationId,
    string? ErrorCode,
    string? Message);