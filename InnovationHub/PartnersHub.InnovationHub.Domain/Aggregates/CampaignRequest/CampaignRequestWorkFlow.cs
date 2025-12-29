using PartnersHub.InnovationHub.Domain.Aggregates.CampaignRequest;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using PartnersHub.InnovationHub.Domain.Events.Campaigns;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Domain.Aggregates.Campaigns
{
    public class CampaignRequestWorkFlow : CampaignRequest
    {
        public CampaignRequestWorkFlow() { }
        /// <summary>
        /// Submitted => Published
        /// </summary>
        /// <param name="nowUtc"></param>
        public new void Publish(DateTime nowUtc)
        {
            if (CampaignRequestStatus != CampaignRequestStatus.Submitted)
                throw new InvalidOperationException("Only Submitted Campaings can be published.");

            if (SubmissionDeadLine.HasValue && nowUtc > SubmissionDeadLine.Value)
                throw new InvalidOperationException("Cannot publish after the submission deadline.");

            CampaignRequestStatus = CampaignRequestStatus.Published;
            AddDomainEvent(new CampaignRequestPublishedEvent(Id));
        }

        /// <summary>
        /// Published => Closed
        /// </summary>
        public new void Close()
        {
            if (CampaignRequestStatus != CampaignRequestStatus.Published)
                throw new InvalidOperationException("Only Published Campaings can be closed.");

            CampaignRequestStatus = CampaignRequestStatus.Closed;
        }


        /// <summary>
        /// Soft Final State
        /// </summary>
        public new void Archive()
        {
            if (CampaignRequestStatus == CampaignRequestStatus.Archived) return;
            CampaignRequestStatus = CampaignRequestStatus.Archived;
        }
    }
}
