using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task SendChallengeSubmittedNotificationAsync(Guid ChallengeRequestId,  CancellationToken cancellationToken = default);
        Task SendChallengeApprovedNotificationAsync(Guid challengeRequestId, string submitterEmail, CancellationToken cancellationToken = default);

        Task SendChallengeReturnedNotificationAsync(Guid challengeRequestId, string submitterEmail, string returnedReason, CancellationToken cancellationToken = default);

        Task SendChallengeLinkedTechnologyNotificationAsync(Guid challengeRequestId, string submitterEmail, string technology, CancellationToken cancellationToken = default);

        Task SendScreeningRequestNotificationAsync(Guid challengeRequestId, CancellationToken cancellationToken = default);

        Task SendCampaignSubmittedNotificationAsync(Guid campaignRequestId, string campaignName, CancellationToken cancellationToken = default);

        Task SendCampaignApprovedNotificationAsync(Guid campaignRequestId, string campaignName, string campaignOwnerEmail, CancellationToken cancellationToken = default);

        Task SendCampaignChangesRequestedNotificationAsync(Guid campaignRequestId, string campaignName, string campaignOwnerEmail, CancellationToken cancellationToken = default);

        Task SendCampaignPublishedNotificationAsync(Guid campaignRequestId, string campaignName, List<string> communityMembersMailList, CancellationToken cancellationToken = default);
    }
}
