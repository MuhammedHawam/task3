using Microsoft.AspNetCore.Mvc;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Application.Models;

namespace PartnerHub.NotificationsHub.Apis.Controllers;

[ApiController]
[Route("api/notifications/sms")]
public class SmsNotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public SmsNotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<ActionResult<SendResult>> SendSms([FromBody] SendSmsRequest request, CancellationToken ct)
    {
        var sourceIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var effectiveIp = string.IsNullOrWhiteSpace(forwardedFor) ? sourceIp : forwardedFor;
        var userAgent = Request.Headers["User-Agent"].ToString();

        var result = await _notificationService.SendSmsAsync(
            request, 
            effectiveIp ?? string.Empty, 
            userAgent, 
            HttpContext.Request.Path, 
            HttpContext.Request.Method, 
            ct);

        if (!result.Success && result.ErrorCode == "VALIDATION_ERROR")
        {
            return BadRequest(result.Message);
        }

        return result.Success ? Ok(result) : StatusCode(202, result);
    }
}