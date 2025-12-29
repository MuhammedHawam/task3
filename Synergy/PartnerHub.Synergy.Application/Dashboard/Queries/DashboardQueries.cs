using MediatR;
using PartnersHub.Synergy.Application.Dashboard.DTOs;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.Dashboard.Queries;

/// <summary>
/// Get complete dashboard home page data (KPIs + recent items) in one query
/// </summary>
public class GetDashboardHomeQuery : IRequest<Result<DashboardHomeDto>>
{
    public Guid CompanyId { get; set; }
    public int? Year { get; set; } // Optional: filter by year (default: current year for YTD)
}

/// <summary>
/// Get user's submissions (opportunities and success stories) with filtering and pagination
/// </summary>
public class GetUserSubmissionsQuery : IRequest<Result<PaginatedList<UserOpportunitySubmissionDto>>>
{
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// Filter by status. Supports comma-separated values: pending, published, returned, rejected, draft
    /// Example: "pending,published" or "returned"
    /// </summary>
    public string? Status { get; set; }
    
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Get user's success story submissions with filtering and pagination
/// </summary>
public class GetUserSuccessStoriesQuery : IRequest<Result<PaginatedList<UserSuccessStorySubmissionDto>>>
{
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// Filter by status. Supports comma-separated values: pending, published, returned, rejected, draft
    /// Example: "pending,published" or "returned"
    /// </summary>
    public string? Status { get; set; }
    
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetAdminDashboardQuery : IRequest<Result<AdminDashboardKPIsDto>>
{
    public int? Year { get; set; }
}