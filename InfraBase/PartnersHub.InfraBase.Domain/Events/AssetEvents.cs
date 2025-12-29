using PartnersHub.InfraBase.Domain.Common;

namespace PartnersHub.InfraBase.Domain.Events;

public class AssetSubmittedEvent : DomainEvent
{
    public Guid AssetId { get; }
    public string AssetCode { get; }
    public string SubmittedBy { get; }

    public AssetSubmittedEvent(Guid assetId, string assetCode, string submittedBy)
    {
        AssetId = assetId;
        AssetCode = assetCode;
        SubmittedBy = submittedBy;
    }
}

public class AssetRejectedByPcAdminEvent : DomainEvent
{
    public Guid AssetId { get; }
    public string AssetCode { get; }
    public string RejectionReason { get; }
    public string RejectedBy { get; }

    public AssetRejectedByPcAdminEvent(Guid assetId, string assetCode, 
        string rejectionReason, string rejectedBy)
    {
        AssetId = assetId;
        AssetCode = assetCode;
        RejectionReason = rejectionReason;
        RejectedBy = rejectedBy;
    }
}

public class AssetAcceptedByPcAdminEvent : DomainEvent
{
    public Guid AssetId { get; }
    public string AssetCode { get; }
    public string AcceptedBy { get; }

    public AssetAcceptedByPcAdminEvent(Guid assetId, string assetCode, string acceptedBy)
    {
        AssetId = assetId;
        AssetCode = assetCode;
        AcceptedBy = acceptedBy;
    }
}

public class AssetCheckedByInfrabaseAdminEvent : DomainEvent
{
    public Guid AssetId { get; }
    public string AssetCode { get; }
    public string CheckedBy { get; }

    public AssetCheckedByInfrabaseAdminEvent(Guid assetId, string assetCode, string checkedBy)
    {
        AssetId = assetId;
        AssetCode = assetCode;
        CheckedBy = checkedBy;
    }
}

public class AssetReturnedForCorrectionByInfrabaseAdminEvent : DomainEvent
{
    public Guid AssetId { get; }
    public string AssetCode { get; }
    public string CorrectionReason { get; }
    public string ReturnedBy { get; }

    public AssetReturnedForCorrectionByInfrabaseAdminEvent(Guid assetId, string assetCode, 
        string correctionReason, string returnedBy)
    {
        AssetId = assetId;
        AssetCode = assetCode;
        CorrectionReason = correctionReason;
        ReturnedBy = returnedBy;
    }
}
