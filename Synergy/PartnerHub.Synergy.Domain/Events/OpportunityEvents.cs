using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Domain.Events;

public class OpportunityCreatedEvent : DomainEvent
{
    public Guid OpportunityId { get; }
    public Guid CompanyId { get; }
    public string Title { get; }
    public Guid CreatedBy { get; }

    public OpportunityCreatedEvent(Guid opportunityId, Guid companyId, string title, Guid createdBy)
    {
        OpportunityId = opportunityId;
        CompanyId = companyId;
        Title = title;
        CreatedBy = createdBy;
    }
}

public class OpportunitySubmittedEvent : DomainEvent
{
    public Guid OpportunityId { get; }
    public Guid CompanyId { get; }
    public Guid SubmittedBy { get; }

    public string OpportunityName { get; }

    public string? CompanyName { get; }

    public OpportunitySubmittedEvent(Guid opportunityId, Guid companyId, Guid submittedBy, string opportunityName, string? companyName)
    {
        OpportunityId = opportunityId;
        CompanyId = companyId;
        SubmittedBy = submittedBy;
        OpportunityName = opportunityName;
        CompanyName = companyName;
    }
}

public class OpportunityApprovedEvent : DomainEvent
{
    public Guid OpportunityId { get; }
    public Guid CompanyId { get; }
    public OpportunityStatus NewStatus { get; }
    public Guid ApprovedBy { get; }
    public string OpportunityName { get; }

    public string? CompanyName { get; }

    public string? CompanyEmail { get; }

    public OpportunityApprovedEvent(Guid opportunityId, Guid companyId, OpportunityStatus newStatus, Guid approvedBy, string opportunityName, string? companyName, string? companyEmail)
    {
        OpportunityId = opportunityId;
        CompanyId = companyId;
        NewStatus = newStatus;
        ApprovedBy = approvedBy;
        OpportunityName = opportunityName;
        CompanyName = companyName;
        CompanyEmail = companyEmail;
    }
}

public class OpportunityRejectedEvent : DomainEvent
{
    public Guid OpportunityId { get; }
    public Guid CompanyId { get; }
    public OpportunityStatus NewStatus { get; }
    public string RejectionReason { get; }
    public Guid RejectedBy { get; }

    public string OpportunityName { get; }

    public string? CompanyName { get; }
    public string? CompanyEmail { get; }

    public OpportunityRejectedEvent(Guid opportunityId, Guid companyId, OpportunityStatus newStatus, string rejectionReason, Guid rejectedBy, string opportunityName, string? companyName, string? companyEmail)
    {
        OpportunityId = opportunityId;
        CompanyId = companyId;
        NewStatus = newStatus;
        RejectionReason = rejectionReason;
        RejectedBy = rejectedBy;
        OpportunityName = opportunityName;
        CompanyName = companyName;
        CompanyEmail = companyEmail;
    }
}

public class OpportunityPublishedEvent : DomainEvent
{
    public Guid OpportunityId { get; }
    public Guid CompanyId { get; }
    public Guid PublishedBy { get; }

    public OpportunityPublishedEvent(Guid opportunityId, Guid companyId, Guid publishedBy)
    {
        OpportunityId = opportunityId;
        CompanyId = companyId;
        PublishedBy = publishedBy;
    }
}


public class OpportunityUpdatedEvent : DomainEvent
{
    public Guid OpportunityId { get; }
    public Guid CompanyId { get; }
    public OpportunityStatus NewStatus { get; }

    public Guid UpdatedBy { get; }

    public string OpportunityName { get; }

    public string CompanyName { get; }
    public string CompanyEmail { get; }

    public OpportunityUpdatedEvent(Guid opportunityId, Guid companyId, OpportunityStatus newStatus,  string opportunityName, string companyName, string companyEmail)
    {
        OpportunityId = opportunityId;
        CompanyId = companyId;
        NewStatus = newStatus;
        OpportunityName = opportunityName;
        CompanyName = companyName;
        CompanyEmail = companyEmail;
    }
}
