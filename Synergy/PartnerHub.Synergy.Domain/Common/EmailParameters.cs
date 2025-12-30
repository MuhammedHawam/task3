using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Domain.Common
{
    public class EmailParameters
    {
        public string SynergyModuleReviever { get; set; }
        public string SynergyModuleCC { get; set; }

        // Opportunity Templates
        public string OpportunitySubmittedSubject { get; set; }
        public string OpportunitySubmittedBody { get; set; }
        public string OpportunityApprovedSubject { get; set; }
        public string OpportunityApprovedBody { get; set; }
        public string OpportunityPublishedSubject { get; set; }
        public string OpportunityPublishedBody { get; set; }
        public string OpportunityRejectedSubject { get; set; }
        public string OpportunityRejectedBody { get; set; }

        // Success Story Templates
        public string SuccessStorySubmittedSubject { get; set; }
        public string SuccessStorySubmittedBody { get; set; }
        public string SuccessStoryApprovedSubject { get; set; }
        public string SuccessStoryApprovedBody { get; set; }
        public string SuccessStoryPublishedSubject { get; set; }
        public string SuccessStoryPublishedBody { get; set; }
        public string SuccessStoryRejectedSubject { get; set; }
        public string SuccessStoryRejectedBody { get; set; }

        public string BaseURL { get; set; }

        public List<CompanyAssetManager> AssetManagersList { get; set; } = new();

        public List<SynergyMember> SynergyTeam {  get; set; } = new();
    }


    public class CompanyAssetManager
    {
        public string PCName { get; set; }
        public string AssetManagerEmail { get; set; }
        public string Sector { get; set; }
        public string Industry { get; set; }
        public string SectorHeadName  { get; set; }
        public string SectorHeadEmail { get; set; }
        public string AssetManagerName { get; set; }
    }

    public class SynergyMember
    {
        public string SynergyMemberName { get; set; }
        public string Email { get; set;}

    }
}
