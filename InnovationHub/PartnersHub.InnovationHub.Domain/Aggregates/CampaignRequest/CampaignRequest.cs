using PartnersHub.InnovationHub.Domain.Aggregates.CampaignRequest;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using PartnersHub.InnovationHub.Domain.Events.Campaigns;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml.Linq;


namespace PartnersHub.InnovationHub.Domain.Aggregates.Campaigns
{
    public class CampaignRequest : AggregateRoot
    {
        public CampaignRequest() { }

        private readonly List<CampaignRequestSponsor> _sponsors = new();
        private readonly List<CampaignRequestEvaluator> _evaluators = new();
        private readonly List<CampaignRequestEvaluationCriteria> _campaignEvaluationCriterias = new();
        private readonly List<CampaignRequestLinkedChallenge> _linkedChallenges = new();
        private readonly List<CampaignRequestTermsAndCondition> _termsAndCondition = new();
        private readonly List<CampaignTrackingHistory> _trackingHistory = new();

        public string Name { get; private set; } 
        public string? Description { get; private set; }
        public string? ProblemStatement { get; private set; }
        public CampaignRequestStatus CampaignRequestStatus { get; set; } 
        public CampaignType Type { get; private set; } 
        public DateTime? LaunchDate { get; private set; }
        public DateTime? SubmissionDeadLine { get; private set; }
        public Guid OwnerId { get; private set; }
        public long ShortId { get; private set; }   
        public string OwnerName { get; private set; }  
        public string Comments { get; private set; }
        public byte[] RowVersion { get; private set; }
        public string UserEmail { get; private set; }

        [NotMapped]
        public string shortId
        {
            get { return $"INN{ShortId.ToString("D3")}"; }
        }

        public IReadOnlyCollection<CampaignRequestSponsor> Sponsors => _sponsors.AsReadOnly();
        public IReadOnlyCollection<CampaignRequestEvaluator> Evaluators => _evaluators.AsReadOnly();
        public ICollection<CampaignRequestEvaluationCriteria> EvaluationCriterias => _campaignEvaluationCriterias;
        public IReadOnlyCollection<CampaignRequestLinkedChallenge> LinkedChallenges => _linkedChallenges.AsReadOnly();
        public IReadOnlyCollection<CampaignRequestTermsAndCondition> TermsAndCondition => _termsAndCondition.AsReadOnly();
        public IReadOnlyCollection<CampaignTrackingHistory> TrackingHistory => _trackingHistory.AsReadOnly();


        #region Create
        public static Result<CampaignRequest> Create(Guid userId,
                                                     string name,
                                                     string desc,
                                                     string problem,
                                                     CampaignType type,
                                                     string ownerName,
                                                     DateTime? lunchDate,
                                                     string comments,
                                                     CampaignRequestStatus status,
                                                     List<(Guid Id, string Name)> SponsorsList,
                                                     List<Guid> challengeList,
                                                     string SubmitterEmail)
        {
            if (userId == null)
                return Result<CampaignRequest>.Failure("User ID is required");
           
            if (string.IsNullOrWhiteSpace(name))
                return Result<CampaignRequest>.Failure("Name is required");
            if (string.IsNullOrWhiteSpace(SubmitterEmail))
                return Result<CampaignRequest>.Failure("Submitter Email is required");

            if (!EmailRegex.IsMatch(SubmitterEmail))
                return Result<CampaignRequest>.Failure("Invalid email format");

            var campaignRequest = new CampaignRequest
            {
                OwnerId = userId,
                Name = name,
                Type = type,
                CampaignRequestStatus = status,
                Description = desc,
                ProblemStatement = problem,
                OwnerName = ownerName,
                LaunchDate = lunchDate,
                Comments = comments,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                UserEmail = SubmitterEmail
            };
            SponsorsList.ForEach(s =>{ campaignRequest.AddSponsor(s.Id, s.Name); });

            challengeList.ForEach(l =>{ campaignRequest.LinkChallenge(l); });

            campaignRequest.AddTrackingHistory("CampaignRequest created", campaignRequest.CampaignRequestStatus, userId, DateTime.UtcNow, comments,"Status","", Enum.GetName(typeof(CampaignRequestStatus), CampaignRequestStatus.Requested));
           
            return Result<CampaignRequest>.Success(campaignRequest);
        }
        #endregion

