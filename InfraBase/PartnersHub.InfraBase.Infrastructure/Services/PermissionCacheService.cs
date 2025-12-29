using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using PartnersHub.InfraBase.Application.Common.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InfraBase.Infrastructure.Services
{
    public class PermissionCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IAdminCommunicationService _adminClient;
        public PermissionCacheService(IMemoryCache cache, IAdminCommunicationService adminClient)
        {
            _cache = cache;
            _adminClient = adminClient;
        }

        public async Task<HashSet<string>> GetPermissionsForRoleAsync(Guid userId)
        {
            return await _cache.GetOrCreateAsync(userId, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                var permissionDto = await _adminClient.GetUserPermissions(userId);
                return permissionDto.ToHashSet();
            });
        }

    }
}