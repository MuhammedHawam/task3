using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;

public interface IUserRoleRepository
{
    Task<bool> AddAsync(UserRole userRole);
    Task<bool> RemoveAsync(string userId, Guid roleId, Guid moduleId);
    Task<IEnumerable<UserRole>> GetByUserIdAsync(string userId);
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId);
    Task<bool> ExistsAsync(string userId, Guid roleId, Guid moduleId);
    Task<PaginatedList<AdminUserDto>> GetAdminsPaginatedAsync(int pageNumber = 1, int pageSize = 20);
}
