using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Application.Models;

namespace PartnerHub.NotificationsHub.Infrastructure.Services;

public class SignalRWebNotificationSender : IWebNotificationSender
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRWebNotificationSender> _logger;

    public SignalRWebNotificationSender(IHubContext<NotificationHub> hubContext, ILogger<SignalRWebNotificationSender> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<SendResult> SendAsync(WebNotificationMessage message, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.User(message.UserId).SendAsync("ReceiveNotification", new
            {
                message.Id,
                message.Title,
                message.Message,
                message.Url,
                Severity = message.Severity.ToString(),
                Payload = message.Payload,
                Timestamp = DateTimeOffset.UtcNow
            }, ct);

            _logger.LogInformation("Sent web notification {NotificationId} to user {UserId}", message.Id, message.UserId);

            return new SendResult(
                Success: true,
                NotificationId: message.Id.ToString(),
                ErrorCode: null,
                Message: "Web notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send web notification {NotificationId} to user {UserId}", message.Id, message.UserId);

            return new SendResult(
                Success: false,
                NotificationId: message.Id.ToString(),
                ErrorCode: "SIGNALR_ERROR",
                Message: ex.Message);
        }
    }
}

public class NotificationHub : Hub
{
}