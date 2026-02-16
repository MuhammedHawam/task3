using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence
{
    public interface IPermissionRepository
    {
        Task<Permission?> GetByIdAsync(Guid id);
        Task<Permission?> GetByNameAsync(string name);
        Task<IEnumerable<Permission>> GetAllAsync();

        Task<IEnumerable<LookupDto>> GetAllPermissionLookpAsync();
        Task<IEnumerable<Permission>> GetByModuleIdAsync(Guid moduleId);

        Task<PaginatedList<ModulePermissionsRolesDto>> GetAllAssignedPermissionsRole(int pageSize, int pageIndex, string? searchparam, string? sortBy = null);
        Task AddAsync(Permission permission);
        Task UpdateAsync(Permission permission);
        Task<bool> DeleteAsync(Guid id);
    }
}
