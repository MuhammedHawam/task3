using PartnerHub.NotificationsHub.Domain.Enums;

namespace PartnerHub.NotificationsHub.Application.Interfaces;

public interface IEmailTemplateService
{
    Task<string> RenderTemplateAsync(EmailTemplateType templateType, IReadOnlyDictionary<string, string> templateData, CancellationToken ct = default);
}