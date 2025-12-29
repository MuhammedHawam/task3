using PartnerHub.NotificationsHub.Application.Models;

namespace PartnerHub.NotificationsHub.Application.Interfaces;

public interface INotificationService
{
    Task<SendResult> SendEmailAsync(SendEmailRequest request, string sourceIp, string userAgent, string requestPath, string httpMethod, CancellationToken ct = default);
    Task<SendResult> SendSmsAsync(SendSmsRequest request, string sourceIp, string userAgent, string requestPath, string httpMethod, CancellationToken ct = default);
    Task<SendResult> SendWebNotificationAsync(SendWebNotificationRequest request, string sourceIp, string userAgent, string requestPath, string httpMethod, CancellationToken ct = default);
}