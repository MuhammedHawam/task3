using MediatR;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Common;
using System.Text.Json.Serialization;



namespace PartnersHub.Synergy.Application.SuccessStories.Commands;

public class UpdateSuccessStoryCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int SuccessStoryTypeId { get; set; }
    public int SuccessStoryCollaborationStatusId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool? IsAdmin { get; set; }

    public List<Guid>? AttachmentIdsToRemove { get; init; }

    [JsonIgnore]
    public List<FileUploadContent>? FilesToUpload { get; set; }
    public string? AttachmentDescription { get; set; }

}

