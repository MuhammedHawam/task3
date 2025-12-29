using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartnersHub.InnovationHub.Domain.Common;

namespace PartnersHub.InnovationHub.Domain.Events.Campaigns
{
    public class CampaignRequestPublishedEvent : DomainEvent
    {
        public Guid CampaignId { get; }

        public CampaignRequestPublishedEvent(Guid campaignId)
        {
            CampaignId = campaignId;
        }
    }
}
