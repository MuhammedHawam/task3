using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;

public class CampaignRequestLinkedChallengeRepository(InnovationHubDbContext dbContext) : ICampaignRequestLinkedChallengeRepository
{

    public async Task AddListAsync(List<CampaignRequestLinkedChallenge> challengeList, CancellationToken cancellationToken)
    {
        await dbContext.campaignRequestLinkedChallenges.AddRangeAsync(challengeList, cancellationToken);
    }
}
