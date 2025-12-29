using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using System.Linq.Expressions;

namespace PartnersHub.Synergy.Application.Interfaces.Repository
{
    public interface ISynergyCompanyRepository
    {
        Task<Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany?> GetByIdAsync(Guid id, bool asNoTracking = false, params Expression<Func<Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany, object>>[] includes);
        Task<IEnumerable<Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany>> GetAllAsync(bool asNoTracking = false, params Expression<Func<Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany, object>>[] includes);
        Task<List<Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany>> GetByIdsAsync(List<Guid> ids, bool asNoTracking = false);
        
        Task AddAsync(Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany company);
        void Update(Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany company);
        Task<PaginatedList<Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany>> Search(int pageSize,
        int pageNumber,
        string? searchTerm = null,
        List<Guid>? sectors = null,
        List<string>? cities = null,
        List<string>? countries = null,
        string? sortBy = null,
        bool sortDescending = true);
        // Dashboard methods
        Task<int> GetTotalCompaniesCountAsync();
        Task<int> GetTotalActiveCompaniesCountAsync();
        Task<List<Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany>> GetRecentAsync(int count, bool asNoTracking = true, params Expression<Func<Domain.Aggregates.SynergyCompanyAggregate.SynergyCompany, object>>[] includes);
    }
}
