using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;

public interface ICampaignRequestLinkedChallengeRepository
{
    Task AddListAsync(List<CampaignRequestLinkedChallenge> challengeList, CancellationToken cancellationToken);
}
