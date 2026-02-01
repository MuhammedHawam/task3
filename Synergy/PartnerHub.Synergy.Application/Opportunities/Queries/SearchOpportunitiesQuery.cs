using MediatR;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.Opportunities.DTOs;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.Opportunities.Queries;

/// <summary>
/// Query for searching opportunities with pagination, search, and filtering
/// </summary>
public record SearchOpportunitiesQuery : IRequest<Result<PaginatedList<OpportunitySearchCardDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public string? SearchTerm { get; init; }
    
    /// <summary>
    /// Filter by company IDs that created the opportunities
    /// </summary>
    public List<Guid>? CompanyIds { get; init; }
    
    public List<Guid>? SectorIds { get; init; }
    public List<int>? OpportunityTypeIds { get; init; }
    public List<int>? ThematicAreaIds { get; init; }
    public List<int>? CollaborationRequirementIds { get; init; }
    public List<int>? ExpectedOutcomeIds { get; init; }
    public List<int>? Statuses { get; init; }
    public List<int>? CollaborationStatuses { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? SortBy { get; init; } = "CreatedAt";

    public bool? IncludeIsHide { get; init; } = true;
}
