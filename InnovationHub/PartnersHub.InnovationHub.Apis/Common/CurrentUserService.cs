using PartnersHub.InnovationHub.Application.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
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

        public string UserId => GetClaimValueFromToken("ContactId").ToString() ?? GetClaimValueFromToken("idsub").ToString() ?? "";

        public Guid CurrentUserId => GetClaimValueFromToken("ContactId") ?? GetClaimValueFromToken("idsub") ?? new Guid();
        public string RoleId => _contextAccessor?.HttpContext?.User.FindFirst("roleId")?.Value ?? Guid.NewGuid().ToString();
        public string UserName => _contextAccessor?.HttpContext?.User?.Claims?.Where(x => x.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault() ?? string.Empty;

        public Guid CompanyId => GetClaimValueFromToken("CompanyId") ?? new Guid();


        private Guid? GetClaimValueFromToken(string claimType)
        {
            var httpContext = _contextAccessor?.HttpContext;

            if (httpContext == null)
                throw new Exception("Invalid Request: HttpContext is null.");

            //Get the Authorization header value
            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return default;
                //throw new Exception("Invalid Authorization Header: Missing or malformed Bearer token.");
            }
            //Extract the token string
            var tokenString = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(tokenString))
                {
                    var token = handler.ReadJwtToken(tokenString);

                    var claimValue = token.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;

                    //if (string.IsNullOrEmpty(claimValue))
                    //{
                    //    throw new Exception($"Missing Required Claim: The token does not contain a claim of type '{claimType}'.");
                    //}

                    if (Guid.TryParse(claimValue, out Guid parsedId))
                    {
                        return parsedId;
                    }
                    else
                    {
                        return default;
                        //throw new Exception($"Invalid Claim Format: The claim '{claimType}' value ('{claimValue}') is not a valid GUID.");
                    }
                }
                else
                {
                    //throw new Exception("Token Read Failure: The authorization string is not a readable JWT.");
                    return null;
                }
            }

            catch (Exception ex)
            {
                throw new Exception("Invalid user token: An unexpected error occurred during token parsing.", ex);
            }
        }

    }
}
