using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories
{
    public class CampaignRequestEvaluatorRepository(InnovationHubDbContext dbContext) : ICampaignRequestEvaluatorRepository
    {

        public async Task AddListAsync(List<CampaignRequestEvaluator> sponsorList, CancellationToken cancellationToken)
        {
            await dbContext.CampaignRequestEvaluator.AddRangeAsync(sponsorList, cancellationToken);
        }
    }
}
