namespace PartnerHub.NotificationsHub.Domain.Enums;

public enum NotificationChannel
{
    Email = 1,
    Sms = 2,
    Web = 3
}

public enum EmailTemplateType
{
    Generic = 0,
    OrderCreated = 1,
    OrderShipped = 2,
    PasswordReset = 3
}

public enum WebNotificationSeverity
{
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3
}

public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    DeadLetter = 3
}