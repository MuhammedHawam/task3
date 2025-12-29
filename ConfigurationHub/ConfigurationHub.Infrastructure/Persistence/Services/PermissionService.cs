using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Permission> CreatePermissionAsync(string name, string description, Guid moduleId)
    {
        var existingPermissions = await _permissionRepository.GetAllAsync();
        if (existingPermissions.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Permission '{name}' already exists");

        var permission = new Permission
        {
            Name = name,
            Description = description,
            ModuleId = moduleId
        };

        await _permissionRepository.AddAsync(permission);
        return permission;
    }

    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync() =>
        await _permissionRepository.GetAllAsync();

    public async Task<IEnumerable<Permission>> GetPermissionsByModuleAsync(Guid moduleId) =>
        await _permissionRepository.GetByModuleIdAsync(moduleId);

    public async Task<Permission?> GetPermissionByIdAsync(Guid permissionId) =>
        await _permissionRepository.GetByIdAsync(permissionId);

    public async Task<Permission?> GetPermissionByNameAsync(string permissionName) =>
        await _permissionRepository.GetByNameAsync(permissionName);

    public async Task<bool> UpdatePermissionAsync(Guid permissionId, string name, string description)
    {
        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        if (permission == null) return false;

        permission.Name = name;
        permission.Description = description;
        await _permissionRepository.UpdateAsync(permission);
        return true;
    }

    public async Task<bool> DeletePermissionAsync(Guid permissionId) =>
        await _permissionRepository.DeleteAsync(permissionId);
}
