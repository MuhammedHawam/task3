using PartnersHub.Synergy.Domain.Aggregates;
using PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate;
using PartnersHub.Synergy.Domain.Aggregates.Synergy.Lookups;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.Events;
using PartnersHub.Synergy.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;

public class SuccessStory : AggregateRoot
{
    private readonly List<SuccessStoryAttachment> _attachments = new();
    private readonly List<SuccessStorySynergyCompany> _collaboratedCompanies = new List<SuccessStorySynergyCompany>();
    private readonly List<SuccessStoryOpportunity> _associatedOpportunities = new List<SuccessStoryOpportunity>();
    //ToDo : Convert RequestId from primitive type to value object
    public string RequestId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Title Title { get; private set; } = null!;
    public Description Description { get; private set; } = null!;
    public int SuccessStoryTypeId { get; private set; }
    public SuccessStoryType SuccessStoryType { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int CollaborationStatusId { get; private set; }
    public IReadOnlyCollection<SuccessStoryAttachment> Attachments => _attachments.AsReadOnly();
    public ICollection<SuccessStorySynergyCompany> CollaboratedProfiles => _collaboratedCompanies;
    public IReadOnlyCollection<SuccessStoryOpportunity> AssociatedOpportunities => _associatedOpportunities.AsReadOnly();
    // Terms and Conditions Reference and Acceptance
    public Guid TermsAndConditionId { get; private set; }
    public bool TermsAccepted { get; private set; }
    public DateTime? TermsAcceptedAt { get; private set; }
    public Guid? TermsAcceptedBy { get; private set; }
    public SuccessStoryStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? RejectedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public Guid? SectorId { get; private set; }
    public string? SectorName { get; private set;}
    public string? UserEmail { get; private set; }

    private SuccessStory() { }

    private SuccessStory(Guid companyId, Title title, Description description, int successStoryTypeId, string requestId,
        DateTime startDate, DateTime endDate, int collaborationStatusId, Guid termsAndConditionId, Guid createdBy, Guid? sectorId , string? sectorName, string userEmail)
    {
        RequestId = requestId;
        CompanyId = companyId;
        Title = title;
        Description = description;
        SuccessStoryTypeId = successStoryTypeId;
        StartDate = startDate;
        EndDate = endDate;
        CollaborationStatusId = collaborationStatusId;
        TermsAndConditionId = termsAndConditionId;
        TermsAccepted = false;
        SectorId = sectorId;
        SectorName = sectorName;
        MarkAsCreated(createdBy);
        UserEmail = userEmail;
    }

    public static Result<SuccessStory> Create(Guid companyId, string title, string description,
        int successStoryTypeId, string requestId, DateTime startDate, DateTime endDate, int collaborationStatusId,
        Guid termsAndConditionId, Guid createdBy, Guid? sectorId , string? sectorName, string UserEmail)
    {
        if (companyId == Guid.Empty)
            return Result<SuccessStory>.Failure("Company ID is required");

        var titleResult = Title.Create(title);
        if (titleResult.IsFailure)
            return Result<SuccessStory>.Failure(titleResult.Error!);

        var descriptionResult = Description.Create(description);
        if (descriptionResult.IsFailure)
            return Result<SuccessStory>.Failure(descriptionResult.Error!);

        if (startDate > endDate)
            return Result<SuccessStory>.Failure("End date must be after start date");

        if (!string.IsNullOrWhiteSpace(UserEmail) && !EmailRegex.IsMatch(UserEmail))
            return Result<SuccessStory>.Failure("Invalid email format");



        //if (termsAndConditionId == Guid.Empty)
        //    return Result<SuccessStory>.Failure("Terms and condition is required");

        var story = new SuccessStory(companyId, titleResult.Value!, descriptionResult.Value!, successStoryTypeId,requestId,
            startDate, endDate, collaborationStatusId, termsAndConditionId, createdBy, sectorId, sectorName, UserEmail);
       
        return Result<SuccessStory>.Success(story);
    }
    public Result<bool> AddCollaboratedCompanies(List<Guid> companyIds)
    {
        if (companyIds == null || companyIds.Count == 0)
            return Result<bool>.Success(true);
        //return Result<bool>.Failure("At least one collaboration company is required");

        try
        {
            foreach (var companyId in companyIds)
            {
                if (!_collaboratedCompanies.Any(c => c.SynergyCompanyId == companyId))
                {
                    var successStorySynergyCompany = new SuccessStorySynergyCompany(Id, companyId);
                    _collaboratedCompanies.Add(successStorySynergyCompany);
                }
            }
            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
    public Result<bool> AddAssociatedOpportunities(List<Guid> opportunityIds)
    {

        if(opportunityIds != null && opportunityIds.Count > 0)
        {
            try
            {
                foreach (var opportunityId in opportunityIds)
                {
                    if (!_associatedOpportunities.Any(c => c.OpportunityId == opportunityId))
                    {
                        var associatedOpportunitiy = new SuccessStoryOpportunity(Id, opportunityId);
                        _associatedOpportunities.Add(associatedOpportunitiy);
                    }
                }
                return Result<bool>.Success(true);
            }
            catch (ArgumentException ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }
        return Result<bool>.Success(true);

    }
    public Result<bool> AcceptTermsAndConditions(Guid userId)
    {
        if (TermsAccepted)
            return Result<bool>.Failure("Terms and conditions already accepted");

        TermsAccepted = true;
        TermsAcceptedAt = DateTime.UtcNow;
        TermsAcceptedBy = userId;
        MarkAsUpdated(userId);

        return Result<bool>.Success(true);
    }

    public Result<bool> AddAttachment(string fileName, string sharePointUrl, long fileSizeInBytes,
        string? uploadedBy = null)
    {
        try
        {
            var attachment = new SuccessStoryAttachment(Id, fileName, sharePointUrl, fileSizeInBytes, uploadedBy);
            _attachments.Add(attachment);
            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public Result<bool> RemoveAttachment(Guid attachmentId)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment == null)
            return Result<bool>.Failure("Attachment not found");

        _attachments.Remove(attachment);
        return Result<bool>.Success(true);
    }

    public Result<bool> Submit(Guid userId)
    {
        //if (Status != SuccessStoryStatus.PendingReview)
        //    return Result<bool>.Failure("Only draft stories can be submitted");

        //if (!TermsAccepted)
        //    return Result<bool>.Failure("Terms and conditions must be accepted before submission");

        //if (_attachments.Count == 0)
        //    return Result<bool>.Failure("At least one attachment is required");

        Status = SuccessStoryStatus.PendingReview;
        MarkAsUpdated(userId);
        
        // Raise domain event for audit trail and notifications
        AddDomainEvent(new SuccessStorySubmittedEvent(Id, CompanyId, userId));

        return Result<bool>.Success(true);
    }

    public Result<bool> Approve(Guid userId)
    {
        if (Status != SuccessStoryStatus.PendingReview)
            return Result<bool>.Failure("Only pending stories can be approved");

        Status = SuccessStoryStatus.AssetManagerApproved;
        ApprovedBy = userId;
        ApprovedAt = DateTime.UtcNow;
        MarkAsUpdated(userId);
        
        AddDomainEvent(new SuccessStoryApprovedEvent(Id, CompanyId, Status, userId));

        return Result<bool>.Success(true);
    }

    public Result<bool> RejectByAssetManager(Guid userId, string rejectionReason)
    {
        if (Status != SuccessStoryStatus.PendingReview)
            return Result<bool>.Failure("Only pending stories can be rejected");

        if (string.IsNullOrWhiteSpace(rejectionReason))
            return Result<bool>.Failure("Rejection reason is required");

        Status = SuccessStoryStatus.AssetManagerRejected;
        RejectionReason = rejectionReason.Trim();
        RejectedBy = userId;
        RejectedAt = DateTime.UtcNow;
        MarkAsUpdated(userId);
        
        AddDomainEvent(new SuccessStoryRejectedEvent(Id, CompanyId, Status, rejectionReason, userId));

        return Result<bool>.Success(true);
    }
    
    public Result<bool> RejectByAdmin(Guid userId, string rejectionReason)
    {
        if (Status != SuccessStoryStatus.AssetManagerApproved)
            return Result<bool>.Failure("Only asset manager approved stories can be rejected");

        if (string.IsNullOrWhiteSpace(rejectionReason))
            return Result<bool>.Failure("Rejection reason is required");

        Status = SuccessStoryStatus.AdminRejected;
        RejectionReason = rejectionReason.Trim();
        RejectedBy = userId;
        RejectedAt = DateTime.UtcNow;
        MarkAsUpdated(userId);
        
        AddDomainEvent(new SuccessStoryRejectedEvent(Id, CompanyId, Status, rejectionReason, userId));

        return Result<bool>.Success(true);
    }
    
    public Result<bool> Publish(Guid userId)
    {
        if (Status != SuccessStoryStatus.AssetManagerApproved)
            return Result<bool>.Failure("Only approved stories can be published");

        Status = SuccessStoryStatus.Published;
        MarkAsUpdated(userId);
        
        AddDomainEvent(new SuccessStoryApprovedEvent(Id, CompanyId, Status, userId));

        return Result<bool>.Success(true);
    }
    private static readonly Regex EmailRegex = new(
       @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
       RegexOptions.Compiled | RegexOptions.IgnoreCase);
    //public bool CanBeEditedBy(Guid userId)
    //{
    //    return Status == SuccessStoryStatus.Draft && CreatedBy == userId;
    //}
}
