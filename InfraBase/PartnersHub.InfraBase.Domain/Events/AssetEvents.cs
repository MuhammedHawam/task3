using PartnersHub.InfraBase.Domain.Common;

namespace PartnersHub.InfraBase.Domain.Events;

public class AssetSubmittedEvent : DomainEvent
{
    public Guid AssetId { get; }
    public string AssetCode { get; }
    public string SubmittedBy { get; }
    public Guid CompanyId { get; }
    public string CreatedBy { get; }
    public bool IsContributorSubmission { get; }

    public AssetSubmittedEvent(Guid assetId, string assetCode, string submittedBy, Guid companyId, string createdBy, bool isContributorSubmission)
    {
        AssetId = assetId;
        AssetCode = assetCode;
        SubmittedBy = submittedBy;
        CompanyId = companyId;
        CreatedBy = createdBy;
        IsContributorSubmission = isContributorSubmission;
    }
}

public class AssetRejectedByPcAdminEvent : DomainEvent
{
    public Guid AssetId { get; }
    public string AssetCode { get; }
    public string RejectionReason { get; }
    public string RejectedBy { get; }
    public string CreatedBy { get; }

    public AssetRejectedByPcAdminEvent(Guid assetId, string assetCode, 
        string rejectionReason, string rejectedBy, string createdBy)
    {
        AssetId = assetId;
        AssetCode = assetCode;
        RejectionReason = rejectionReason;
        RejectedBy = rejectedBy;
        CreatedBy = createdBy;
    }
}

public class AssetAcceptedByPcAdminEvent : DomainEvent
{
    public Guid AssetId { get; }
    public string AssetCode { get; }
    public string AcceptedBy { get; }
    public string CreatedBy { get; }
    public Guid CompanyId { get; }

    public AssetAcceptedByPcAdminEvent(Guid assetId, string assetCode, string acceptedBy, string createdBy, Guid companyId)
    {
        AssetId = assetId;
        AssetCode = assetCode;
        AcceptedBy = acceptedBy;
        CreatedBy = createdBy;
        CompanyId = companyId;
    }
}

public class AssetCheckedByInfrabaseAdminEvent : DomainEvent
{
    public Guid AssetId { get; }
    public string AssetCode { get; }
    public string CheckedBy { get; }
    public string CreatedBy { get; }
    public Guid CompanyId { get; }

    public AssetCheckedByInfrabaseAdminEvent(Guid assetId, string assetCode, string checkedBy, string createdBy, Guid companyId)
    {
        AssetId = assetId;
        AssetCode = assetCode;
        CheckedBy = checkedBy;
        CreatedBy = createdBy;
        CompanyId = companyId;
    }
}

public class AssetReturnedForCorrectionByInfrabaseAdminEvent : DomainEvent
{
    public Guid AssetId { get; }
    public string AssetCode { get; }
    public string CorrectionReason { get; }
    public string ReturnedBy { get; }
    public string CreatedBy { get; }
    public Guid CompanyId { get; }

    public AssetReturnedForCorrectionByInfrabaseAdminEvent(Guid assetId, string assetCode, 
        string correctionReason, string returnedBy, string createdBy, Guid companyId)
    {
        AssetId = assetId;
        AssetCode = assetCode;
        CorrectionReason = correctionReason;
        ReturnedBy = returnedBy;
        CreatedBy = createdBy;
        CompanyId = companyId;
    }
}
