using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;


public class CampaignRequestSponsorRepository(InnovationHubDbContext dbContext) : ICampaignRequestSponsorRepository
{

    public async Task AddListAsync(List<CampaignRequestSponsor> sponsorList, CancellationToken cancellationToken)
    {
        await dbContext.campaignRequestSponsors.AddRangeAsync(sponsorList, cancellationToken);
    }
}
