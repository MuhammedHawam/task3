using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PartnersHub.ConfigurationHub.Infrastructure.Presistence.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ConfigurationHubDbContext _context;
        
        public PermissionRepository(ConfigurationHubDbContext context) => _context = context;

        public async Task AddAsync(Permission permission)
        {
            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Permission permission)
        {
            _context.Permissions.Update(permission);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var p = await _context.Permissions.FindAsync(id);
            if (p != null)
            {
                _context.Permissions.Remove(p);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
            => await _context.Permissions.Include(p => p.Module).ToListAsync();


        public async Task<IEnumerable<LookupDto>> GetAllPermissionLookpAsync()
            => await _context.Permissions.Select(a=> new LookupDto
            {
                Id = a.Id,
                Value = a.Name
            }).ToListAsync();


        public async Task<PaginatedList<ModulePermissionsRolesDto>> GetAllAssignedPermissionsRole(int pageSize, int pageIndex, string? searchparam)
        {
            var query = _context.Roles
                .Include(r => r.Module)
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchparam))
            {
                query = query.Where(r =>
                    r.Name.Contains(searchparam) ||
                    r.Module.Name.Contains(searchparam) ||
                    r.RolePermissions.Any(rp => rp.Permission.Name.Contains(searchparam)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(r => r.Name) 
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ModulePermissionsRolesDto
                {
                    ProductId = r.ModuleId,
                    ProductName = r.Module.Name,
                    RoleId = r.Id,
                    RoleName = r.Name,
                    Permissions = r.RolePermissions
                        .Select(rp => new PermissionDto
                        {
                            Id = rp.Permission.Id,
                           PermissionName = rp.Permission.Name
                        })
                        .ToList()
                })
                .ToListAsync();

            return new PaginatedList<ModulePermissionsRolesDto>(items, totalCount, pageIndex, pageSize);
        }

        public async Task<IEnumerable<Permission>> GetByModuleIdAsync(Guid moduleId)
            => await _context.Permissions.Include(p => p.Module).Where(p => p.ModuleId == moduleId).ToListAsync();

        public async Task<Permission?> GetByIdAsync(Guid id)
            => await _context.Permissions.Include(p => p.Module).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Permission?> GetByNameAsync(string name)
            => await _context.Permissions.Include(p => p.Module).FirstOrDefaultAsync(p => p.Name == name);
    }
}
