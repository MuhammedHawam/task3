using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartnersHub.InnovationHub.Domain.Common;

namespace PartnersHub.InnovationHub.Domain.Events.Campaigns
{
    public class CampaignRequestPendingApprovalEvent : DomainEvent
    {
        public Guid CampaignId { get; }
        public Guid OwnerId { get; }

        public CampaignRequestPendingApprovalEvent(Guid campaignId, Guid ownerId)
        {
            CampaignId = campaignId;
            OwnerId = ownerId;
        }
    }
}
