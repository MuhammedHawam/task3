using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly ConfigurationHubDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RolePermissionRepository(ConfigurationHubDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> AddAsync(Guid roleId, List<Guid> permissionIds)
    {
        var distinctPermissionIds = (permissionIds ?? new List<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (distinctPermissionIds.Count == 0)
        {
            return true;
        }

        var hasAnyExisting = await _context.RolePermissions
            .AsNoTracking()
            .AnyAsync(rp => rp.RoleId == roleId && distinctPermissionIds.Contains(rp.PermissionId));

        if (hasAnyExisting)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var rolePermissions = distinctPermissionIds.Select(permissionId => new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            AssignedBy = _currentUserService.UserName,
            AssignedDate = now,
        });

        await _context.RolePermissions.AddRangeAsync(rolePermissions);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateBulkAsync(Guid roleId, List<Guid> permissionId)
    {
        permissionId = permissionId ?? new List<Guid>();

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                await _context.RolePermissions
                    .Where(a => a.RoleId == roleId)
                    .ExecuteDeleteAsync();

                var newPermissions = permissionId.Select(pId => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = pId
                }).ToList();

                if (newPermissions.Any())
                {
                    _context.RolePermissions.AddRange(newPermissions);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }
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
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .ThenInclude(p => p.Module)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<IEnumerable<LookupDto>> GetPermissionsLookupByRoleIdAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => new LookupDto
            {
                Id = rp.PermissionId,
                Value = rp.Permission.Name
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetRolesByPermissionIdAsync(Guid permissionId)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.PermissionId == permissionId)
            .Include(rp => rp.Role)
            .ThenInclude(r => r.Module)
            .Select(rp => rp.Role)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid roleId, Guid permissionId)
    {
        return await _context.RolePermissions
            .AsNoTracking()
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionNamesByUserIdAsync(string userId)
    {
        return await (
            from userRole in _context.UserRoles.AsNoTracking()
            join rolePermission in _context.RolePermissions.AsNoTracking()
                on userRole.RoleId equals rolePermission.RoleId
            join permission in _context.Permissions.AsNoTracking()
                on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId
            select permission.Name
        )
        .Distinct()
        .ToListAsync();
    }
}
