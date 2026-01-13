using MediatR;
using PartnersHub.Synergy.Application.Dashboard.DTOs;
using PartnersHub.Synergy.Application.Dashboard.Queries;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.Dashboard.QueryHandlers;

public class GetUserSubmissionsQueryHandler : IRequestHandler<GetUserSubmissionsQuery, Result<PaginatedList<UserOpportunitySubmissionDto>>>
{
    private readonly IOpportunityRepository _opportunityRepository;

    public GetUserSubmissionsQueryHandler(IOpportunityRepository opportunityRepository)
    {
        _opportunityRepository = opportunityRepository;
    }

    public async Task<Result<PaginatedList<UserOpportunitySubmissionDto>>> Handle(GetUserSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var statuses = MapStatusFilter(request.Status);

        // Use unified SearchOpportunitiesAsync method with companyId
        var paginatedResult = await _opportunityRepository.SearchOpportunitiesAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            companyIds: new List<Guid> { request.CompanyId },
            searchTerm: request.SearchTerm,
            sectorIds: null,
            thematicAreaIds: null,
            opportunityTypeIds: null,
            collaborationRequirementIds: null,
            expectedOutcomeIds: null,
            statuses: statuses,
            sortBy: request.SortBy ?? "CreatedAt",
            asNoTracking: true);

        var dtos = paginatedResult.Items.Select(o => new UserOpportunitySubmissionDto
        {
            Id = o.Id,
            RequestId = o.RequestId,
            Title = o.Title.Value,
            SubmissionDate = o.CreatedAt,
            CollaborationType = o.OpportunityType?.Name ?? "N/A",
            Sector = o.Sector?.Value ?? "N/A",
            Status = o.Status,
            StatusDescription = MapStatusToDisplay(o.Status)
        }).ToList();

        var result = new PaginatedList<UserOpportunitySubmissionDto>(
            dtos,
            paginatedResult.TotalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<UserOpportunitySubmissionDto>>.Success(result);
    }

    private static List<OpportunityStatus>? MapStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        // Split by comma to support multiple statuses
        var statusList = status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToLower())
            .ToList();

        if (statusList.Count == 0)
            return null;

        var mappedStatuses = new List<OpportunityStatus>();

        foreach (var statusItem in statusList)
        {
            var mapped = statusItem switch
            {
                "pending" or "pendingreview" => new List<OpportunityStatus>
                {
                    OpportunityStatus.PendingReview,
                    OpportunityStatus.Pending
                },
                "published" => new List<OpportunityStatus> { OpportunityStatus.Published },
                "returned" or "rejected" => new List<OpportunityStatus>
                {
                    OpportunityStatus.AdminRejected,
                    OpportunityStatus.AssetManagerRejected
                },
                _ => null
            };

            if (mapped != null)
            {
                mappedStatuses.AddRange(mapped);
            }
        }

        return mappedStatuses.Count > 0 ? mappedStatuses.Distinct().ToList() : null;
    }

    private static string MapStatusToDisplay(OpportunityStatus status)
    {
        return status switch
        {
            OpportunityStatus.PendingReview => "Pending",
            OpportunityStatus.Pending => "Approved",
            OpportunityStatus.Published => "Published",
            OpportunityStatus.AdminRejected => "Rejected",
            OpportunityStatus.AssetManagerRejected => "Rejected",
            _ => status.ToString()
        };
    }
}

public class GetUserSuccessStoriesQueryHandler : IRequestHandler<GetUserSuccessStoriesQuery, Result<PaginatedList<UserSuccessStorySubmissionDto>>>
{
    private readonly ISuccessStoryRepository _successStoryRepository;

    public GetUserSuccessStoriesQueryHandler(ISuccessStoryRepository successStoryRepository)
    {
        _successStoryRepository = successStoryRepository;
    }

