using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;
using PartnersHub.Synergy.Domain.Aggregates.Synergy.Lookups;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.Events;
using PartnersHub.Synergy.Domain.ValueObjects;
using System.Collections.Generic;
using System.Net.Mail;
using OpportunityStatus = PartnersHub.Synergy.Domain.Common.OpportunityStatus;

namespace PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate;

public class Opportunity : AggregateRoot
{
    private readonly List<OpportunityExpectedOutcome> _expectedOutcomes = new List<OpportunityExpectedOutcome>();
    private readonly List<OpportunityCollaborationRequirement> _collaborationRequirements = new List<OpportunityCollaborationRequirement>();
    private readonly List<OpportunitySynergyCompany> _collaboratedCompanies = new List<OpportunitySynergyCompany>();
    private readonly List<SuccessStoryOpportunity> _successStoryOpportunities = new List<SuccessStoryOpportunity>();
    private readonly List<OpportunityAttachment> _attachments = new List<OpportunityAttachment>();
    //ToDo : Convert RequestId from primitive type to value object
    public string RequestId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string? CompanyName { get; private set; }
    public Title Title { get; private set; } = null!;
    public Description Description { get; private set; } = null!;
    public int OpportunityTypeId { get; private set; }
    public OpportunityType? OpportunityType { get; private set; }
    public int ThematicAreaId { get; private set; }
    public ThematicArea? ThematicArea { get; private set; }
    public Sector Sector { get; private set; }
    public IReadOnlyCollection<OpportunitySynergyCompany> CollaboratedCompanies => _collaboratedCompanies.AsReadOnly();
    public IReadOnlyCollection<OpportunityAttachment> Attachments => _attachments.AsReadOnly();
    public IReadOnlyCollection<SuccessStoryOpportunity> SuccessStoryOpportunities => _successStoryOpportunities.AsReadOnly();
    public string? CollaborationRationale { get; private set; }
    public IReadOnlyCollection<OpportunityCollaborationRequirement> CollaborationRequirements => _collaborationRequirements.AsReadOnly();
    public string? CollaborationRequirementOther { get; private set; }
    public IReadOnlyCollection<OpportunityExpectedOutcome> ExpectedOutcomes => _expectedOutcomes.AsReadOnly();
    public string? ExpectedOutcomeOther { get; private set; }
    public RepresentativeInformation RepresentativeInformation { get; set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public Guid TermsAndConditionId { get; private set; }
    public bool TermsAccepted { get; private set; }
    public DateTime? TermsAcceptedAt { get; private set; }
    public Guid? TermsAcceptedBy { get; private set; }

    public OpportunityStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    //TODO: change type to string to allow storing adfs user id
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? RejectedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }

    public bool? IsAdminCreated { get; private set; }   

    public string? ContactName { get; private set; }

    public string? ContactAddress { get; private set; }

    public string? ContactMobile { get; private set; }

    public string? UserEmail { get; private set; }

    public bool IsHide {  get; private set; }  = false;

    public bool? IsAdminUpdated { get; private set; }

    private Opportunity() { }

    private Opportunity(Guid companyId, Title title, Description description, Sector sector, RepresentativeInformation representativeInfo,
        string collaborationRationale, int opportunityTypeId, int thematicAreaId, string requestId, DateOnly startDate, DateOnly endDate,
        Guid termsAndConditionId, Guid createdBy, bool? isAdminCreated, string? contactName, string? contactAddress, string? contactMobile, string? companyName, string? userEmail)
    {
        CompanyId = companyId;
        CompanyName = companyName;
        Title = title;
        Description = description;
        Sector = sector;
        RepresentativeInformation = representativeInfo;
        OpportunityTypeId = opportunityTypeId;
        ThematicAreaId = thematicAreaId;
        StartDate = startDate;
        EndDate = endDate;
        TermsAndConditionId = termsAndConditionId;
        TermsAccepted = true;
        Status = isAdminCreated == true ? OpportunityStatus.Published : OpportunityStatus.PendingReview;
        CollaborationRationale = collaborationRationale;
        RequestId = requestId;
        IsAdminCreated = isAdminCreated;
        ContactName = contactName;
        ContactAddress = contactAddress;
        ContactMobile = contactMobile;
        CompanyName = companyName;
        UserEmail = userEmail;
        MarkAsCreated(createdBy);
    }

