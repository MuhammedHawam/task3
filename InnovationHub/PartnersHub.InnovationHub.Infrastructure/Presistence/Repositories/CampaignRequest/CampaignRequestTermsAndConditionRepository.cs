using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;


public class CampaignRequestTermsAndConditionRepository(InnovationHubDbContext dbContext) : ICampaignRequestTermsAndConditionRepository
{

    public async Task AddListAsync(List<CampaignRequestTermsAndCondition> termsList, CancellationToken cancellationToken)
    {
        await dbContext.campaignRequestTermsAndConditions.AddRangeAsync(termsList, cancellationToken);
    }
}