    public async Task<Result<PaginatedList<UserSuccessStorySubmissionDto>>> Handle(GetUserSuccessStoriesQuery request, CancellationToken cancellationToken)
    {
        var statuses = MapStatusFilter(request.Status);

        // If multiple statuses, we need to query separately and combine results
        PaginatedList<SuccessStory> paginatedResult;

        if (statuses != null && statuses.Count > 1)
        {
            // For multiple statuses, we need to get all items and paginate manually
            // since the repository method only accepts a single status
            var allItems = new List<SuccessStory>();
            int totalCount = 0;

            foreach (var status in statuses)
            {
                var singleStatusResult = await _successStoryRepository.GetPaginatedAsync(
                    pageNumber: 1,
                    pageSize: int.MaxValue, // Get all for this status
                    companyId: request.CompanyId,
                    status: status,
                    searchTerm: request.SearchTerm,
                    sortBy: request.SortBy ?? "CreatedAt",
                    sortDescending: request.SortDescending,
                    asNoTracking: true);

                allItems.AddRange(singleStatusResult.Items);
                totalCount += singleStatusResult.TotalCount;
            }

            // Apply sorting
            var sortedItems = ApplySorting(allItems, request.SortBy ?? "CreatedAt", request.SortDescending);

            // Apply pagination
            var pagedItems = sortedItems
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            paginatedResult = new PaginatedList<SuccessStory>(
                pagedItems,
                sortedItems.Count,
                request.PageNumber,
                request.PageSize);
        }
        else
        {
            // Single status or no status filter
            paginatedResult = await _successStoryRepository.GetPaginatedAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                companyId: request.CompanyId,
                status: statuses?.FirstOrDefault(),
                searchTerm: request.SearchTerm,
                sortBy: request.SortBy ?? "CreatedAt",
                sortDescending: request.SortDescending,
                asNoTracking: true);
        }

        var dtos = paginatedResult.Items.Select(s => new UserSuccessStorySubmissionDto
        {
            Id = s.Id,
            RequestId = s.RequestId,
            Title = s.Title.Value,
            SubmissionDate = s.CreatedAt,
            Type = MapSuccessStoryType(s.SuccessStoryTypeId),
            Status = s.Status,
            StatusDescription = MapStatusToDisplay(s.Status)
        }).ToList();

        var result = new PaginatedList<UserSuccessStorySubmissionDto>(
            dtos,
            paginatedResult.TotalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PaginatedList<UserSuccessStorySubmissionDto>>.Success(result);
    }

    private static List<SuccessStoryStatus>? MapStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        // Split by comma to support multiple statuses
        var statusList = status.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.ToLower())
            .ToList();

        if (statusList.Count == 0)
            return null;

        var mappedStatuses = new List<SuccessStoryStatus>();

        foreach (var statusItem in statusList)
        {
            var mapped = statusItem switch
            {
                "pending" or "pendingreview" => new List<SuccessStoryStatus>
                {
                    SuccessStoryStatus.PendingReview,
                    SuccessStoryStatus.pending
                },
                "published" => new List<SuccessStoryStatus> { SuccessStoryStatus.Published },
                "returned" or "rejected" => new List<SuccessStoryStatus>
                {
                    SuccessStoryStatus.AdminRejected,
                    SuccessStoryStatus.AssetManagerRejected
                },
                _ => null
            };

            if (mapped != null)
            {
                mappedStatuses.AddRange(mapped);
            }
        }

        return mappedStatuses.Count > 0 ? mappedStatuses.Distinct().ToList() : null;
    }

    private static List<SuccessStory> ApplySorting(List<SuccessStory> items, string sortBy, bool descending)
    {
        var query = items.AsQueryable();

        query = sortBy?.ToLower() switch
        {
            "title" => descending 
                ? query.OrderByDescending(s => s.Title.Value) 
                : query.OrderBy(s => s.Title.Value),
            "submissiondate" or "createdat" => descending 
                ? query.OrderByDescending(s => s.CreatedAt) 
                : query.OrderBy(s => s.CreatedAt),
            "status" => descending 
                ? query.OrderByDescending(s => s.Status) 
                : query.OrderBy(s => s.Status),
            _ => descending 
                ? query.OrderByDescending(s => s.CreatedAt) 
                : query.OrderBy(s => s.CreatedAt)
        };

        return query.ToList();
    }

    private static string MapStatusToDisplay(SuccessStoryStatus status)
    {
        return status switch
        {
            SuccessStoryStatus.PendingReview => "Pending",
            SuccessStoryStatus.pending => "Pending",
            SuccessStoryStatus.Published => "Published",
            SuccessStoryStatus.AssetManagerRejected => "Rejected",
            SuccessStoryStatus.AdminRejected => "Rejected",
            _ => status.ToString()
        };
    }

    private static string MapSuccessStoryType(int typeId)
    {
        // TODO: Load from lookup table
        return typeId switch
        {
            1 => "Partnership",
            2 => "Collaboration",
            3 => "Joint Venture",
            _ => "Unknown"
        };
    }
}
