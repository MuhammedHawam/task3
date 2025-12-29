using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;

public interface IRoleRepository
{
    Task<Role> AddAsync(Role role);
    Task<Role?> GetByIdAsync(Guid roleId);
    Task<Role?> GetByNameAsync(string roleName);
    Task<PaginatedList<Role>> GetAllAsync(int pageNumber = 1, int pageSize = 20);
    Task<IEnumerable<Role>> GetByModuleIdAsync(Guid moduleId);
    Task<bool> UpdateAsync(Role role);
    Task<bool> DeleteAsync(Guid roleId);
    Task<bool> ExistsByNameAsync(string roleName);
}
