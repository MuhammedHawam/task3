using Microsoft.AspNetCore.Mvc;
using PartnersHub.Synergy.Apis.Common;
using PartnersHub.Synergy.Application.Opportunities.Commands;
using PartnersHub.Synergy.Application.SuccessStories.Commands;

namespace PartnersHub.Synergy.Apis.Models;

public sealed class CreateOpportunityFormRequest
{
    [ModelBinder(BinderType = typeof(FormJsonModelBinder), Name = "opportunity")]
    public CreateOpportunityCommand? Opportunity { get; init; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; init; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; init; }

    [FromForm(Name = "ContactId")]
    public Guid? ContactId { get; init; }
}


public sealed class UpdateOpportunityFormRequest
{
    [ModelBinder(BinderType = typeof(FormJsonModelBinder), Name = "opportunity")]
    public UpdateOpportunityCommand? Opportunity { get; init; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; init; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; init; }

    [FromForm(Name = "ContactId")]
    public Guid? ContactId { get; init; }
}
