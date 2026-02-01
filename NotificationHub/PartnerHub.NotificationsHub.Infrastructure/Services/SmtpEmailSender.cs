using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Application.Models;
using PartnerHub.NotificationsHub.Domain.Enums;

namespace PartnerHub.NotificationsHub.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _smtpOptions;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> smtpOptions, IEmailTemplateService templateService, ILogger<SmtpEmailSender> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<SendResult> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Preparing to send email {NotificationId} to {Recipients}",
                message.Id, string.Join(", ", message.To.Select(t => t.Value)));

            // Render template if not generic
            var body = message.Body;
            var subject = message.Subject;
            
            if (message.TemplateType != EmailTemplateType.Generic && message.TemplateData != null)
            {
                _logger.LogInformation("Rendering template {TemplateType} for email {NotificationId}",
                    message.TemplateType, message.Id);

                body = await _templateService.RenderTemplateAsync(message.TemplateType, message.TemplateData, ct);
                subject = message.TemplateData.GetValueOrDefault("Subject", subject);
            }

            _logger.LogInformation("Connecting to SMTP server {Host}:{Port}", _smtpOptions.Host, _smtpOptions.Port);

            using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
            {
                EnableSsl = _smtpOptions.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = _smtpOptions.UseAuthentication 
                    ? new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password)
                    : null,
                Timeout = _smtpOptions.TimeoutSeconds * 1000
            };
            
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpOptions.Username),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            foreach (var recipient in message.To)
            {
                mailMessage.To.Add(recipient.Value);
            }

            foreach (var cc in message.Cc)
            {
                mailMessage.CC.Add(cc.Value);
            }

            foreach (var bcc in message.Bcc)
            {
                mailMessage.Bcc.Add(bcc.Value);
            }

            await client.SendMailAsync(mailMessage, ct);

            _logger.LogInformation("Email {NotificationId} sent successfully via SMTP", message.Id);

            return new SendResult(
                Success: true,
                NotificationId: message.Id.ToString(),
                ErrorCode: null,
                Message: "Email sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email {NotificationId}: {Error}", message.Id, ex.Message);

            return new SendResult(
                Success: false,
                NotificationId: message.Id.ToString(),
                ErrorCode: "SMTP_ERROR",
                Message: ex.Message);
        }
    }
}

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public bool UseAuthentication { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableDebug { get; set; } = false;
}