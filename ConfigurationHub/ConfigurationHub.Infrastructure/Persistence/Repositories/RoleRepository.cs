using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ConfigurationHubDbContext _context;

    public RoleRepository(ConfigurationHubDbContext context)
    {
        _context = context;
    }

    public async Task<Role> AddAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role?> GetByIdAsync(Guid roleId)
    {
        return await _context.Roles
            .Include(r => r.Module)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _context.Roles
            .Include(r => r.Module)
            .FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<PaginatedList<Role>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Roles
            .Include(r => r.Module)
            .Where(r => r.IsActive)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PaginatedList<Role>.Create(items, totalCount, pageNumber, pageSize);
    }

    public async Task<List<LookupDto>> GetAllLookUpByModuleAsync(Guid moduleId)
    {
        return await _context.Roles
            .Where(r => r.IsActive)
            .Include(r => r.Module)
            .Where(r => r.ModuleId == moduleId)
            .Select(a=>new LookupDto
            {
                Id = a.Id,
                Value = a.Name
            })
            .AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetByModuleIdAsync(Guid moduleId)
    {
        return await _context.Roles
            .Include(r => r.Module)
            .Where(r => r.ModuleId == moduleId && r.IsActive)
            .ToListAsync();
    }

    public async Task<bool> UpdateAsync(Role role)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid roleId)
    {
        var role = await GetByIdAsync(roleId);
        if (role == null) return false;

        role.IsActive = false;
        await UpdateAsync(role);
        return true;
    }

    public async Task<bool> ExistsByNameAsync(string roleName)
    {
        return await _context.Roles.AnyAsync(r => r.Name == roleName);
    }
}
