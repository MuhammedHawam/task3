using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;

public interface IRolePermissionRepository
{
    Task<bool> AddAsync(Guid roleId, List<Guid> permissionIds);
    Task<bool> RemoveAsync(Guid roleId, Guid permissionId);
    Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId);
    Task<IEnumerable<LookupDto>> GetPermissionsLookupByRoleIdAsync(Guid roleId);
    Task<IEnumerable<Role>> GetRolesByPermissionIdAsync(Guid permissionId);
    Task<bool> ExistsAsync(Guid roleId, Guid permissionId);
    Task<bool> UpdateBulkAsync(Guid roleId, List<Guid> permissionId);
}
