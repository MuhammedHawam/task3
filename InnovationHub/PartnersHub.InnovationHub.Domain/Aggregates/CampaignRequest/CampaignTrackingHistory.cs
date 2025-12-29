using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Domain.Aggregates.CampaignRequest
{
     
    public class CampaignTrackingHistory : Entity
    {
        public Guid CampaignRequestId { get; private set; }
        public CampaignRequestStatus Status { get; private set; }
        public string Action { get; private set; }
        public Guid PerformedBy { get; private set; }
        public DateTime PerformedAt { get; private set; }
        public string? Comments { get; private set; }
        public string? FieldsChanged { get; private set; }
        public string? OldValues { get; private set; }
        public string? NewValues { get; private set; }

        private CampaignTrackingHistory() { }

        public CampaignTrackingHistory(CampaignRequestStatus status,
                                       string action,
                                       Guid performedBy,
                                       DateTime performedAt,
                                       string? comments,
                                       string? fieldsChanged,
                                       string? oldValues,
                                       string? newValues)
        {
            PerformedAt = DateTime.UtcNow;
            Action = action;
            Status = status;
            PerformedBy = performedBy;
            PerformedAt = performedAt;
            Comments = comments;    
            FieldsChanged = fieldsChanged;
            OldValues = oldValues;
            NewValues = newValues;
        }
    }

}
