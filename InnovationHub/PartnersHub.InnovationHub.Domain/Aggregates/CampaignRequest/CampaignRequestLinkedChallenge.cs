using PartnersHub.InnovationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Domain.Aggregates.Campaigns
{
    public class CampaignRequestLinkedChallenge : Entity
    {
        public Guid CampaignRequestId { get; private set; }
        public Guid ChallengeRequestId { get; private set; }
        public bool IsDeleted { get; private set; }
        public Guid? DeletedBy { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        private CampaignRequestLinkedChallenge() { }

        public CampaignRequestLinkedChallenge(Guid campaignRequestId, Guid challengeId)
        {
            if (campaignRequestId == Guid.Empty) throw new ArgumentNullException("CampaignId is required.", nameof(campaignRequestId));
            if (challengeId == Guid.Empty) throw new ArgumentNullException("ChallengeId is required.", nameof(ChallengeRequestId));

            CampaignRequestId = campaignRequestId;
            ChallengeRequestId = challengeId;

        }

        public static Result<CampaignRequestLinkedChallenge> Create(Guid campaignRequestId, Guid challengeId)
        {
            if (campaignRequestId == Guid.Empty)
                return Result<CampaignRequestLinkedChallenge>.Failure("CampaignRequestId is required.");

            if (challengeId == Guid.Empty)
                return Result<CampaignRequestLinkedChallenge>.Failure("challengeId is required.");


            var campaignRequestSponsor = new CampaignRequestLinkedChallenge(campaignRequestId,challengeId);

            return Result<CampaignRequestLinkedChallenge>.Success(campaignRequestSponsor);
        }

        public Result MarkAsDeleted(Guid deletedBy)
        {
            if (IsDeleted)
                return Result.Failure("Challenge is already deleted");

            if (deletedBy == Guid.Empty)
                return Result.Failure("Deleted by user is required");

            IsDeleted = true;
            DeletedBy = deletedBy;
            DeletedAt = DateTime.UtcNow;

            return Result.Success();
        }
    }
}