    public static Result<Opportunity> Create(Guid companyId, string title, string description,
        string sectorName,
        Guid sectorId,
        int opportunityTypeId,
        int thematicAreaId,
        string representativeEmail,
        string representativeName,
        string representativePhone,
        string representativePosition,
        string collaborationRationale,
        string requestId,
        DateOnly startDate,
        DateOnly endDate,
        Guid termsAndConditionId,
        Guid createdBy,
        bool? IsAdmin,
        string? ContactName,
        string? ContactAddress,
        string? ContactMobile,
        string? CompanyName,
        string? UserEmail)
    {
        if (companyId == Guid.Empty)
            return Result<Opportunity>.Failure("Company ID is required");

        var titleResult = Title.Create(title);
        if (titleResult.IsFailure)
            return Result<Opportunity>.Failure(titleResult.Error!);

        var descriptionResult = Description.Create(description);
        if (descriptionResult.IsFailure)
            return Result<Opportunity>.Failure(descriptionResult.Error!);


        var sectorResult = Sector.Create(sectorName, sectorId);
        if (sectorResult.IsFailure)
            return Result<Opportunity>.Failure(sectorResult.Error!);

        var representativeInfo = RepresentativeInformation.Create(representativeName, representativePosition, representativeEmail, representativePhone);
        if (representativeInfo.IsFailure)
            return Result<Opportunity>.Failure(representativeInfo.Error!);

        if (startDate >= endDate)
            return Result<Opportunity>.Failure("End date must be after start date");


        //if (termsAndConditionId == Guid.Empty)
        //    return Result<Opportunity>.Failure("Terms and condition is required");

        var opportunity = new Opportunity(companyId, titleResult.Value!, descriptionResult.Value!, sectorResult.Value, representativeInfo.Value,
            collaborationRationale, opportunityTypeId, thematicAreaId, requestId, startDate, endDate,
            termsAndConditionId, createdBy,IsAdmin, ContactName, ContactAddress, ContactMobile,CompanyName, UserEmail);

        return Result<Opportunity>.Success(opportunity);
    }

    public Result AcceptTermsAndConditions(Guid userId)
    {
        if (TermsAccepted)
            return Result.Failure("Terms and conditions already accepted");

        TermsAccepted = true;
        TermsAcceptedAt = DateTime.UtcNow;
        TermsAcceptedBy = userId;
        MarkAsUpdated(userId);

        return Result.Success();
    }

