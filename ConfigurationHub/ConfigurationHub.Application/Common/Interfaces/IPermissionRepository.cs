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
        Task<IEnumerable<Permission>> GetByModuleIdAsync(Guid moduleId);
        Task AddAsync(Permission permission);
        Task UpdateAsync(Permission permission);
        Task<bool> DeleteAsync(Guid id);
    }
}
