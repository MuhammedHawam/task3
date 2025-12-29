using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence;

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

        public async Task<IEnumerable<Permission>> GetByModuleIdAsync(Guid moduleId)
            => await _context.Permissions.Include(p => p.Module).Where(p => p.ModuleId == moduleId).ToListAsync();

        public async Task<Permission?> GetByIdAsync(Guid id)
            => await _context.Permissions.Include(p => p.Module).FirstOrDefaultAsync(p => p.Id == id);

        public async Task<Permission?> GetByNameAsync(string name)
            => await _context.Permissions.Include(p => p.Module).FirstOrDefaultAsync(p => p.Name == name);
    }
}
