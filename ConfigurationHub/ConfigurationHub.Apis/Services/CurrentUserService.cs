
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;

namespace PartnersHub.ConfigurationHub.Serices
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CurrentUserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public string UserName =>( _contextAccessor?.HttpContext?.User.FindFirst("First Name")?.Value +" " + _contextAccessor?.HttpContext?.User.FindFirst("Last Name")?.Value) ?? string.Empty;
        public string UserId => _contextAccessor?.HttpContext?.User.FindFirst("contactId")?.Value ?? Guid.NewGuid().ToString();
        public string RoleId => _contextAccessor?.HttpContext?.User.FindFirst("roleId")?.Value ?? Guid.NewGuid().ToString();

    }
}
