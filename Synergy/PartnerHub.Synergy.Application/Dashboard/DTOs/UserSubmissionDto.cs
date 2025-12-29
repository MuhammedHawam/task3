using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.Dashboard.DTOs;

/// <summary>
/// User's opportunity submission for dashboard
/// </summary>
public class UserOpportunitySubmissionDto
{
    public Guid Id { get; set; }
    public string RequestId { get; set; }
    public string Title { get; set; } = null!;
    public DateTime SubmissionDate { get; set; }
    public string CollaborationType { get; set; } = null!;
    public string Sector { get; set; } = null!;
    public OpportunityStatus Status { get; set; }
    public string StatusDescription { get; set; } = null!;
}

/// <summary>
/// User's success story submission for dashboard
/// </summary>
public class UserSuccessStorySubmissionDto
{
    public Guid Id { get; set; }
    public string RequestId { get; set; }
    public string Title { get; set; } = null!;
    public DateTime SubmissionDate { get; set; }
    public string Type { get; set; } = null!; // Partnership / Collaboration / Joint Venture
    public string SubmissionStatus => Status.ToString();
    public SuccessStoryStatus Status { get; set; }
    public string StatusDescription { get; set; } = null!;
}
