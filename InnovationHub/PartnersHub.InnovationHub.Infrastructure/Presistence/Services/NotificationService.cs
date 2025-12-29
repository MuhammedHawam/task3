using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EmailParameters _emailParams;
        public NotificationService(ILogger<NotificationService> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<EmailParameters> options)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _emailParams = options.Value;
        }
        private async Task SendEmail(EmailNotificationModel emailDto)
        {
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Authorization header is missing or not in Bearer format.");
                throw new UnauthorizedAccessException("Missing or invalid authorization token in the current request.");
            }

            using var httpClient = _httpClientFactory.CreateClient(Constants.NotificationClient);

            httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

            if (!httpClient.DefaultRequestHeaders.Accept.Any())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var requestUri = Constants.EmailNotificationPath;

            HttpResponseMessage response;
            try
            {
                response = await httpClient.GetAsync(requestUri);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while calling SCIM endpoint at {Uri}", requestUri);
                throw;
            }

            response.EnsureSuccessStatusCode();

        }

        #region Challenge
        public async Task SendChallengeSubmittedNotificationAsync(Guid ChallengeRequestId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Challenge {ChallengeRequestId} Has been {status}.",
                ChallengeRequestId, "Submitted");


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { _emailParams.ChallengeModuleReviewer },
                //cc = new List<string> { _emailParams.ChallengeModuleCC },
                subject = _emailParams.ChallengeSubmittedSubject,
                body = _emailParams.ChallengeSubmittedBody
            });
        }

        public async Task SendChallengeApprovedNotificationAsync(Guid challengeRequestId,string submitterEmail, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Challenge {challengeRequestId} Approved. Notification sent.", challengeRequestId);


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { submitterEmail },
                subject = _emailParams.ChallengeApprovedSubject,
                body = _emailParams.ChallengeApprovedBody
            });
        }


        public async Task SendChallengeReturnedNotificationAsync(Guid challengeRequestId, string submitterEmail,string returnedReason, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Challenge {challengeRequestId} Returned. Reason: {returnedReason}.", challengeRequestId, returnedReason);


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { submitterEmail },
                subject = _emailParams.ChallengeReturnedSubject,
                body = $"{_emailParams.ChallengeReturnedBody} {returnedReason}"
            });
        }

        public async Task SendChallengeLinkedTechnologyNotificationAsync(Guid challengeRequestId, string submitterEmail, string technology, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(" {technology} Technology linked to your challenge.", challengeRequestId, technology);


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { submitterEmail , _emailParams.ChallengeSectorLeadMail},
                subject = _emailParams.ChallengeLinkedToTechnologySubject,
                body = $"{technology}  {_emailParams.ChallengeLinkedToTechnologyBody}"
            });
        }

        public async Task SendScreeningRequestNotificationAsync(Guid challengeRequestId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("New screening request pending review. ");


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> {  _emailParams.ChallengeSectorLeadMail },
                subject = _emailParams.ChallengeScreeningRequestSubmittedSubject,
                body = _emailParams.ChallengeScreeningRequestSubmittedBody
            });
        }

        #endregion

        #region Campaign

        public async Task SendCampaignSubmittedNotificationAsync(Guid campaignRequestId,string campaignName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Campaign {CampaignRequestId} Has been {status}.",
                campaignRequestId, "Submitted");


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { _emailParams.InnovationLeadershipMail },
                subject = _emailParams.CampaignSubmittedSubject,
                body = _emailParams.CampaignSubmittedBody.Replace("{name}", campaignName)
            
            });
        }

        public async Task SendCampaignApprovedNotificationAsync(Guid campaignRequestId, string campaignName,string campaignOwnerEmail, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Challenge {CampaignRequestId} Has been {status}.",
                campaignRequestId, "Approved");


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { campaignOwnerEmail },
                subject = _emailParams.CampaignApprovedSubject,
                body = _emailParams.CampaignApprovedBody.Replace("{name}", campaignName)

            });
        }

        public async Task SendCampaignChangesRequestedNotificationAsync(Guid campaignRequestId, string campaignName, string campaignOwnerEmail, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Challenge {CampaignRequestId} Has been {status}.",
                campaignRequestId, "ChangesRequested");


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { campaignOwnerEmail },
                subject = _emailParams.CampaignChangesRequestedSubject,
                body = _emailParams.CampaignChangesRequestedBody.Replace("{name}", campaignName)

            });
        }

        public async Task SendCampaignPublishedNotificationAsync(Guid campaignRequestId, string campaignName, List<string> communityMembersMailList, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Challenge {CampaignRequestId} Has been {status}.",
                campaignRequestId, "Published");


            await SendEmail(new EmailNotificationModel
            {
                to = communityMembersMailList,
                subject = _emailParams.CampaignPublishedSubject,
                body = _emailParams.CampaignPublishedBody.Replace("{name}", campaignName)

            });
        }


        #endregion
    }
}
