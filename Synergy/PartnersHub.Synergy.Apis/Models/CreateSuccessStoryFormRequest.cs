using Microsoft.AspNetCore.Mvc;
using PartnersHub.Synergy.Apis.Common;
using PartnersHub.Synergy.Application.SuccessStories.Commands;

namespace PartnersHub.Synergy.Apis.Models;

public sealed class CreateSuccessStoryFormRequest
{
    [ModelBinder(BinderType = typeof(FormJsonModelBinder), Name = "successStory")]
    public CreateSuccessStoryCommand? SuccessStory { get; init; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; init; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; init; }

    [FromForm(Name = "ContactId")]
    public Guid? ContactId { get; init; }
}


public sealed class UpdateSuccessStoryFormRequest
{
    [ModelBinder(BinderType = typeof(FormJsonModelBinder), Name = "successStory")]
    public UpdateSuccessStoryCommand? SuccessStory { get; init; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; init; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; init; }

    [FromForm(Name = "ContactId")]
    public Guid? ContactId { get; init; }
}
