using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using PartnersHub.InfraBase.Application.Common.Interfaces;

namespace PartnersHub.InfraBase.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public TokenService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public string GetUserEmail()
    {
        var email = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("ContactId")?.Value;

        //if (string.IsNullOrEmpty(email))
        //{
        //    throw new UnauthorizedAccessException("User email not found in token");
        //}

        return email;
    }

    public string GetUserName()
    {
        var email = GetUserEmail();
        
        if (string.IsNullOrEmpty(email))
        {
            return "Unknown User";
        }

        // Extract username from email (part before @)
        var atIndex = email.IndexOf('@');
        return atIndex > 0 ? email.Substring(0, atIndex) : email;
    }

    public Guid? GetCompanyId()
    {
        var companyIdClaim = _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(claim => IsCompanyIdClaim(claim.Type))
            ?.Value;

        if (string.IsNullOrWhiteSpace(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
        {
            return null;  // Return null if CompanyId not found or ADFS admin user
        }

        return companyId;
    }

    public string? GetCompanyName()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst("company_name")?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("companyName")?.Value;
    }

    public List<Guid> GetUserRoleIds()
    {
        var roleIdClaims = _httpContextAccessor.HttpContext?.User?.FindAll("RoleId")
            ?? _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role);

        var roleIds = new List<Guid>();

        if (roleIdClaims != null)
        {
            foreach (var claim in roleIdClaims)
            {
                if (Guid.TryParse(claim.Value, out var roleId))
                {
                    roleIds.Add(roleId);
                }
            }
        }

        return roleIds;
    }

    public bool IsPcAdmin()
    {
        var userRoleIds = GetUserRoleIds();
        var pcAdminRoleIds = GetConfiguredRoleIds("RoleIds:PcAdmin");

        return userRoleIds.Any(userRole => pcAdminRoleIds.Contains(userRole));
    }

    public bool IsInfrabaseAdmin()
    {
        var userRoleIds = GetUserRoleIds();
        var infrabaseAdminRoleIds = GetConfiguredRoleIds("RoleIds:InfrabaseAdmin");

        return userRoleIds.Any(userRole => infrabaseAdminRoleIds.Contains(userRole));
    }

    private List<Guid> GetConfiguredRoleIds(string configKey)
    {
        var roleIdsString = _configuration[configKey];

        if (string.IsNullOrWhiteSpace(roleIdsString))
        {
            return new List<Guid>();
        }

        return roleIdsString
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();
    }

    private static bool IsCompanyIdClaim(string claimType)
    {
        if (string.IsNullOrWhiteSpace(claimType))
        {
            return false;
        }

        var normalized = string.Concat(claimType.Where(char.IsLetterOrDigit))
            .ToLowerInvariant();
        return normalized == "companyid";
    }
}
