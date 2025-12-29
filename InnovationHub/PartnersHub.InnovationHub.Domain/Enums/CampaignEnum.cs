using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Domain.Enums
{
    public enum CampaignRequestStatus
    {
        Requested,
        Draft,
        PendingReview,
        Submitted,
        Published,
        Closed,
        Archived
    }


    public enum RequestState
    {
        PendingReview,
        Approved,
        Rejected,
        Returned
    }
    public enum CampaignStatus
    {
        Open,
        Closed,
        Upcoming
    }

    public enum CampaignType
    {
        Public,
        Internal
    }
}
