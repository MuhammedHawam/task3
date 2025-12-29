using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;

public interface IPermissionService
{
    Task<Permission> CreatePermissionAsync(string name, string description, Guid moduleId);
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    Task<IEnumerable<Permission>> GetPermissionsByModuleAsync(Guid moduleId);
    Task<Permission?> GetPermissionByIdAsync(Guid permissionId);
    Task<bool> UpdatePermissionAsync(Guid permissionId, string name, string description);
    Task<bool> DeletePermissionAsync(Guid permissionId);
}
