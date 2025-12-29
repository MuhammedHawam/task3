using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Domain.Events;

public class SuccessStoryCreatedEvent : DomainEvent
{
    public Guid SuccessStoryId { get; }
    public Guid CompanyId { get; }
    public string Title { get; }
    public Guid CreatedBy { get; }

    public SuccessStoryCreatedEvent(Guid successStoryId, Guid companyId, string title, Guid createdBy)
    {
        SuccessStoryId = successStoryId;
        CompanyId = companyId;
        Title = title;
        CreatedBy = createdBy;
    }
}

public class SuccessStorySubmittedEvent : DomainEvent
{
    public Guid SuccessStoryId { get; }
    public Guid CompanyId { get; }
    public Guid SubmittedBy { get; }

    public SuccessStorySubmittedEvent(Guid successStoryId, Guid companyId, Guid submittedBy)
    {
        SuccessStoryId = successStoryId;
        CompanyId = companyId;
        SubmittedBy = submittedBy;
    }
}

public class SuccessStoryApprovedEvent : DomainEvent
{
    public Guid SuccessStoryId { get; }
    public Guid CompanyId { get; }
    public SuccessStoryStatus NewStatus { get; }
    public Guid ApprovedBy { get; }

    public SuccessStoryApprovedEvent(Guid successStoryId, Guid companyId, SuccessStoryStatus newStatus, Guid approvedBy)
    {
        SuccessStoryId = successStoryId;
        CompanyId = companyId;
        NewStatus = newStatus;
        ApprovedBy = approvedBy;
    }
}

public class SuccessStoryRejectedEvent : DomainEvent
{
    public Guid SuccessStoryId { get; }
    public Guid CompanyId { get; }
    public SuccessStoryStatus NewStatus { get; }
    public string RejectionReason { get; }
    public Guid RejectedBy { get; }

    public SuccessStoryRejectedEvent(Guid successStoryId, Guid companyId, SuccessStoryStatus newStatus, string rejectionReason, Guid rejectedBy)
    {
        SuccessStoryId = successStoryId;
        CompanyId = companyId;
        NewStatus = newStatus;
        RejectionReason = rejectionReason;
        RejectedBy = rejectedBy;
    }
}

public class SuccessStoryPublishedEvent : DomainEvent
{
    public Guid SuccessStoryId { get; }
    public Guid CompanyId { get; }
    public Guid PublishedBy { get; }

    public SuccessStoryPublishedEvent(Guid successStoryId, Guid companyId, Guid publishedBy)
    {
        SuccessStoryId = successStoryId;
        CompanyId = companyId;
        PublishedBy = publishedBy;
    }
}
