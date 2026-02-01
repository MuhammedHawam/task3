using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PartnersHub.InfraBase.Apis.Models;

public sealed class AssetMultipartRequest
{
    [FromForm(Name = "asset")]
    public string Asset { get; set; } = string.Empty;

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; set; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; set; }
}
