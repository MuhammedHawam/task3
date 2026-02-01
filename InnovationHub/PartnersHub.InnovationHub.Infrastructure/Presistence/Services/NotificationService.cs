using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Domain.Aggregates;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
        private const string TemplateFolderName = "Templates/Emails/Images";
        public NotificationService(ILogger<NotificationService> logger, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IOptions<EmailParameters> options)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _emailParams = options.Value;
        }
        private async Task SendEmail(EmailNotificationModel emailDto)
        {

            _logger.LogInformation("Sending email notification. Payload: {@EmailDto}", emailDto);
            // 1. Extract Token safely
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Authorization header is missing or invalid.");
                throw new UnauthorizedAccessException("Missing or invalid authorization token.");
            }

            // 2. Use HttpClient efficiently
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Constants.NotificationClient);
         
                // Clear and set headers to avoid accumulation if the client is reused
                httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);

                if (!httpClient.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }

                try
                {
                    _logger.LogInformation("Sending email {dto}", emailDto);
                    _logger.LogInformation("email endpoint {url}", httpClient.BaseAddress);

                    // 3. PostAsJsonAsync handles serialization and Content-Type headers automatically
                    var response = await httpClient.PostAsJsonAsync( Constants.EmailNotificationPath, emailDto);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Email API returned {StatusCode}: {Error}", response.StatusCode, errorContent);
                    }

                    _logger.LogInformation("notification response: {response}", response);

                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                {
                    _logger.LogError(ex, "Network or timeout error calling notification service at {Uri}", Constants.EmailNotificationPath);
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string LoadTemplate(string templateName, Dictionary<string, string> placeholders)
        {
            try
            {
                var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "Emails", templateName);
                var templateContent = File.ReadAllText(templatePath);

                foreach (var placeholder in placeholders)
                {
                    templateContent = templateContent.Replace(placeholder.Key, placeholder.Value);
                }


                templateContent = InlineImage(templateContent, "BG.png");
                templateContent = InlineImage(templateContent, "PIF_Logo.png");
                templateContent = InlineImage(templateContent, "PIF.png");
                templateContent = InlineImage(templateContent, "Platform_Logo.png");

                return templateContent;
            }
            catch (Exception ex)
            {

                throw;
            }
          
        }

        private static string InlineImage(string html, string fileName)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, TemplateFolderName, fileName);
            if (!File.Exists(filePath))
            {
                return html;
            }

            var base64 = Convert.ToBase64String(File.ReadAllBytes(filePath));
            var dataUri = $"data:image/png;base64,{base64}";

            // The template references images as "./<fileName>" (both in img src and css url()).
            return html.Replace($"./Images/{fileName}", dataUri, StringComparison.Ordinal);
        }

        #region Challenge
        public async Task SendChallengeSubmittedNotificationAsync(Guid challengeRequestId,string challengeName ,CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Challenge {challengeName} Has been {status}.",
                challengeName, "Submitted");

            var messageEn = _emailParams.ChallengeSubmittedBody.Replace("{ChallengeName}", challengeName);

            int atIndex = _emailParams.ChallengeModuleReviewer.IndexOf('@');

            string recieverName = atIndex > -1 ? _emailParams.ChallengeModuleReviewer.Substring(0, atIndex) : _emailParams.ChallengeModuleReviewer;

            var placeholders = new Dictionary<string, string>
                              {
                                { "{messageEn}", messageEn },
                                { "{recieverName}", recieverName },
                                { "{module}", "challenges" },
                                { "{BaseURL}", _emailParams.BaseURL },
                                { "{messageAr}", messageEn },
                              };

            var emailBody = LoadTemplate("MailTemplate.html", placeholders);


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { _emailParams.ChallengeModuleReviewer },
               // cc = new List<string> { _emailParams.ChallengeModuleCC },
                subject = _emailParams.ChallengeSubmittedSubject,
                body = emailBody,
                isHtml = true
            });
        }

        public async Task SendChallengeApprovedNotificationAsync(Guid challengeRequestId,string challengeName,string submitterEmail, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Challenge {challengeName} Approved. Notification sent.", challengeName);


            var messageEn = _emailParams.ChallengeApprovedBody.Replace("{ChallengeName}", challengeName);

            int atIndex = submitterEmail.IndexOf('@');

            string recieverName = atIndex > -1 ? submitterEmail.Substring(0, atIndex) : submitterEmail;

            var placeholders = new Dictionary<string, string>
                              {
                                { "{messageEn}", messageEn },
                                { "{recieverName}", recieverName },
                                { "{module}", "challenges" },
                                { "{BaseURL}", _emailParams.BaseURL },
                                { "{messageAr}", messageEn },
                              };

            var emailBody = LoadTemplate("MailTemplate.html", placeholders);


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { submitterEmail },
                cc = new List<string> { _emailParams.ChallengeModuleCC },
                subject = _emailParams.ChallengeApprovedSubject,
                body = emailBody,
                isHtml = true
            });
        }


        public async Task SendChallengeReturnedNotificationAsync(Guid challengeRequestId, string challengeName , string submitterEmail,string returnedReason, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Challenge {challengeRequestId} Returned. Reason: {returnedReason}.", challengeRequestId, returnedReason);


            var messageEn = $"{_emailParams.ChallengeReturnedBody} {returnedReason}".Replace("{ChallengeName}", challengeName);

            int atIndex = submitterEmail.IndexOf('@');

            string recieverName = atIndex > -1 ? submitterEmail.Substring(0, atIndex) : submitterEmail;

            var placeholders = new Dictionary<string, string>
                              {
                                { "{messageEn}", messageEn },
                                { "{recieverName}", recieverName },
                                { "{module}", "challenges" },
                                { "{BaseURL}", _emailParams.BaseURL },
                                { "{messageAr}", messageEn },
                              };

            var emailBody = LoadTemplate("MailTemplate.html", placeholders);

            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { submitterEmail },
                cc = new List<string> { _emailParams.ChallengeModuleCC },
                subject = _emailParams.ChallengeReturnedSubject,
                body = emailBody
            });
        }

        public async Task SendChallengeLinkedTechnologyNotificationAsync(Guid challengeRequestId, string challengeName , string submitterEmail, string technology, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(" {technology} Technology linked to your challenge.", challengeRequestId, technology);

            var messageEn = $"{technology}  {_emailParams.ChallengeLinkedToTechnologyBody}";

            var placeholders = new Dictionary<string, string>
                              {
                                { "{messageEn}", messageEn },
                                { "{recieverName}", "all" },
                                { "{module}", "challenges" },
                                { "{BaseURL}", _emailParams.BaseURL },
                                { "{messageAr}", messageEn },
                              };

            var emailBody = LoadTemplate("MailTemplate.html", placeholders);

            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { submitterEmail , _emailParams.ChallengeSectorLeadMail},
                cc = new List<string> { _emailParams.ChallengeModuleCC },
                subject = _emailParams.ChallengeLinkedToTechnologySubject,
                body = emailBody,
                isHtml = true
            });
        }

        public async Task SendScreeningRequestNotificationAsync(Guid challengeRequestId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("New screening request pending review. ");

            var messageEn = _emailParams.ChallengeScreeningRequestSubmittedBody;

            var placeholders = new Dictionary<string, string>
                              {
                                { "{messageEn}", messageEn },
                                { "{recieverName}", "all" },
                                { "{module}", "challenges" },
                                { "{BaseURL}", _emailParams.BaseURL },
                                { "{messageAr}", messageEn },
                              };

            var emailBody = LoadTemplate("MailTemplate.html", placeholders);


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> {  _emailParams.ChallengeSectorLeadMail },
                cc = new List<string> { _emailParams.ChallengeModuleCC },
                subject = _emailParams.ChallengeScreeningRequestSubmittedSubject,
                body = emailBody,
                isHtml = true
            });
        }

        #endregion

        #region Campaign

        public async Task SendCampaignSubmittedNotificationAsync(Guid campaignRequestId,string campaignName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Campaign {CampaignRequestId} Has been {status}.",
                campaignRequestId, "Submitted");

            int atIndex = _emailParams.InnovationLeadershipMail.IndexOf('@');

            string recieverName = atIndex > -1 ? _emailParams.InnovationLeadershipMail.Substring(0, atIndex) : _emailParams.InnovationLeadershipMail;

            var messageEn = _emailParams.CampaignSubmittedBody.Replace("{name}", campaignName);

            var placeholders = new Dictionary<string, string>
                              {
                                { "{messageEn}", messageEn },
                                { "{recieverName}", recieverName },
                                { "{module}", "campaigns" },
                                { "{BaseURL}", _emailParams.BaseURL },
                                { "{messageAr}", messageEn },
                              };

            var emailBody = LoadTemplate("MailTemplate.html", placeholders);


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { _emailParams.InnovationLeadershipMail },
                cc = new List<string> { _emailParams.ChallengeModuleCC },
                subject = _emailParams.CampaignSubmittedSubject,
                body = emailBody,
                isHtml = true

            });
        }

        public async Task SendCampaignApprovedNotificationAsync(Guid campaignRequestId, string campaignName,string campaignOwnerEmail, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Challenge {CampaignRequestId} Has been {status}.",
                campaignRequestId, "Approved");

            int atIndex = campaignOwnerEmail.IndexOf('@');

            string recieverName = atIndex > -1 ? campaignOwnerEmail.Substring(0, atIndex) : campaignOwnerEmail;

            var messageEn = _emailParams.CampaignApprovedBody.Replace("{name}", campaignName);

            var placeholders = new Dictionary<string, string>
                              {
                                { "{messageEn}", messageEn },
                                { "{recieverName}", recieverName },
                                { "{module}", "campaigns" },
                                { "{BaseURL}", _emailParams.BaseURL },
                                { "{messageAr}", messageEn },
                              };

            var emailBody = LoadTemplate("MailTemplate.html", placeholders);

            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { campaignOwnerEmail },
                cc = new List<string> { _emailParams.ChallengeModuleCC },
                subject = _emailParams.CampaignApprovedSubject,
                body = emailBody,
                isHtml = true

            });
        }

        public async Task SendCampaignChangesRequestedNotificationAsync(Guid campaignRequestId, string campaignName, string campaignOwnerEmail, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Challenge {CampaignRequestId} Has been {status}.",
                campaignRequestId, "ChangesRequested");

            int atIndex = campaignOwnerEmail.IndexOf('@');

            string recieverName = atIndex > -1 ? campaignOwnerEmail.Substring(0, atIndex) : campaignOwnerEmail;

            var messageEn = _emailParams.CampaignChangesRequestedBody.Replace("{name}", campaignName);

            var placeholders = new Dictionary<string, string>
                              {
                                { "{messageEn}", messageEn },
                                { "{recieverName}", recieverName },
                                { "{module}", "campaigns" },
                                { "{BaseURL}", _emailParams.BaseURL },
                                { "{messageAr}", messageEn },
                              };

            var emailBody = LoadTemplate("MailTemplate.html", placeholders);


            await SendEmail(new EmailNotificationModel
            {
                to = new List<string> { campaignOwnerEmail },
                cc = new List<string> { _emailParams.ChallengeModuleCC },
                subject = _emailParams.CampaignChangesRequestedSubject,
                body = emailBody,
                isHtml = true

            });
        }

        public async Task SendCampaignPublishedNotificationAsync(Guid campaignRequestId, string campaignName, List<string> communityMembersMailList, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Challenge {CampaignRequestId} Has been {status}.",
                campaignRequestId, "Published");



            var messageEn = _emailParams.CampaignChangesRequestedBody.Replace("{name}", campaignName);

            var placeholders = new Dictionary<string, string>
                              {
                                { "{messageEn}", messageEn },
                                { "{recieverName}", "all" },
                                { "{module}", "campaigns" },
                                { "{BaseURL}", _emailParams.BaseURL },
                                { "{messageAr}", messageEn },
                              };

            var emailBody = LoadTemplate("MailTemplate.html", placeholders);


            await SendEmail(new EmailNotificationModel
            {
                to = communityMembersMailList,
                cc = new List<string> { _emailParams.ChallengeModuleCC },
                subject = _emailParams.CampaignPublishedSubject,
                body = emailBody,
                isHtml = true

            });
        }


        #endregion
    }
}
