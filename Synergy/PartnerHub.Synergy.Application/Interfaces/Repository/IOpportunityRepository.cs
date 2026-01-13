using PartnersHub.Synergy.Application.Common;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Domain.Common;
using System.Linq.Expressions;
using OpportunityEntity = PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate.Opportunity;
using SynergyCompanyEntity = PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany;

namespace PartnersHub.Synergy.Application.Interfaces.Repository;

public interface IOpportunityRepository
{
    Task<OpportunityEntity?> GetByIdAsync(Guid id, bool asNoTracking = false, params Expression<Func<OpportunityEntity, object>>[] includes);
    Task AddAsync(OpportunityEntity opportunity);
    void Update(OpportunityEntity opportunity);
    void Delete(OpportunityEntity opportunity);
    Task<bool> IsOpportunityWithTitleAndCompanyExistsAsync(string title, Guid companyId);
    Task<List<OpportunityEntity>> GetAllAsync(bool asNoTracking = false, params Expression<Func<OpportunityEntity, object>>[] includes);
    Task<List<OpportunityEntity>> GetByPublishingCompanyId(Guid companyId, bool asNoTracking = false, params Expression<Func<OpportunityEntity, object>>[] includes);
    Task<Dictionary<OpportunityEntity, List<SynergyCompanyEntity>>> GetOpportunitiesWithCompaniesByStatus(OpportunityStatus status,
       bool asNoTracking = false, params Expression<Func<OpportunityEntity, object>>[] includes);
    /// <summary>
    /// Unified method for searching opportunities with comprehensive filtering
    /// Handles both company-specific queries (user submissions) and platform-wide searches (public opportunities)
    /// </summary>
    Task<PaginatedList<OpportunityEntity>> SearchOpportunitiesAsync(
        int pageNumber,
        int pageSize,
        List<Guid>? companyIds = null,
        string? searchTerm = null,
        List<Guid>? sectorIds = null,
        List<int>? opportunityTypeIds = null,
        List<int>? thematicAreaIds = null,
        List<int>? collaborationRequirementIds = null,
        List<int>? expectedOutcomeIds = null,
        List<CollaborationStatusFilter> collaborationStatuses = null,
        List<OpportunityStatus>? statuses = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        string? sortBy = null,
        bool asNoTracking = true);
    
    Task<List<OpportunityEntity>> GetOpportunitiesByCollaboratedCompanyAsync(Guid companyId);



    // Dashboard statistics methods
    Task<int> GetTotalCountByStatusAsync(OpportunityStatus status, DateTime? fromDate = null);
    Task<int> GetCountByCompanyAndStatusAsync(Guid companyId, OpportunityStatus status, DateTime? fromDate = null);
    Task<int> GetDistinctCollaboratedCompaniesCountAsync(Guid companyId, DateTime? fromDate = null);
    Task<List<OpportunityEntity>> GetByIds(List<Guid> opportunityIds, bool asNoTracking = false, params Expression<Func<OpportunityEntity, object>>[] includes);
    Task<Dictionary<OpportunityEntity, List<SynergyCompanyEntity>>> GetOpportunitiesWithCompaniesByCompanyId(Guid companyId, bool asNoTracking = false, params Expression<Func<OpportunityEntity, object>>[] includes);
    Task<int> GetNextRequestIdAsync(CancellationToken cancellationToken = default);
    Task<(int publishedCount, int totalCount)> GetTotalCount(DateTime? fromDate = null);
}