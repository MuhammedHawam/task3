using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence
{
    public interface IUserPermissionRepository
    {
        Task<bool> AddAsync(UserPermission userRole);
        Task<bool> AddBulkAsync(IEnumerable<UserPermission> userRole);
        Task<bool> RemoveAsync(string userId, Guid permissionId);
        Task<IEnumerable<UserPermission>> GetByUserIdAsync(string userId);
        Task<IEnumerable<UserPermission>> GetByRoleIdAsync(Guid permissionId);
        Task<bool> ExistsAsync(string userId, Guid permissionId);

    }
}