    public Result AddCollaboratedCompanies(List<Guid> companyIds)
    {
        if (companyIds == null || companyIds.Count == 0)
            return Result.Success();
            //return Result.Failure("At least one collaboration company is required");

        try
        {
            foreach (var companyId in companyIds)
            {
                if (!_collaboratedCompanies.Any(c => c.SynergyCompanyId == companyId))
                {
                    var collaboration = new OpportunitySynergyCompany(Id, companyId);
                    _collaboratedCompanies.Add(collaboration);
                }
            }
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public Result RemoveCollaboratedCompany(Guid companyId)
    {
        var collaboration = _collaboratedCompanies.FirstOrDefault(c => c.SynergyCompanyId == companyId);
        if (collaboration == null)
            return Result.Failure("Collaborated company not found");

        _collaboratedCompanies.Remove(collaboration);
        return Result.Success();
    }

    public Result AddAttachment(string fileName, string sharePointUrl, long fileSizeInBytes,
        string? uploadedBy = null)
    {
        try
        {
            var attachment = new OpportunityAttachment(Id, fileName, sharePointUrl, fileSizeInBytes, uploadedBy);
            _attachments.Add(attachment);
            return Result.Success();
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public Result RemoveAttachment(Guid attachmentId)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment == null)
            return Result.Failure("Attachment not found");

        _attachments.Remove(attachment);
        return Result.Success();
    }


    public Result AddCollaborationRequirement(
    List<CollaborationRequirement>? collaborationRequirements,
    string? otherCollaborationRequirement = null)
    {
        // collaborationRequirements is optional
        if (collaborationRequirements == null || !collaborationRequirements.Any())
        {
            // nothing to add, return success
            return Result.Success();
        }

        // Check if 'Other' exists in the list
        bool hasOther = collaborationRequirements
            .Any(cr => cr.Name.Contains(Constants.Other, StringComparison.OrdinalIgnoreCase));

        if (hasOther)
        {
            if (string.IsNullOrWhiteSpace(otherCollaborationRequirement))
            {
                return Result.Failure("Other collaboration requirement field is required.");
            }

            CollaborationRequirementOther = otherCollaborationRequirement;
        }

        foreach (var collaborationRequirement in collaborationRequirements)
        {
            var item = new OpportunityCollaborationRequirement(Id, collaborationRequirement.Id);
            _collaborationRequirements.Add(item);
        }

        return Result.Success();
    }

   public Result AddExpectedOutcome(List<ExpectedOutcome>? expectedOutcomes = null, string otherExpectedOutcome = "")
    {
        // If nothing provided at all simply return success (because both are optional now)
        if ((expectedOutcomes == null || expectedOutcomes.Count == 0)
        && string.IsNullOrWhiteSpace(otherExpectedOutcome))
        {
            return Result.Success();
        }

        // If "Other" is selected  validate the additional field
        if (expectedOutcomes != null &&
        expectedOutcomes.Any(eo => eo.Name.Contains(Constants.Other)))
        {
            if (string.IsNullOrWhiteSpace(otherExpectedOutcome))
                return Result.Failure("Other expected outcome field is required");

            ExpectedOutcomeOther = otherExpectedOutcome;
        }

        if (expectedOutcomes != null)
        {
            foreach (var expectedOutcome in expectedOutcomes)
            {
                var opportunityExpectedOutcome =
                new OpportunityExpectedOutcome(Id, expectedOutcome.Id);

                _expectedOutcomes.Add(opportunityExpectedOutcome);
            }
        }

        return Result.Success();
    }


    public Result SetOpportunityType(OpportunityType opportunityType)
    {
        if (opportunityType == null)
            return Result.Failure("Opportunity type is invalid");
        OpportunityType = opportunityType;
        return Result.Success();
    }

    public Result SetThematicArea(ThematicArea thematicArea)
    {
        if (thematicArea == null)
            return Result.Failure("thematic area is invalid");
        ThematicArea = thematicArea;
        return Result.Success();
    }

    public Result SetCollaborationRationale(string rationale)
    {
        if (string.IsNullOrWhiteSpace(rationale))
            return Result.Failure("Collaboration rationale is required");

        CollaborationRationale = rationale.Trim();
        return Result.Success();
    }

    public Result Submit(Guid userId, string opportunityName, string? companyName)
    {
        //if (Status != OpportunityStatus.Draft)
        //    return Result.Failure("Only draft opportunities can be submitted");

        if (!TermsAccepted)
            return Result.Failure("Terms and conditions must be accepted before submission");

        //if (_collaboratedCompanies.Count == 0)
        //    return Result.Failure("At least one collaboration company is required");

        //if (_collaborationRequirements.Count == 0)
        //    return Result.Failure("At least one collaboration requirement is required");

        //if (_expectedOutcomes.Count == 0)
        //    return Result.Failure("At least one expected outcome is required");

        Status = OpportunityStatus.PendingReview;
        MarkAsUpdated(userId);
        AddDomainEvent(new OpportunitySubmittedEvent(Id, CompanyId, userId, opportunityName, companyName));

        return Result.Success();
    }

    public Result<bool> Publish(Guid userId, string opportunityName, string? companyName, string? companyEmail, bool isAdmin = false)
    {
        if (Status != OpportunityStatus.Pending && !isAdmin)
            return Result<bool>.Failure("Only pending opportunities can be approved");

        Status = OpportunityStatus.Published;
        ApprovedBy = userId;
        ApprovedAt = DateTime.UtcNow;
        MarkAsUpdated(userId);
        AddDomainEvent(new OpportunityApprovedEvent(Id, CompanyId, Status, userId, opportunityName, companyName, companyEmail));

        return Result<bool>.Success(true);
    }

    public Result<bool> ApproveByAssetManager(Guid userId, string opportunityName, string? companyName, string? companyEmail)
    {
        if (Status != OpportunityStatus.PendingReview)
            return Result<bool>.Failure("Opportunity must be pending approval");

        Status = OpportunityStatus.Pending;
        ApprovedBy = userId;
        ApprovedAt = DateTime.UtcNow;
        MarkAsUpdated(userId);
        AddDomainEvent(new OpportunityApprovedEvent(Id, CompanyId, Status, userId, opportunityName, companyName, companyEmail));

        return Result<bool>.Success(true);
    }

    public Result<bool> RejectByAdmin(Guid userId, string rejectionReason, string opportunityName, string? companyName,string? companyEmail)
    {
        if (Status != OpportunityStatus.Pending)
            return Result<bool>.Failure("Only approved opportunities can be rejected");

        if (string.IsNullOrWhiteSpace(rejectionReason))
            return Result<bool>.Failure("Rejection reason is required");

        Status = OpportunityStatus.AdminRejected;
        RejectionReason = rejectionReason.Trim();
        RejectedBy = userId;
        RejectedAt = DateTime.UtcNow;
        MarkAsUpdated(userId);
        AddDomainEvent(new OpportunityRejectedEvent(Id, CompanyId, Status, rejectionReason, userId, opportunityName, companyName, companyEmail));

        return Result<bool>.Success(true);
    }

    public Result RejectByAssetManager(Guid userId, string rejectionReason, string opportunityName, string? companyName, string? companyEmail)
    {
        if (Status != OpportunityStatus.PendingReview)
            return Result.Failure("Opportunity must be admin approved before asset manager can reject");

        if (string.IsNullOrWhiteSpace(rejectionReason))
            return Result.Failure("Rejection reason is required");

        Status = OpportunityStatus.AssetManagerRejected;
        RejectionReason = rejectionReason.Trim();
        RejectedBy = userId;
        RejectedAt = DateTime.UtcNow;
        MarkAsUpdated(userId);
        AddDomainEvent(new OpportunityRejectedEvent(Id, CompanyId, Status, rejectionReason, userId, opportunityName, companyName, companyEmail));

        return Result.Success();
    }




    public Result SetVisibility(bool hide, Guid userId)
    {
        if (IsHide == hide)
            return Result.Failure(IsHide ? "This partnership is already hidden" : "This partnership is already unhidden");

        IsHide = hide;
        MarkAsUpdated(userId);

        return Result.Success();
    }
    public Result Update(
    Guid userId,
    string title,
    string description,
    string sectorName,
    Guid sectorId,
    int opportunityTypeId,
    int thematicAreaId,
    string collaborationRationale,
    DateOnly startDate,
    DateOnly endDate,
    string? contactName,
    string? contactAddress,
    string? contactMobile,
    bool? IsAdmin)
    {


        if (Status != OpportunityStatus.Published )
            return Result.Failure("Opportunity cannot be edited in the current status");


        var titleResult = Title.Create(title);
        if (titleResult.IsFailure)
            return Result.Failure(titleResult.Error!);

        var descriptionResult = Description.Create(description);
        if (descriptionResult.IsFailure)
            return Result.Failure(descriptionResult.Error!);

        var sectorResult = Sector.Create(sectorName, sectorId);
        if (sectorResult.IsFailure)
            return Result.Failure(sectorResult.Error!);

        if (startDate >= endDate)
            return Result.Failure("End date must be after start date");



        Title = titleResult.Value!;
        Description = descriptionResult.Value!;
        Sector = sectorResult.Value;
        OpportunityTypeId = opportunityTypeId;
        ThematicAreaId = thematicAreaId;
        CollaborationRationale = collaborationRationale.Trim();
        StartDate = startDate;
        EndDate = endDate;

        ContactName = contactName;
        ContactAddress = contactAddress;
        ContactMobile = contactMobile;
        IsAdminUpdated = IsAdmin;

        MarkAsUpdated(userId);
        AddDomainEvent(new OpportunityUpdatedEvent(Id, CompanyId, Status, Title.Value, CompanyName, UserEmail));


        return Result.Success();
    }

    public Result ReplaceCollaboratedCompanies(List<Guid>? companyIds, Guid userId)
    {
        companyIds ??= new List<Guid>();

        _collaboratedCompanies.RemoveAll(x => !companyIds.Contains(x.SynergyCompanyId));

        foreach (var companyId in companyIds)
        {
            if (!_collaboratedCompanies.Any(x => x.SynergyCompanyId == companyId))
            {
                _collaboratedCompanies.Add(new OpportunitySynergyCompany(Id, companyId));
            }
        }

        MarkAsUpdated(userId);
        return Result.Success();
    }

    public Result ReplaceCollaborationRequirements(
        List<CollaborationRequirement>? collaborationRequirements,
        string? otherCollaborationRequirement,
        Guid userId)
    {
        collaborationRequirements ??= new List<CollaborationRequirement>();

        _collaborationRequirements.Clear();

        if (!collaborationRequirements.Any())
        {
            MarkAsUpdated(userId);
            return Result.Success();
        }

        var hasOther = collaborationRequirements
            .Any(cr => cr.Name.Contains(Constants.Other, StringComparison.OrdinalIgnoreCase));

        if (hasOther)
        {
            if (string.IsNullOrWhiteSpace(otherCollaborationRequirement))
                return Result.Failure("Other collaboration requirement field is required.");

            CollaborationRequirementOther = otherCollaborationRequirement;
        }
        else
        {
            CollaborationRequirementOther = null;
        }
        
        foreach (var cr in collaborationRequirements
            .GroupBy(x => x.Id)
            .Select(g => g.First()))
        {

            _collaborationRequirements.Add(
                new OpportunityCollaborationRequirement(Id, cr.Id)
            );
        }

        MarkAsUpdated(userId);
        return Result.Success();
    }



    public Result ReplaceExpectedOutcomes(
        List<ExpectedOutcome>? expectedOutcomes,
        string? otherExpectedOutcome,
        Guid userId)
    {
        expectedOutcomes ??= new List<ExpectedOutcome>();

        _expectedOutcomes.Clear();

        var distinctExpectedOutcomes = expectedOutcomes
            .GroupBy(eo => eo.Id)
            .Select(g => g.First())
            .ToList();

        if (!distinctExpectedOutcomes.Any() && string.IsNullOrWhiteSpace(otherExpectedOutcome))
        {
            MarkAsUpdated(userId);
            return Result.Success();
        }

        var hasOther = distinctExpectedOutcomes
            .Any(eo => eo.Name.Contains(Constants.Other, StringComparison.OrdinalIgnoreCase));

        if (hasOther)
        {
            if (string.IsNullOrWhiteSpace(otherExpectedOutcome))
                return Result.Failure("Other expected outcome field is required");

            ExpectedOutcomeOther = otherExpectedOutcome;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(otherExpectedOutcome))
                return Result.Failure("Other expected outcome is not allowed unless 'Other' is selected");
        }

        
        foreach (var eo in distinctExpectedOutcomes)
        {

            _expectedOutcomes.Add(
                new OpportunityExpectedOutcome(Id, eo.Id)
            );
        }

        MarkAsUpdated(userId);
        return Result.Success();
    }




}
