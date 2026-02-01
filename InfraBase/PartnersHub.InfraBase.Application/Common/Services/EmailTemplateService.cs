using Microsoft.Extensions.Options;
using PartnersHub.InfraBase.Domain.Common;
using System.Text;

namespace PartnersHub.InfraBase.Application.Common.Services;

/// <summary>
/// Generates HTML email bodies for asset workflow notifications.
///
/// Implementation note:
/// We render from the shared PIF email template in `MailTemplate/emailTemplate.html`
/// (copied to output by the InfraBase API project) and inline the images as base64
/// to keep the email self-contained.
/// </summary>
public class EmailTemplateService
{
    private const string TemplateFolderName = "MailTemplate";
    private const string TemplateFileName = "emailTemplate.html";
    private readonly EmailParameters _emailParams;

    public EmailTemplateService(IOptions<EmailParameters> options)
    {
        _emailParams = options.Value;
    }

    private static readonly Lazy<string> _baseTemplate = new(LoadAndInlineTemplate, isThreadSafe: true);

    public string BuildAssetSubmittedEmail(string creatorName, Guid assetId)
    {
        var link = $"{_emailParams.BaseURL}/assets/{assetId}";
        return Render(
            messageEn: $"New asset submitted by \"{creatorName}\" and waiting your approval.",
            messageAr: "تم إرسال أصل جديد وبانتظار موافقتك.",
            linkUrl: link);
    }

    public string BuildAssetAcceptedByPcAdminEmail(Guid assetId)
    {
        var link = $"{_emailParams.BaseURL}/assets/{assetId}";
        return Render(
            messageEn: "Your asset has been approved.",
            messageAr: "تمت الموافقة على الأصل.",
            linkUrl: link);
    }

    public string BuildAssetRejectedByPcAdminEmail(Guid assetId)
    {
        var link = $"{_emailParams.BaseURL}/assets/{assetId}";
        return Render(
            messageEn: "Your asset has been rejected.",
            messageAr: "تم رفض الأصل.",
            linkUrl: link);
    }

    public string BuildNewRequestSubmittedEmail(string companyName, Guid assetId)
    {
        var link = $"{_emailParams.BaseURL}/assets/{assetId}";
        return Render(
            messageEn: $"New request submitted by \"{companyName}\" and waiting your approval.",
            messageAr: $"تم إرسال طلب جديد من \"{companyName}\" وبانتظار موافقتك.",
            linkUrl: link);
    }

    public string BuildAssetAcceptedByInfrabaseAdminEmail(Guid assetId)
    {
        var link = $"{_emailParams.BaseURL}/assets/{assetId}";
        return Render(
            messageEn: "Your asset has been approved.",
            messageAr: "تمت الموافقة على الأصل.",
            linkUrl: link);
    }

    public string BuildAssetRejectedByInfrabaseAdminEmail(Guid assetId)
    {
        var link = $"{_emailParams.BaseURL}/assets/{assetId}";
        return Render(
            messageEn: "Your asset has been rejected and returned for correction.",
            messageAr: "تم إرجاع الأصل للتعديل.",
            linkUrl: link);
    }

    private static string Render(string messageEn, string messageAr, string linkUrl)
    {
        // NOTE: The source template is a static HTML file with sample data.
        // We replace the key text blocks (EN/AR) and set the link target.
        var html = _baseTemplate.Value;

        // English section replacements
        html = html.Replace("Dear Bashayr Alghamdi - NEOM,", "Dear,", StringComparison.Ordinal);
        html = html.Replace("You have been assigned a new Task.", messageEn, StringComparison.Ordinal);
        html = html.Replace(
            "Please click on this\u00A0(\u00A0Link\u00A0)\u00A0to access the request.",
            BuildLinkLineEn(linkUrl),
            StringComparison.Ordinal);

        // Arabic section replacements
        html = html.Replace("السلام عليكم Bashayr Alghamdi - NEOM،", "السلام عليكم،", StringComparison.Ordinal);
        html = html.Replace("تم تكليفك بمهمة جديدة.", messageAr, StringComparison.Ordinal);

        // Replace ONLY the request-link target (the template contains other "#" links e.g. support).
        html = ReplaceFirst(html, "href=\"#\"", $"href=\"{linkUrl}\"", StringComparison.Ordinal);

        return html;
    }

    private static string BuildLinkLineEn(string linkUrl)
    {
        // Keep the same styling as template (dark green + underline).
        var sb = new StringBuilder();
        sb.Append("Please click on this ");
        sb.Append($"<a href=\"{linkUrl}\" style=\"color: #00342b; text-decoration: underline; font-weight: 600;\">( Link )</a>");
        sb.Append(" to access the request.");
        return sb.ToString();
    }

    private static string ReplaceFirst(string input, string oldValue, string newValue, StringComparison comparison)
    {
        var idx = input.IndexOf(oldValue, comparison);
        if (idx < 0)
        {
            return input;
        }

        return string.Concat(input.AsSpan(0, idx), newValue, input.AsSpan(idx + oldValue.Length));
    }

    private static string LoadAndInlineTemplate()
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, TemplateFolderName, TemplateFileName);
        var html = File.ReadAllText(templatePath);

        // Inline images (email clients cannot resolve relative file paths).
        html = InlineImage(html, "BG.png");
        html = InlineImage(html, "PIF_Logo.png");
        html = InlineImage(html, "PIF.png");
        html = InlineImage(html, "Platform_Logo.png");

        return html;
    }

    private static string InlineImage(string html, string fileName)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, TemplateFolderName, fileName);
        if (!File.Exists(filePath))
        {
            return html;
        }

        var base64 = Convert.ToBase64String(File.ReadAllBytes(filePath));
        var dataUri = $"data:image/png;base64,{base64}";

        // The template references images as "./<fileName>" (both in img src and css url()).
        return html.Replace($"./{fileName}", dataUri, StringComparison.Ordinal);
    }
}
