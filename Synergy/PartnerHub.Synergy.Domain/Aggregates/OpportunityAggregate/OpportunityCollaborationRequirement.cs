using PartnersHub.Synergy.Domain.Aggregates.Synergy.Lookups;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate
{
    public class OpportunityCollaborationRequirement : Entity
    {
        public Guid OpportunityId { get; set; }
        public int CollaborationRequirementId { get; set; }

        private OpportunityCollaborationRequirement() { }
        internal OpportunityCollaborationRequirement(Guid opportunityId ,  int collaborationRequirementId)
        {
            if (opportunityId == Guid.Empty)
                throw new ArgumentException("Opportunity ID is required", nameof(opportunityId));

            if (collaborationRequirementId <= default(int))
                throw new ArgumentException("Collaboration requirement ID is required", nameof(collaborationRequirementId));

            OpportunityId = opportunityId;
            CollaborationRequirementId = collaborationRequirementId;
        }
        

    }
}
