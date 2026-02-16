using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;

public interface IPermissionService
{
    Task<Permission> CreatePermissionAsync(string name, string description, Guid moduleId);
    Task<IEnumerable<Permission>> GetAllPermissionsAsync();
    Task<IEnumerable<LookupDto>> GetAllPermissionsLookupAsync();
    Task<IEnumerable<Permission>> GetPermissionsByModuleAsync(Guid moduleId);
    Task<PaginatedList<ModulePermissionsRolesDto>> GetAllAssignedPermissionsRole(int pageSize, int pageIndex, string? searchparam, string? sortBy);
    Task<Permission?> GetPermissionByIdAsync(Guid permissionId);
    Task<bool> UpdatePermissionAsync(Guid permissionId, string name, string description);
    Task<bool> DeletePermissionAsync(Guid permissionId);
}
