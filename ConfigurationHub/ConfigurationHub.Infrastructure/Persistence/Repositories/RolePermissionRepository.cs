using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly ConfigurationHubDbContext _context;

    public RolePermissionRepository(ConfigurationHubDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(Guid roleId, Guid permissionId)
    {
        var exists = await ExistsAsync(roleId, permissionId);
        if (exists) return false;

        await _context.RolePermissions.AddAsync(new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(Guid roleId, Guid permissionId)
    {
        var rolePermission = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rolePermission == null) return false;

        _context.RolePermissions.Remove(rolePermission);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .ThenInclude(p => p.Module)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetRolesByPermissionIdAsync(Guid permissionId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.PermissionId == permissionId)
            .Include(rp => rp.Role)
            .ThenInclude(r => r.Module)
            .Select(rp => rp.Role)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid roleId, Guid permissionId)
    {
        return await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }
}
