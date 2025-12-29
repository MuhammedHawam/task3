using Microsoft.AspNetCore.Authorization;
using PartnersHub.InfraBase.Apis.Common;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Infrastructure.Services;
using PartnersHub.InnovationHub.Apis.Common;

namespace PartnersHub.InfraBase.Apis.Common
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public PermissionHandler(IHttpContextAccessor contextAccessor, IServiceScopeFactory scopeFactory)
        {
            _contextAccessor = contextAccessor;
            _scopeFactory = scopeFactory;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                // Resolve the Scoped service (PermissionCacheService) within the temporary scope
                var cacheService = scope.ServiceProvider.GetRequiredService<PermissionCacheService>();
                var userService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();

                if (!Guid.TryParse(userService.UserId, out var UserGuid)) return;

                var permissions = await cacheService.GetPermissionsForRoleAsync(UserGuid);
                if (permissions.Contains(requirement.Permission))
                    context.Succeed(requirement);
            }
        }

    }
}
