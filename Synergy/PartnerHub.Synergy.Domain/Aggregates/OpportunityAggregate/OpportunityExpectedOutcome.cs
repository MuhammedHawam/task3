using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate
{
    public class OpportunityExpectedOutcome : Entity
    {
        public Guid OpportunityId { get; set; }
        public int ExpectedOutcomeId { get; set; }
        private OpportunityExpectedOutcome() { }
        internal OpportunityExpectedOutcome(Guid opportunityId, int expectedOutcomeId)
        {
            if (opportunityId == Guid.Empty)
                throw new ArgumentException("Opportunity ID is required", nameof(opportunityId));

            if (expectedOutcomeId <= default(int))
                throw new ArgumentException("Expected outcome ID is required", nameof(expectedOutcomeId));

            OpportunityId = opportunityId;
            ExpectedOutcomeId = expectedOutcomeId;
        }
    }
}
