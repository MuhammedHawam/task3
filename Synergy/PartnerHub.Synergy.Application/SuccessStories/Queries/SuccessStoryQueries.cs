using MediatR;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.SuccessStories.DTOs;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Domain.Events;


namespace PartnersHub.Synergy.Application.SuccessStories.Queries
{
    public class GetSuccessStoryByIdQuery : IRequest<Result<SuccessStoryResponseDto>>
    {
        public Guid Id { get; set; }

    }
    public class GetAllSuccessStoriesQuery : IRequest<Result<List<SuccessStoryResponseDto>>>
    {

    }
    public class GetSuccessStoriesByStatusQuery : IRequest<Result<List<SuccessStoryResponseDto>>>
    {
        public SuccessStoryStatus Status { get; set; }
    }
    
    /// <summary>
    /// Query for searching success stories with pagination, search, and filtering
    /// </summary>
    public class SearchSuccessStoriesQuery : IRequest<Result<PaginatedList<SuccessStoryResponseDto>>>
    {
        public List<Guid>? CompanyIds { get; set; }
        public List<int>? CollaborationTypes { get; set; }
        public List<Guid>? SectorIds { get; set; }
        
        /// <summary>
        /// Status filter as string (e.g., "Published", "PendingReview", "AssetManagerApproved")
        /// Converted to enum internally
        /// </summary>
        public string? Status { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PartnerCompanyName { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public bool SortDescending { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
