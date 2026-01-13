using MediatR;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Campaign.Commands;

public record CreateCampaignRequestCommand : IRequest<Results<Guid>>
{
    public string CampaignName { get; init; }
    public string Description { get; init; }
    public string ProblemStatement { get; init; }  
    public CampaignType Type { get; init; }
    public Guid SubmitterId { get; init; }
    public string SubmitterName { get; init; }
    public string SubmitterEmail { get; init; }= "na@pif.gov.sa";
    public List<Guid>? LinkedDevCoChallenges { get; init; }
    public DateTime? LaunchDate { get; init; }
    public string? Comment { get; init; } = string.Empty;
    public List<SponsorDto> SponsorsList { get; init; }

}

public record SponsorDto(Guid id, string name);
