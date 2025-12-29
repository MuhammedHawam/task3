using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories
{
    public class CampaignRequestEvaluationCriteriaRepository(InnovationHubDbContext dbContext) : ICampaignRequestEvaluationCriteriaRepository
    {
        public async Task AddListAsync(List<CampaignRequestEvaluationCriteria> sponsorList, CancellationToken cancellationToken)
        {
            await dbContext.CampaignRequestEvaluationCriteria.AddRangeAsync(sponsorList, cancellationToken);
        }

    }
}
