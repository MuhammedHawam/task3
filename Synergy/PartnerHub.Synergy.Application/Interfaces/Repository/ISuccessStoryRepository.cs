using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.SuccessStories.DTOs;
using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;
using PartnersHub.Synergy.Domain.Common;
using System.Linq.Expressions;

namespace PartnersHub.Synergy.Application.Interfaces.Repository;

public interface ISuccessStoryRepository
{
    Task AddAsync(SuccessStory successStory);
    void Update(SuccessStory successStory);
    void Delete(SuccessStory successStory);
    
    Task<IEnumerable<SuccessStory>> GetAllAsync(bool asNoTracking = false, params Expression<Func<SuccessStory, object>>[] includes);
    Task<SuccessStory> GetByIdAsync(Guid id, bool asNoTracking = false, params Expression<Func<SuccessStory, object>>[] includes);
    Task<List<SuccessStory>> GetByCompanyIdAsync(Guid companyId);

    /// <summary>
    /// Get paginated success stories with filtering, sorting, and search
    /// </summary>
    Task<PaginatedList<SuccessStory>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        Guid? companyId = null,
        SuccessStoryStatus? status = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = true,
        bool includeIsHide = true,
        bool asNoTracking = false);

    // Dashboard statistics methods
    Task<int> GetCountByCompanyAndStatusAsync(Guid companyId, SuccessStoryStatus status, DateTime? fromDate = null);
    Task<int> GetTotalCountByStatusAsync(SuccessStoryStatus status, DateTime? fromDate = null);
    Task<int> GetNextRequestIdAsync(CancellationToken cancellationToken = default);
    Task<(int PublishedCount, int TotalCount)> GetTotalCount(DateTime? fromDate = null);
    Task<bool> CheckTitleUniqueness(string Title, Guid? Id);
}
