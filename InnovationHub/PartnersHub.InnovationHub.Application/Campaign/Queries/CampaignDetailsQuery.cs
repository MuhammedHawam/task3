using MediatR;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;


namespace PartnersHub.InnovationHub.Application.Campaign.Queries;

public class CampaignDetailsQuery : IRequest<CampaignDetailsDTO>
{
    public Guid CampaignId { get; set; }
}
