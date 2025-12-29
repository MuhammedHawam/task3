using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;

public class ChallengeCompanyDTO
{
    public Guid Id { get; init; }
    public string Name { get; init; } 
    public string Description { get; init; } 
    public string DevCoName { get; init; } 
    public string SectorName { get; init; } 
    public PriorityLevel PriorityLevel { get; init; }
    public ChallengeStatus ChallengeStatus { get; set; }
    public DateTime CreatedAt { get; init; }
    public Guid SourceCompanyId { get; init; }  
}

public class CampaignCompanyDTO
{
    public Guid CampaignId { get; init; }
    public string CampaignName { get; init; }
    public string Description { get; init; }
    public DateTime? SubmissionDeadline { get; init; }
    public CampaignStatus CampaignStatus { get; init; }
    public CampaignType CampaignType { get; init; }
    public DateTime? LunchDate { get; init; }
    public List<Guid> SourceCompanyIds { get; init; }
}
