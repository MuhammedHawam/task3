using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Domain.Common
{
    public class EmailParameters
    {
        #region Challenge 
        public string ChallengeModuleReviewer { get; set; }
        public string ChallengeSectorLeadMail { get; set; }
        public string ChallengeModuleCC { get; set; }

        // Challenge Templates
        public string ChallengeSubmittedSubject { get; set; }
        public string ChallengeSubmittedBody { get; set; }

        public string ChallengeApprovedSubject { get; set; }
        public string ChallengeApprovedBody { get; set; }

        public string ChallengeReturnedSubject { get; set; }
        public string ChallengeReturnedBody { get; set; }

        public string ChallengeLinkedToTechnologySubject { get; set; }
        public string ChallengeLinkedToTechnologyBody { get; set; }

        public string ChallengeScreeningRequestSubmittedSubject { get; set; }
        public string ChallengeScreeningRequestSubmittedBody { get; set; }
        #endregion

        #region Campaign
         
        public string InnovationLeadershipMail { get; set; }

        public string CampaignSubmittedSubject { get; set; }
        public string CampaignSubmittedBody { get; set; }

        public string CampaignApprovedSubject {  get; set; }
        public string CampaignApprovedBody { get; set; }

        public string CampaignChangesRequestedSubject { get; set; }
        public string CampaignChangesRequestedBody { get; set; }


        public string CampaignPublishedSubject { get; set; }
        public string CampaignPublishedBody { get; set; }


        public string IdeaSubmittedSubject { get; set; }
        public string IdeaSubmittedBody { get; set; }

        public string IdeaEvaluationCompletedSubject { get; set; }
        public string IdeaEvaluationCompletedBody { get; set; }

        #endregion

        public string BaseURL { get; set; }
    }
}
