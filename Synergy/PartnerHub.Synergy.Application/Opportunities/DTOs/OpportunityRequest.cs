using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.Opportunities.DTOs
{
    public class OpportunityRequest
    {
        public Guid OpportunityId { get; set; }
        public string CompanyName { get; set; }
        public string Sector { get; set; }
        public string RepresentativeName { get; set; }
        public string RepresentativeEmail { get; set; }
        public string RepresentativePosition { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; }
    }
}
