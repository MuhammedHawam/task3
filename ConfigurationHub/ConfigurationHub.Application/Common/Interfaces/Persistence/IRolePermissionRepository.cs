using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;

public interface IRolePermissionRepository
{
    Task<bool> AddAsync(Guid roleId, Guid permissionId);
    Task<bool> RemoveAsync(Guid roleId, Guid permissionId);
    Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId);
    Task<IEnumerable<Role>> GetRolesByPermissionIdAsync(Guid permissionId);
    Task<bool> ExistsAsync(Guid roleId, Guid permissionId);
}
