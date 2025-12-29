using Microsoft.AspNetCore.Http;
using PartnersHub.Synergy.Application.Interfaces.Common;
using System;
using System.IdentityModel.Tokens.Jwt; // Needed to read the token manually
using System.Security.Claims;

// Assuming IUserService interface defines CurrentUserId, CompanyId, and UserName
public class UserService : IUserService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public UserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    // New helper method to manually read the token and extract a claim value
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
    // Use the helper method for robust claim extraction
    public Guid CurrentUserId => GetClaimValueFromToken("ContactId") ?? GetClaimValueFromToken("idsub") ??default;

    public Guid CompanyId => GetClaimValueFromToken("CompanyId") ?? default;


}