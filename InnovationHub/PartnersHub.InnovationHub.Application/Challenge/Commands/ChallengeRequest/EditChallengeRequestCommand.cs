using MediatR;
using PartnersHub.InnovationHub.Application.Models;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System.Text.Json.Serialization;



namespace PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest
{
    public record EditChallengeRequestCommand : IRequest<Result<bool>>
    {
        public Guid UserId { get; init; }
        public Guid ChallengeRequestId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public SourceCompanyDto SourceCompany { get; set; }
        public AssociatedSectorDto AssociatedSector { get; set; }
        public string SubmitterName { get; init; } = string.Empty;
        public PriorityLevel PriorityLevel { get; init; }
        public bool IsDraft { get; init; }
        public List<AttachementDto> Attachments { get; init; } = new List<AttachementDto>();

        public List<Guid>? AttachmentIdsToRemove { get; init; }

        [JsonIgnore]
        public List<FileUploadContent>? FilesToUpload { get; set; }
        public string? AttachmentDescription { get; set; }
    }


        public record AttachementDto(string fileName, long fileSizeInBytes,string contentType ,string sharePointFileId ,string SharePointUrl , string sharePointLibrary, AttachementMetadataDto attachmentMetadata);
        public record AttachementMetadataDto(string Name, Format Format, Extension Extension, long SizeInBytes, string Url);

    
}
