using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;

public class ActiveCampaignCardDTO
{
    public Guid CampaignId { get; init; }
    public string CampaignName { get; init; }   
    public string Description { get; init; }
    public DateTime? SubmissionDeadline { get; init; }
    public CampaignStatus CampaignStatus { get; init; }
    public CampaignRequestStatus Status { get; init; }
    public CampaignType CampaignType { get; init; }
    public DateTime? LunchDate { get; init; }    
    public string Submitter {  get; init; }
    public string ShortId { get; init; }


}
