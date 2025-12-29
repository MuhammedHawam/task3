using Microsoft.Extensions.Logging;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Application.Models;

namespace PartnerHub.NotificationsHub.Infrastructure.Services;

public class ConsoleSmsSender : ISmsSender
{
    private readonly ILogger<ConsoleSmsSender> _logger;

    public ConsoleSmsSender(ILogger<ConsoleSmsSender> logger)
    {
        _logger = logger;
    }

    public Task<SendResult> SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending SMS to {PhoneNumber}: {Message}", phoneNumber, message);

        return Task.FromResult(new SendResult(
            Success: true,
            NotificationId: Guid.NewGuid().ToString(),
            ErrorCode: null,
            Message: "SMS sent successfully"));
    }
}