        #region Create Campaign
        public static Result<CampaignRequest> CreateCampaign(Guid userId,
                                                             string name,
                                                             string desc, 
                                                             string problem,
                                                             CampaignType type,
                                                             string ownerName,
                                                             DateTime lunchDate, 
                                                             DateTime  SubmissionDate,
                                                             CampaignRequestStatus status,
                                                             List<(Guid Id, string Name)> EvaluatorsList,
                                                             List<(Guid Id, string Name)> SponsorsList,
                                                             List<(string name, int value)> EvaluationCriteriaList,
                                                             List<Guid>? LinkedDevCoChallenges,
                                                     string SubmitterEmail)
        {
            if (userId == null)
                return Result<CampaignRequest>.Failure("User ID is required");

            if (string.IsNullOrWhiteSpace(name))
                return Result<CampaignRequest>.Failure("Name is required");

            if (!EmailRegex.IsMatch(SubmitterEmail))
                return Result<CampaignRequest>.Failure("Invalid email format");

            var campaignRequest = new CampaignRequest
            {
                OwnerId = userId,
                Name = name,
                Type = type,
                CampaignRequestStatus = status,
                Description = desc,
                ProblemStatement = problem,
                OwnerName = ownerName,
                LaunchDate = lunchDate,
                SubmissionDeadLine = SubmissionDate,
                Comments = string.Empty,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                UserEmail = SubmitterEmail
            };

            SponsorsList.ForEach(s => { campaignRequest.AddSponsor(s.Id, s.Name); });

            EvaluationCriteriaList.ForEach(e => { campaignRequest.AddOrUpdateEvaluationCriteria(e.name, e.value); });

            EvaluatorsList.ForEach(v => { campaignRequest.AddEvaluator(v.Id); });

            LinkedDevCoChallenges.ForEach(l => { campaignRequest.LinkChallenge(l); });

            campaignRequest.AddTrackingHistory("Campaign created", campaignRequest.CampaignRequestStatus, userId, DateTime.UtcNow, "", "Status", "", Enum.GetName(typeof(CampaignRequestStatus), status));

            return Result<CampaignRequest>.Success(campaignRequest);
        }
        #endregion

        #region Update
        /// <summary>
        /// PreFill Content & Relationships (when Convet a request  to Draft)
        /// Exisiting collections are cleared then refilled from provided Ids
        /// </summary>
        public void Update(
            string? shortDesc,
            string? desc,
            IEnumerable<Guid>? challengeIds,
            IEnumerable<Guid>? sponsorIds,
            DateTime? launchDate, 
            DateTime? deadLine,
            CampaignType type)
        {
            Description = shortDesc;
            ProblemStatement = desc;
            _linkedChallenges.Clear();

            if (challengeIds != null)
            {
                foreach (var challengedId in challengeIds.Where(x => x != Guid.Empty).Distinct())
                    _linkedChallenges.Add(new CampaignRequestLinkedChallenge(Id, challengedId));
            }

            _sponsors.Clear();

            if (sponsorIds != null)
            {
                foreach (var sponsorId in sponsorIds.Where(x => x != Guid.Empty).Distinct())
                    _sponsors.Add( CampaignRequestSponsor.Create(Id, sponsorId,"").Value);
            }

            if (launchDate.HasValue && deadLine.HasValue && deadLine <= launchDate)
                throw new InvalidOperationException("Submission deadline must be after launch date.");

            LaunchDate = launchDate;
            SubmissionDeadLine = deadLine;
            Type = type;
            Description = shortDesc;
            ProblemStatement = desc;

        }
        #endregion

        #region Sponsor  
        public void AddSponsor(Guid sponsorId, string sponserName)
        {
            if (sponsorId == Guid.Empty) throw new ArgumentException("SponsorId is required.", nameof(sponsorId));
            if (_sponsors.Any(s => s.SponsorId == sponsorId)) return; // No Duplication

            _sponsors.Add(CampaignRequestSponsor.Create(Id, sponsorId, sponserName).Value);

        }

        public void RemoveSponsor(Guid sponsorId)
        {
            var foundSponsor = _sponsors.FirstOrDefault(s => s.SponsorId == sponsorId);
            if (foundSponsor is null) return;
            _sponsors.Remove(foundSponsor);

        }
        #endregion

        #region Evaluator  
        public void AddEvaluator(Guid evaluatorId)
        {
            if (evaluatorId == Guid.Empty) throw new ArgumentException("EvaluatorId is required.", nameof(evaluatorId));
            if (_evaluators.Any(s => s.EvaluatorId == evaluatorId)) return; // No Duplication
            _evaluators.Add(new CampaignRequestEvaluator(Id, evaluatorId));
        }

        public void RemoveEvaluator(Guid evaluatorId)
        {
            var foundEvaluator = _evaluators.FirstOrDefault(s => s.EvaluatorId == evaluatorId);
            if (foundEvaluator is null) return;
            _evaluators.Remove(foundEvaluator);
        }
        #endregion


        #region Challenge
        public void LinkChallenge(Guid challengeId)
        {
            if (challengeId == Guid.Empty) throw new ArgumentException("ChallengeId is required.", nameof(challengeId));
            if (_linkedChallenges.Any(x => x.ChallengeRequestId == challengeId)) return;
            _linkedChallenges.Add(new CampaignRequestLinkedChallenge(Id, challengeId));
        }

