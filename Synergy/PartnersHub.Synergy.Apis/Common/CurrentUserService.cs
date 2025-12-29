using PartnersHub.Synergy.Apis.Common;
using PartnersHub.Synergy.Application.Common.Interfaces;

namespace PartnersHub.Synergy.Apis.Common
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CurrentUserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string UserId => _contextAccessor?.HttpContext?.User.FindFirst("contactId")?.Value ?? Guid.NewGuid().ToString();
        public string RoleId => _contextAccessor?.HttpContext?.User.FindFirst("roleId")?.Value ?? Guid.NewGuid().ToString();

    }
}
