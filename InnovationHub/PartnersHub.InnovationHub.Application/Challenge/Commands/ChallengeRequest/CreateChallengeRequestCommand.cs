using MediatR;
using PartnersHub.InnovationHub.Application.Models;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System.Text.Json.Serialization;


namespace PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest
{
    public record CreateChallengeRequestCommand : IRequest<Results<Guid>>
    {
        public Guid UserId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public SourceCompanyDto SourceCompany { get; set; }
        public AssociatedSectorDto AssociatedSector { get; set; }
        public string SubmitterName { get; init; } = string.Empty;
        public string SubmitterEmail { get; init; } = string.Empty;
        public PriorityLevel? PriorityLevel { get; init; }
        public bool? IsDraft { get; init; }

        [JsonIgnore]
        public List<FileUploadContent>? FilesToUpload { get; set; }
        public string? AttachmentDescription { get; set; }
        //public List<ChallengeRequestAttachment> Attachments { get; init; } = new List<ChallengeRequestAttachment>();
        //public List<Technology> Technologies { get; init; } = new List<Technology>();
    }

    public record SourceCompanyDto(Guid id, string name);
    public record AssociatedSectorDto(Guid id, string name);
}
