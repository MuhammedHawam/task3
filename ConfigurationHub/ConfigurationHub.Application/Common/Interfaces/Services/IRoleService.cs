using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;

/// <summary>
/// Service for managing roles and role-based access control
/// </summary>
public interface IRoleService
{
    // Role Management
    Task<Role> CreateRoleAsync(string name, string description, Guid? moduleId);
    Task<PaginatedList<Role>> GetAllRolesAsync(int pageSize, int pageNumber);
    Task<IEnumerable<Role>> GetRolesByModuleAsync(Guid moduleId);
    Task<Role?> GetRoleByIdAsync(Guid roleId);
    Task<Role?> GetRoleByNameAsync(string roleName);
    Task<bool> UpdateRoleAsync(Guid roleId, string name, string description);
    Task<bool> DeleteRoleAsync(Guid roleId);
    Task<List<LookupDto>> GetAllRolesLookupByModuleAsync(Guid moduleId);

    // Role-Permission Management
    Task<bool> AssignPermissionToRoleAsync(Guid roleId, List<Guid> permissionId);
    Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId);
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId);

    Task<IEnumerable<LookupDto>> GetRolePermissionsLookupAsync(Guid roleId);

    // User-Role Management
    Task<bool> AssignRoleToUserAsync(string userId, string userName, string useremail, Guid roleId, Guid moduleId, string assignedBy);
    Task<bool> RemoveRoleFromUserAsync(string userId, Guid roleId, Guid moduleId);
    Task<IEnumerable<Role>> GetUserRolesAsync(string userId);
    Task<IEnumerable<UserRole>> GetUserRoleDetailsAsync(string userId);
    
    // Authorization Queries
    Task<bool> UserHasRoleAsync(string userId, string roleName);
    Task<bool> UserHasPermissionAsync(string userId, string permissionName);
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);
    Task<bool> UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionId);

    Task<PaginatedList<AdminUserDto>> GetPaginatedAdminAsync(int pagenumber, int pageIndex, string? searchTerm = null, string? sortBy = null);
}
