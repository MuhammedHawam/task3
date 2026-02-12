using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.InfraBase.Apis.Common;
using PartnersHub.InfraBase.Application.Assets.Commands;

namespace PartnersHub.InfraBase.Apis.Models;

public sealed class CreateAssetFormRequest
{
    [ModelBinder(BinderType = typeof(FormJsonModelBinder), Name = "asset")]
    public CreateAssetCommand? Asset { get; init; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; init; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; init; }

    [FromForm(Name = "ContactId")]
    public Guid? ContactId { get; init; }
}

public sealed class UpdateAssetFormRequest
{
    [ModelBinder(BinderType = typeof(FormJsonModelBinder), Name = "asset")]
    public UpdateAssetCommand? Asset { get; init; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; init; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; init; }

    [FromForm(Name = "ContactId")]
    public Guid? ContactId { get; init; }
}