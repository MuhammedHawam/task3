using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUserPermissionRepository _userPermissionRepository;


    public RoleService(
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IRolePermissionRepository rolePermissionRepository, IUserPermissionRepository userPermissionRepository)
    {
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _userPermissionRepository = userPermissionRepository;
    }

    // Role Management
    public async Task<Role> CreateRoleAsync(string name, string description, Guid? moduleId)
    {
        var exists = await _roleRepository.ExistsByNameAsync(name);
        if (exists)
            throw new InvalidOperationException($"Role '{name}' already exists");

        var role = new Role
        {
            Name = name,
            Description = description,
            ModuleId = moduleId,
            IsActive = true,
            IsSystemRole = false
        };

        return await _roleRepository.AddAsync(role);
    }

    public async Task<PaginatedList<Role>> GetAllRolesAsync(int pageSize,int pageNumber)
    {
        return await _roleRepository.GetAllAsync(pageNumber, pageSize);
    }

    public async Task<List<LookupDto>> GetAllRolesLookupByModuleAsync(Guid moduleId)
    {
        return await _roleRepository.GetAllLookUpByModuleAsync(moduleId);
    }

    public async Task<IEnumerable<Role>> GetRolesByModuleAsync(Guid moduleId)
    {
        return await _roleRepository.GetByModuleIdAsync(moduleId);
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId)
    {
        return await _roleRepository.GetByIdAsync(roleId);
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _roleRepository.GetByNameAsync(roleName);
    }

    public async Task<bool> UpdateRoleAsync(Guid roleId, string name, string description)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            return false;

        if (role.IsSystemRole)
            throw new InvalidOperationException("Cannot modify system roles");

        role.Name = name;
        role.Description = description;

        return await _roleRepository.UpdateAsync(role);
    }

    public async Task<bool> DeleteRoleAsync(Guid roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            return false;

        if (role.IsSystemRole)
            throw new InvalidOperationException("Cannot delete system roles");

        return await _roleRepository.DeleteAsync(roleId);
    }

    // Role-Permission Management
    public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, List<Guid> permissionId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role == null)
            throw new InvalidOperationException("Role not found");

        return await _rolePermissionRepository.AddAsync(roleId, permissionId);
    }

    public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
    {
        return await _rolePermissionRepository.RemoveAsync(roleId, permissionId);
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId)
    {
        return await _rolePermissionRepository.GetPermissionsByRoleIdAsync(roleId);
    }

    public async Task<IEnumerable<LookupDto>> GetRolePermissionsLookupAsync(Guid roleId)
    {
        return await _rolePermissionRepository.GetPermissionsLookupByRoleIdAsync(roleId);
    }

    public async Task<bool> AssignRoleToUserAsync(string userId, string userName, string userEmail, Guid roleId, Guid moduleId, string assignedBy)
    {
        if (await _userRoleRepository.ExistsAsync(userId, roleId, moduleId))
            return false;

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            ModuleId = moduleId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            UserName = userName,
            UserEmail = userEmail,
        };

        await _userRoleRepository.AddAsync(userRole);
        var rolePermissions = await _rolePermissionRepository.GetPermissionsByRoleIdAsync(roleId);

        var existingUserPermissions = await _userPermissionRepository.GetByUserIdAsync(userId);
        var existingPermissionIds = existingUserPermissions
            .Where(p => p.ModuleId == moduleId)
            .Select(p => p.PermissionId)
            .ToHashSet();

        var permissionsToInsert = rolePermissions
            .Where(rp => !existingPermissionIds.Contains(rp.Id)) // Only add what they don't have
            .Select(rp => new UserPermission
            {
                UserId = userId,
                PermissionId = rp.Id,
                ModuleId = moduleId
            })
            .ToList();

        if (permissionsToInsert.Any())
        {
            return await _userPermissionRepository.AddBulkAsync(permissionsToInsert);
        }

        return true;
    }

    public async Task<bool> RemoveRoleFromUserAsync(string userId, Guid roleId, Guid moduleId)
    {
        return await _userRoleRepository.RemoveAsync(userId, roleId, moduleId);
    }

    public async Task<PaginatedList<AdminUserDto>> GetPaginatedAdminAsync( int pagenumber, int pageIndex,string? searchTerm = null, string? sortBy = null)
    {
        return await _userRoleRepository.GetAdminsPaginatedAsync(searchTerm, sortBy,pagenumber, pageIndex);
    }
    public async Task<IEnumerable<Role>> GetUserRolesAsync(string userId)
    {
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId);
        return userRoles.Select(ur => ur.Role);
    }

    public async Task<IEnumerable<UserRole>> GetUserRoleDetailsAsync(string userId)
    {
        return await _userRoleRepository.GetByUserIdAsync(userId);
    }

    // Authorization Methods
    public async Task<bool> UserHasRoleAsync(string userId, string roleName)
    {
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId);
        return userRoles.Any(ur => ur.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> UserHasPermissionAsync(string userId, string permissionName)
    {
        var permissionNames = await _rolePermissionRepository.GetPermissionNamesByUserIdAsync(userId);
        return permissionNames.Any(name => name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
    {
        return await _rolePermissionRepository.GetPermissionNamesByUserIdAsync(userId);
    }

    public async Task<bool> UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionId)
    {
        return await _rolePermissionRepository.UpdateBulkAsync(roleId, permissionId);
    }
}
