using MediatR;
using PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Application.Campaign.Commands;

public record CreateCampaignCommand : IRequest<Results<Guid>>
{
    public string CampaignName { get; init; }
    public string Description { get; init; }
    public string ProblemStatement { get; init; }
    public CampaignType Type { get; init; }
    public Guid SubmitterId { get; init; }
    public string SubmitterName { get; init; }
    public string SubmitterEmail { get; init; } = "na@pif.gov.sa";
    public List<Guid>? LinkedDevCoChallenges { get; init; }
    public DateTime LaunchDate { get; init; }
    public DateTime SubmissionDeadlineDate { get; init; }
    public List<SponsorDto> SponsorsList { get; init; }
    public List<EvaluatorDto> EvaluatorList { get; init; }
    public List<EvaluationCriteria> EvaluationCriteriaList { get; init; }
    public List<TermsDto> TermsAndConditions { get; init; }
    public bool? IsDraft { get; init; }


}

public record EvaluatorDto(Guid id, string name);
public record EvaluationCriteria(string name, int value);
public record TermsDto(string fileName, long fileSizeInBytes, string contentType, string sharePointFileId, string SharePointUrl, string sharePointLibrary, AttachementMetadataDto attachmentMetadata);