        public void UnLinkChallenge(Guid challengeId)
        {
            var foundLinkedChallenge = _linkedChallenges.FirstOrDefault(x => x.ChallengeRequestId == challengeId);
            if (foundLinkedChallenge is null) return;
            _linkedChallenges.Remove(foundLinkedChallenge);
        }
        #endregion

        #region Terms and Condition
        public void AddTermsAndCondition(CampaignRequestTermsAndCondition attachment)
        {
            _termsAndCondition.Add(attachment);
        }
        public Result<bool> RemoveTermsAndCondition(Guid attachmentId, string deletedBy)
        {
            var attachment = _termsAndCondition.FirstOrDefault(a => a.Id == attachmentId && !a.IsDeleted);
            if (attachment == null)
            {
                return Result<bool>.Failure("Attachment not found");
            }


            var result = attachment.MarkAsDeleted(Guid.Parse(deletedBy));
            if (result.IsFailure)
            {
                return Result<bool>.Success(false);
            }

            UpdatedAt = DateTime.Now;
            AddTrackingHistory("Attachment Removed", CampaignRequestStatus, Guid.Parse(deletedBy), DateTime.UtcNow ,"","Remove attachments", "","");
            return Result<bool>.Success(true);
        }
        #endregion

        #region Evaluation 
        public void AddOrUpdateEvaluationCriteria(string name , int value)
        {
            var foundEvaluationCriteria=_campaignEvaluationCriterias.FirstOrDefault(x => x.CriteriaName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (foundEvaluationCriteria is null)
            {
                _campaignEvaluationCriterias.Add(new CampaignRequestEvaluationCriteria(Id, name, value));
            }
            else
            {
                foundEvaluationCriteria.Update(name, value);
            }

        }
        #endregion

        #region WorkFlow

        /// <summary>
        /// Submitted => Published
        /// </summary>
        /// <param name="nowUtc"></param>
        public void Publish(DateTime nowUtc)
        {
            if (CampaignRequestStatus != CampaignRequestStatus.Submitted)
                throw new InvalidOperationException("Only Submitted Campaings can be published.");

            if (SubmissionDeadLine.HasValue && nowUtc > SubmissionDeadLine.Value)
                throw new InvalidOperationException("Cannot publish after the submission deadline.");

            CampaignRequestStatus = CampaignRequestStatus.Published;
            AddDomainEvent(new CampaignRequestPublishedEvent(Id));

        }

        /// <summary>
        /// Published => Closed
        /// </summary>
        public void Close()
        {
            if (CampaignRequestStatus != CampaignRequestStatus.Published)
                throw new InvalidOperationException("Only Published Campaings can be closed.");

            CampaignRequestStatus = CampaignRequestStatus.Closed;
        }


        /// <summary>
        /// Soft Final State
        /// </summary>
        public void Archive()
        {
            if (CampaignRequestStatus == CampaignRequestStatus.Archived) return;
            CampaignRequestStatus = CampaignRequestStatus.Archived;
        }

        #endregion

        /// <summary>
        /// ONLY Published Campaign Can Participate in Status (Open, Closed, Upcoming)
        /// </summary>
        /// <param name="nowUtc"></param>
        /// <returns></returns>
        public CampaignStatus GetCampaignStatus(DateTime nowUtc)
        {

            if (CampaignRequestStatus != CampaignRequestStatus.Published)
                return CampaignStatus.Closed; //Safty default for non-Published

            if (LaunchDate.HasValue && nowUtc < LaunchDate.Value)
                return CampaignStatus.Upcoming;

            if (SubmissionDeadLine.HasValue && nowUtc > SubmissionDeadLine.Value)
                return CampaignStatus.Closed;

            return CampaignStatus.Open;

        }
        private static readonly Regex EmailRegex = new(
       @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
       RegexOptions.Compiled | RegexOptions.IgnoreCase);


        #region Update
        public void Update(string name, string desc, CampaignType type, DateTime? lunchDate, CampaignRequestStatus status)
        {
            Name = name;
            Description = desc;
            Type = type;
            LaunchDate = lunchDate;
            CampaignRequestStatus = status;
            _sponsors.Clear();
            _linkedChallenges.Clear();  
            _campaignEvaluationCriterias.Clear();
            _evaluators.Clear();    
        }

        #endregion
        #region Tracking
        public void AddTrackingHistory(string action,
                                       CampaignRequestStatus status,
                                       Guid performedBy,
                                       DateTime performedAt,
                                       string comments,
                                       string fieldsChanged,
                                       string oldValue,
                                       string newvalue)
        {
            _trackingHistory.Add(new CampaignTrackingHistory(status, action, performedBy,performedAt,comments,fieldsChanged,oldValue,newvalue));
        }
        #endregion

    }
}
