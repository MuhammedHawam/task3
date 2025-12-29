using Microsoft.Extensions.Hosting;
using PartnerHub.NotificationsHub.Application.Interfaces;
using PartnerHub.NotificationsHub.Domain.Enums;

namespace PartnerHub.NotificationsHub.Infrastructure.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IHostEnvironment _environment;
    private readonly string _templatesPath;

    public EmailTemplateService(IHostEnvironment environment)
    {
        _environment = environment;
        _templatesPath = Path.Combine(_environment.ContentRootPath, "Templates", "Email");
    }

    public async Task<string> RenderTemplateAsync(EmailTemplateType templateType, IReadOnlyDictionary<string, string> templateData, CancellationToken ct = default)
    {
        var masterTemplate = await LoadMasterTemplateAsync(ct);
        var contentTemplate = await LoadContentTemplateAsync(templateType, ct);

        // Render content template with data
        var renderedContent = RenderTemplate(contentTemplate, templateData);

        // Merge with master template
        var masterData = new Dictionary<string, string>(templateData)
        {
            ["Content"] = renderedContent,
            ["Title"] = GetTemplateTitle(templateType),
            ["CompanyName"] = templateData.GetValueOrDefault("CompanyName", "Partner Hub"),
            ["Year"] = DateTime.UtcNow.Year.ToString(),
            ["CompanyAddress"] = templateData.GetValueOrDefault("CompanyAddress", "123 Business St, City, Country")
        };

        return RenderTemplate(masterTemplate, masterData);
    }

    private async Task<string> LoadMasterTemplateAsync(CancellationToken ct)
    {
        var masterPath = Path.Combine(_templatesPath, "Master", "master.html");
        return await File.ReadAllTextAsync(masterPath, ct);
    }

    private async Task<string> LoadContentTemplateAsync(EmailTemplateType templateType, CancellationToken ct)
    {
        var templateFolder = templateType switch
        {
            EmailTemplateType.OrderCreated => "OrderCreated",
            EmailTemplateType.OrderShipped => "OrderShipped",
            EmailTemplateType.PasswordReset => "PasswordReset",
            _ => throw new ArgumentException($"Unknown template type: {templateType}")
        };

        var templatePath = Path.Combine(_templatesPath, templateFolder, "template.html");
        return await File.ReadAllTextAsync(templatePath, ct);
    }

    private static string RenderTemplate(string template, IReadOnlyDictionary<string, string> data)
    {
        var result = template;
        foreach (var kvp in data)
        {
            result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value ?? string.Empty);
        }
        return result;
    }

    private static string GetTemplateTitle(EmailTemplateType templateType)
    {
        return templateType switch
        {
            EmailTemplateType.OrderCreated => "Order Confirmation",
            EmailTemplateType.OrderShipped => "Order Shipped",
            EmailTemplateType.PasswordReset => "Password Reset",
            _ => "Notification"
        };
    }
}