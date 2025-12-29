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
    public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId)
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

    // User-Role Management
    public async Task<bool> AssignRoleToUserAsync(string userId, Guid roleId, Guid moduleId, string assignedBy)
    {
        var exists = await _userRoleRepository.ExistsAsync(userId, roleId, moduleId);
        if (exists)
            return false;

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            ModuleId = moduleId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow
        };

         await _userRoleRepository.AddAsync(userRole);

        var permissions = await  _rolePermissionRepository.GetPermissionsByRoleIdAsync(roleId);

        List<UserPermission> users = new List<UserPermission>();
        foreach ( var permission in permissions)
        {
            users.Add(new UserPermission
            {
                UserId = userId,
                PermissionId = permission.Id,
                ModuleId=moduleId,
            });
        }
        return await _userPermissionRepository.AddBulkAsync(users);
    }

    public async Task<bool> RemoveRoleFromUserAsync(string userId, Guid roleId, Guid moduleId)
    {
        return await _userRoleRepository.RemoveAsync(userId, roleId, moduleId);
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
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId);
        
        foreach (var userRole in userRoles)
        {
            var permissions = await _rolePermissionRepository.GetPermissionsByRoleIdAsync(userRole.RoleId);
            if (permissions.Any(p => p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
    {
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId);
        var allPermissions = new HashSet<string>();

        foreach (var userRole in userRoles)
        {
            var permissions = await _rolePermissionRepository.GetPermissionsByRoleIdAsync(userRole.RoleId);
            foreach (var permission in permissions)
            {
                allPermissions.Add(permission.Name);
            }
        }

        return allPermissions;
    }
}
