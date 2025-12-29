using PartnersHub.InnovationHub.Application.Common.Interfaces;
using System.Security.Claims;

namespace PartnersHub.InnovationHub.Apis.Common
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
        public string UserName => _contextAccessor?.HttpContext?.User?.Claims?.Where(x => x.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() ?? string.Empty;
         
    }
}
