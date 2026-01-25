using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly ConfigurationHubDbContext _context;

    public UserRoleRepository(ConfigurationHubDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(UserRole userRole)
    {
        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(string userId, Guid roleId, Guid moduleId)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.ModuleId == moduleId);

        if (userRole == null) return false;

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(string userId)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Include(ur => ur.Module)
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Include(ur => ur.Module)
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string userId, Guid roleId, Guid moduleId)
    {
        return await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.ModuleId == moduleId);
    }

    public async Task<PaginatedList<AdminUserDto>> GetAdminsPaginatedAsync(int pageNumber = 1, int pageSize = 20)
    {
        var users = _context.UserRoles
            .Include(ur => ur.Role)
            .Include(ur => ur.Module)
            .Where(ur => ur.Role.Name.Contains("admin"))
            .Select(a => new AdminUserDto
            {
                AssignedAt = a.AssignedAt,
                AssignedBy = a.AssignedBy,
                UserId = a.UserId,
                ProductName = a.Module.Name,
                DisplayName = a.UserName,
                Email = a.UserEmail,
                Role = a.Role.Name,
                RoleId = a.RoleId,
                ModuleId = a.ModuleId
                
            })
            .AsNoTracking();

        var totalCount = await users.CountAsync();

        var items = await users
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PaginatedList<AdminUserDto>.Create(items, totalCount, pageNumber, pageSize);

    }
}
