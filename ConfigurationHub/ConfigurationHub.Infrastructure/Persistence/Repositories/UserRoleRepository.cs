using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Helpers;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

    public async Task<PaginatedList<AdminUserDto>> GetAdminsPaginatedAsync(string? searchTerm = null, string? sortBy = null, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.UserRoles
                            .Include(ur => ur.Role)
                            .Include(ur => ur.Module)
                            .OrderByDescending(x=>x.AssignedAt)
                            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x =>
                x.UserName.Contains(searchTerm));
        }

        query = sortBy?.ToLower() switch
        {
            "assignedat:asc" => query.OrderBy(x => x.AssignedAt),
            "assignedat:desc" => query.OrderByDescending(x => x.AssignedAt),

            "displayname:asc" => query.OrderBy(x => x.UserName),
            "displayname:desc" => query.OrderByDescending(x => x.UserName),

            "productname:asc" => query.OrderBy(x => x.Module.Name),
            "productname:desc" => query.OrderByDescending(x => x.Module.Name),

            "role:asc" => query.OrderBy(x => x.Role.Name),
            "role:desc" => query.OrderByDescending(x => x.Role.Name),

            "email:asc" => query.OrderBy(x => x.UserEmail),
            "email:desc" => query.OrderByDescending(x => x.UserEmail),

            _ => query.OrderByDescending(x => x.AssignedAt) 
        };

        var totalCount = await query.CountAsync();

        var items = await query
                         .Skip((pageNumber - 1) * pageSize)
                         .Take(pageSize)
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
                                      .ToListAsync();

   

        return PaginatedList<AdminUserDto>.Create(items, totalCount, pageNumber, pageSize);

    }
}
