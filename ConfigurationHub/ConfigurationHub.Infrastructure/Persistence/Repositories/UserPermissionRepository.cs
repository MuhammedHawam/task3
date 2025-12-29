using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Repositories
{
    public class UserPermissionRepository : IUserPermissionRepository
    {
        private readonly ConfigurationHubDbContext _context;

        public UserPermissionRepository(ConfigurationHubDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AddAsync(UserPermission userRole)
        {
            await _context.UserPermissions.AddAsync(userRole);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddBulkAsync(IEnumerable<UserPermission> userRole)
        {
            await _context.UserPermissions.AddRangeAsync(userRole);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveAsync(string userId, Guid PermissionId)
        {
            var userRole = await _context.UserPermissions
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.PermissionId == PermissionId);

            if (userRole == null) return false;

            _context.UserPermissions.Remove(userRole);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserPermission>> GetByUserIdAsync(string userId)
        {
            return await _context.UserPermissions
                .Include(ur => ur.Permission)
                .Include(ur => ur.Module)
                .Where(ur => ur.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserPermission>> GetByRoleIdAsync(Guid permissionId)
        {
            return await _context.UserPermissions
                .Include(ur => ur.Permission)
                .Include(ur => ur.Module)
                .Where(ur => ur.PermissionId == permissionId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string userId, Guid permissionId)
        {
            return await _context.UserPermissions
                .AnyAsync(ur => ur.UserId == userId && ur.PermissionId == permissionId);
        }
    }
}
