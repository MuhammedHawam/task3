using Microsoft.AspNetCore.Mvc;
using PartnersHub.InnovationHub.Apis.Common;
using PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest;

namespace PartnersHub.InnovationHub.Apis.Models;

public sealed class CreateChallengeFormRequest
{
    [ModelBinder(BinderType = typeof(FormJsonModelBinder), Name = "challenge")]
    public CreateChallengeRequestCommand? Challenge { get; init; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; init; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; init; }

    [FromForm(Name = "ContactId")]
    public Guid? ContactId { get; init; }
}


public sealed class UpdateChallengeFormRequest
{
    [ModelBinder(BinderType = typeof(FormJsonModelBinder), Name = "challenge")]
    public EditChallengeRequestCommand? Challenge { get; init; }

    [FromForm(Name = "files")]
    public List<IFormFile> Files { get; init; } = new();

    [FromForm(Name = "attachmentDescription")]
    public string? AttachmentDescription { get; init; }

    [FromForm(Name = "ContactId")]
    public Guid? ContactId { get; init; }
}