using Microsoft.AspNetCore.Mvc;
using PartnersHub.InnovationHub.Apis.Common;
using PartnersHub.InnovationHub.Application.Campaign.Commands;

namespace PartnersHub.InnovationHub.Apis.Models;

public sealed class CreateCampaignReqFormRequest
{
    [ModelBinder(BinderType = typeof(FormJsonModelBinder), Name = "campaign")]
    public CreateCampaignRequestCommand? Campaign { get; init; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; init; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; init; }

    [FromForm(Name = "ContactId")]
    public Guid? ContactId { get; init; }
}

public sealed class CreateCampaignFormRequest
{
    [ModelBinder(BinderType = typeof(FormJsonModelBinder), Name = "campaign")]
    public CreateCampaignCommand? Campaign { get; init; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; init; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; init; }

    [FromForm(Name = "ContactId")]
    public Guid? ContactId { get; init; }
}



