using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;

public class ChallengeRequest : AggregateRoot
{
    private readonly List<ChallengeRequestAttachment> _attachments = new();
    private readonly List<ChallengeRequestRevisionComment> _revisionComments = new();
    private readonly List<ChallengeTechnologiesRequest> _technologies = new();
    private readonly List<ChallengeTrackingHistory> _trackingHistory = new();
    private readonly List<CampaignRequestLinkedChallenge> _campaignRequestLinkedChallenge = new();

    public Guid UserId { get; set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid SourceCompanyId { get; set; }
    public ChallengeRequestAssociatedProvider SourceCompany { get; private set; }
    public Guid AssociatedSectorId { get; private set; }
    public ChallengeRequestAssociatedSector AssociatedSector { get; private set; }
    public string SubmitterName { get; private set; }
    public int PriorityLevelId { get; private set; }
    public ChallengeStatus ChallengeStatus { get; set; }
    public string UserEmail { get; private set; }


    public long ShortId { get; private set; }
    public bool? IsDraft { get; private set; }
    public bool? IsArchived { get; private set; }

    [Timestamp]
    public byte[] Version { get; set; }

    [NotMapped]
    public string shortId
    {
        get { return $"INN{ShortId.ToString("D3")}"; }
    }

    public IReadOnlyCollection<ChallengeRequestAttachment> Attachments => _attachments.AsReadOnly();
    public IReadOnlyCollection<ChallengeRequestRevisionComment> RevisionComments => _revisionComments.AsReadOnly();
    public IReadOnlyCollection<ChallengeTechnologiesRequest> Technologies => _technologies.AsReadOnly();
    public IReadOnlyCollection<ChallengeTrackingHistory> TrackingHistory => _trackingHistory.AsReadOnly();
    public ICollection<CampaignRequestLinkedChallenge> CampaignRequests => _campaignRequestLinkedChallenge;

    private ChallengeRequest() { }

    private ChallengeRequest(
        Guid userId,
        string name,
        string description,
        ChallengeRequestAssociatedProvider sourceCompany,
        ChallengeRequestAssociatedSector sector,
        string submitterName,
        int priorityLevelId,
        bool? isDraft,
        string SubmitterEmail)
    {
        UserId = userId;
        Name = name;
        Description = description;
        SourceCompany = sourceCompany;
        AssociatedSector = sector;
        SubmitterName = submitterName;
        PriorityLevelId = priorityLevelId;
        ChallengeStatus = isDraft == true ? ChallengeStatus.Draft : ChallengeStatus.Pending;
        IsDraft = isDraft;
        UserEmail = SubmitterEmail;
        AddTrackingHistory("Challenge created", ChallengeStatus.Pending, submitterName);
    }

    public static Result<ChallengeRequest> Create(
        Guid userId,
        string name,
        string description,
        Guid sourceCompanyId,
        string sourceCompanyName,
        Guid associatedSectorId,
        string associatedSectorName,
        string submitterName,
        int priorityLevelId,
        ChallengeRequestAssociatedProvider? sourceCompany,
        ChallengeRequestAssociatedSector? associatedSector,
        bool? isDraft,
        string SubmitterEmail)
    {
        if (userId == Guid.Empty)
            return Result<ChallengeRequest>.Failure("User ID is required");

        if (string.IsNullOrWhiteSpace(name))
            return Result<ChallengeRequest>.Failure("Challenge name is required");

        if (string.IsNullOrWhiteSpace(description))
            return Result<ChallengeRequest>.Failure("Description is required");

        if (string.IsNullOrWhiteSpace(submitterName))
            return Result<ChallengeRequest>.Failure("Submitter name is required");

        if (string.IsNullOrWhiteSpace(SubmitterEmail))
            return Result<ChallengeRequest>.Failure("Submitter Email is required");

        if (!EmailRegex.IsMatch(SubmitterEmail))
            return Result<ChallengeRequest>.Failure("Invalid email format");

        if (sourceCompany == null)
            sourceCompany = new ChallengeRequestAssociatedProvider(sourceCompanyId, sourceCompanyName);

        if (associatedSector == null)
             associatedSector = new ChallengeRequestAssociatedSector(associatedSectorId, associatedSectorName);

        var challengeRequest = new ChallengeRequest(
            userId,
            name,
            description,
            sourceCompany,
            associatedSector,
            submitterName,
            priorityLevelId,
            isDraft,
            SubmitterEmail);

        return Result<ChallengeRequest>.Success(challengeRequest);
    }

    public void UpdateStatus(ChallengeStatus newStatus, string updatedBy, string? note = null)
    {
        ChallengeStatus = newStatus;
        AddTrackingHistory(note ?? $"Status changed to {newStatus}", newStatus, updatedBy);
    }

    public void AddAttachment(ChallengeRequestAttachment attachment)
    {
        _attachments.Add(attachment);
        AddTrackingHistory($"Attachment '{attachment.Metadata.Name}' added", ChallengeStatus);
    }

    public Result<bool> AddRevisionComment(ChallengeRequestRevisionComment comment)
    {
        if (comment is null)
            throw new ArgumentNullException(nameof(comment));

        var previous = _revisionComments.LastOrDefault();
        if (previous != null)
        {
            previous.IsCurrent = false;
        }
        _revisionComments.Add(comment);
        AddTrackingHistory($"Revision comment added by {comment.Author}", ChallengeStatus);
        return Result<bool>.Success(true);
    }

  

    private void AddTrackingHistory(string description, ChallengeStatus status, string? changedBy = null)
    {
        _trackingHistory.Add(new ChallengeTrackingHistory(description, status, changedBy));
    }

    public Result Archive(string userId)
    {
        if (ChallengeStatus != ChallengeStatus.Approved)
            return Result.Failure("Only approved requests can be archived");

        if (IsArchived == true)
            return Result.Failure("Request is already archived");

        IsArchived = true;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;

        AddTrackingHistory("Challenge archived", ChallengeStatus);

        return Result.Success();
    }

    public Result Unarchive(string userId)
    {
        if (ChallengeStatus != ChallengeStatus.Approved)
            return Result.Failure("Only approved requests can be unarchived");

        if (IsArchived != true)
            return Result.Failure("Request is not archived");

        IsArchived = false;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;

        AddTrackingHistory("Challenge unarchived", ChallengeStatus);

        return Result.Success();
    }

    public void Update(string name,
                       string description,
                       int priorityId,
                       Guid associatedSectorId,
                       string associatedSectorName, 
                       ChallengeRequestAssociatedSector? associatedSector,
                       ChallengeStatus status)   
    { 
        Name = name;
        Description = description;
        ChallengeStatus = status;
        PriorityLevelId = priorityId;

        if (associatedSector == null && (associatedSectorId != AssociatedSectorId))
            associatedSector = new ChallengeRequestAssociatedSector(associatedSectorId, associatedSectorName);


        if(associatedSectorId != AssociatedSectorId)
            AssociatedSector = associatedSector;

    }



    public Result<bool> RemoveAttachment(Guid attachmentId, string deletedBy)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId && !a.IsDeleted);
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
        AddTrackingHistory("Attachment Removed", ChallengeStatus);
        return Result<bool>.Success(true);
    }

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
}
