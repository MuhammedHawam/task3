using MediatR;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Application.Campaign.Commands;

public class ConvertRequestToCampaignDraftCommand : IRequest<CampaignRequestStatus>
{
    public Guid CampaignRequestId { get; set; }
    public Guid UserId { get; init; }
    public string CampaignName { get; init; }
    public string CampaignDescription { get; init; }
    public CampaignType type { get; init; }
    public List<Guid>? LinkedChallenges { get; init; }
    public DateTime? LaunchDate { get; init; }
    public List<CriteriaWeight> CriteriaWeight { get; set; }
    public List<sponsers> Sponsers { get; set; }
    public List<Guid> Evaluators { get; set; }
    public CampaignRequestStatus status { get; set; }
    public List<TermsAndConditionsDTO> Attachments { get; init; } = new List<TermsAndConditionsDTO>();
}

public record TermsAndConditionsDTO(string fileName, long fileSizeInBytes, string contentType, string sharePointFileId, string SharePointUrl, string sharePointLibrary, TermsAndConditionsMetadataDto attachmentMetadata);
public record TermsAndConditionsMetadataDto(string Name, Format Format, Extension Extension, long SizeInBytes, string Url);
public record CriteriaWeight(string CriteriaName, int CriteriaValue);
public record sponsers(Guid SponsorId, string SponserName);
