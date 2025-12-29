using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs
{
    public class CampaignDetailsDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ProblemStatement { get; set; }
        public CampaignRequestStatus CampaignRequestStatus { get; set; }
        public CampaignType Type { get; set; }
        public DateTime? LaunchDate { get; set; }
        public TimeSpan? RemainingTime { get; set; }
        public DateTime? SubmissionDeadLine { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string Comments { get; set; }
        public List<CampaignLinkedChallengesDTO> LinkedChallenges { get; set; }
        public List<CampaignSponsersDTO> CampaignSponsers { get; set; }
        public List<CampaignEvaluationCriteriaDTO> CampaignEvaluationCriteria { get; set; }

        public List<AttachmentDto> TermsAndConditions { get; set; } 

        public SubmitterDTO SubmitterData { get; set; }

        public List<CampaignEvaluatorDto> EvaluatorList { get; set; }
    }
    public class CampaignLinkedChallengesDTO
    {
        public string ChallengeName { get; set; }
        public Guid ChallengeId { get; set; }
        public string Description { get; init; }
        public string DevCoName { get; init; }
        public string DevCoLogoUrl { get; init; }
        public string SectorName { get; init; }
        public PriorityLevel PriorityLevel { get; init; }
        public DateTime CreatedAt { get; init; }
        public string SubmitterName { get; init; }  
    }
    public class CampaignSponsersDTO
    {
        public Guid SponsorId { get; set; }
        public string SponserName { get; set; }
    }
    public class CampaignEvaluationCriteriaDTO
    {
        public string CriteriaName { get; set; }
        public int CriteriaValue { get; set; }
    }

    public class CampaignEvaluatorDto
    {
        public Guid EvaluatorId { get; set; }
        public string EvaluatorName { get; set; }
    }

    public class SubmitterDTO(Guid SubmitterId,string SubmitterName,string SubmitterCompany, string RepresentativeName,string Description , String LogoUrl);


}
