using PartnersHub.InnovationHub.Domain.Aggregates;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;

public class ChallengeDetailsDTO
{
    public string Name { get; set; }
    public string SubmitterName { get;  set; }
    public string Description { get;  set; }
    public AssociatedProviderModel SourceCompany { get;  set; }
    public AssociatedSectorModel AssociatedSector { get;  set; }
    public PriorityLevel PriorityLevel { get;  set; }
    public DateTime DateAdded { get; set; }

    public ChallengeStatus ChallengeStatus { get;  set; }

    public bool? IsDraft { get;  set; }

    public List<AttachmentDto> Attachments { get; set; }

    public List<TechnologyDTO> Technologies { get; set; }

    public string Comment { get; set; }

}


public class AssociatedProviderModel
{
    public Guid Id { get;  set; }
    public string Name { get;  set; }
}


public class AssociatedSectorModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public string LogoUrl { get; set; }
}

public record TechnologyDTO(Guid TechnologyId,
                            String TechnologyName,
                            string JustificationForLinking,
                            RequestStatus RequestStatus,
                            DateTime SubmitterDate,
                            TechnologyStage Stage,
                            string Sector,
                            string Submitter);




public record AttachmentDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
    public string UploadedBy { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
}
