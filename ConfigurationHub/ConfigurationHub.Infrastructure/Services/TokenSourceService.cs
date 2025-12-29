using System.Security.Claims;

namespace PartnersHub.ConfigurationHub.Infrastructure.Services;

public interface ITokenSourceService
{
    string GetTokenSource(ClaimsPrincipal user);
    bool IsInternalUser(ClaimsPrincipal user);
    bool IsExternalUser(ClaimsPrincipal user);
    string GetUsername(ClaimsPrincipal user);
    string GetUsernameFromUrl(string url);
}

public class TokenSourceService : ITokenSourceService
{
    /// <summary>
    /// Determines the source of the authentication token
    /// </summary>
    public string GetTokenSource(ClaimsPrincipal user)
    {
        if (user?.Identity?.AuthenticationType == null)
            return "Unknown";

        // Check issuer claim
        var issuer = user.FindFirst("iss")?.Value;
        
        if (issuer != null)
        {
            // ADFS issuers
            if (issuer.Contains("testadfs.pif.gov.sa") || 
                issuer.Contains("adfs.pif.gov.sa") ||
                issuer.Contains("adfs/services/trust"))
                return "ADFS (Internal Portal)";
            
            // CIAM issuers
            if (issuer.Contains("ciam-uat.pif.gov.sa") || 
                issuer.Contains("ciam.pif.gov.sa"))
                return "CIAM (External Portal)";
            
            // External middleware issuer
            if (issuer.Contains("EBPIDENTITYSERVER.COM"))
                return "Middleware (External Portal)";
        }

        // Check authentication type
        var authType = user.Identity.AuthenticationType;
        if (authType?.Contains("ActiveDirectory") == true)
            return "ADFS (Internal Portal)";
        
        if (authType?.Contains("SsoTwo") == true || authType?.Contains("ExternalPortal") == true)
            return "CIAM (External Portal)";

        return $"Unknown ({authType})";
    }

    /// <summary>
    /// Checks if user is from internal portal (ADFS)
    /// </summary>
    public bool IsInternalUser(ClaimsPrincipal user)
    {
        var issuer = user.FindFirst("iss")?.Value;
        
        if (issuer != null)
        {
            return issuer.Contains("testadfs.pif.gov.sa") || 
                   issuer.Contains("adfs.pif.gov.sa") ||
                   issuer.Contains("adfs/services/trust");
        }

        var authType = user.Identity?.AuthenticationType;
        return authType?.Contains("ActiveDirectory") == true;
    }

    /// <summary>
    /// Checks if user is from external portal (CIAM)
    /// </summary>
    public bool IsExternalUser(ClaimsPrincipal user)
    {
        var issuer = user.FindFirst("iss")?.Value;
        
        if (issuer != null)
        {
            return issuer.Contains("ciam-uat.pif.gov.sa") || 
                   issuer.Contains("ciam.pif.gov.sa") ||
                   issuer.Contains("EBPIDENTITYSERVER.COM");
        }

        var authType = user.Identity?.AuthenticationType;
        return authType?.Contains("SsoTwo") == true || 
               authType?.Contains("ExternalPortal") == true;
    }

    /// <summary>
    /// Gets username from token claims (handles both internal and external tokens)
    /// </summary>
    public string GetUsername(ClaimsPrincipal user)
    {
        var username = user.FindFirst("upn")?.Value ??                    // ID token: con-maboulnaga@testpif.local
                      user.FindFirst("unique_name")?.Value ??             // ID token: TESTPIF\con-maboulnaga
                      user.Identity?.Name ??
                      user.FindFirst(ClaimTypes.Name)?.Value ??
                      user.FindFirst("Name")?.Value ??
                      user.FindFirst("preferred_username")?.Value ??
                      user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                      user.FindFirst("sub")?.Value ??
                      user.FindFirst("Email")?.Value;

        // If still no username, try ADFS-specific claims (for access tokens)
        if (string.IsNullOrEmpty(username))
        {
            // Check for idsub (ADFS unique identifier) - extract meaningful part
            var idsub = user.FindFirst("idsub")?.Value;
            if (!string.IsNullOrEmpty(idsub))
            {
                // Try to construct username from First Name and Last Name
                var firstName = user.FindFirst("First Name")?.Value;
                var lastName = user.FindFirst("Last Name")?.Value;
                
                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    // Create username like "con-maboulnaga" from "Mahmoud Aboulnaga"
                    username = $"{firstName.ToLower().Substring(0, Math.Min(3, firstName.Length))}-{lastName.ToLower()}";
                }
                else
                {
                    // Fallback to idsub hash
                    username = idsub;
                }
            }
        }

        // Clean up username
        if (!string.IsNullOrEmpty(username))
        {
            // Remove domain prefix if present (e.g., "TESTPIF\username" -> "username")
            if (username.Contains("\\"))
            {
                username = username.Split('\\')[1];
            }
            
            // Remove email domain if present (e.g., "user@testpif.local" -> "user")
            if (username.Contains("@"))
            {
                username = username.Split('@')[0];
            }
        }

        return username ?? "Unknown";
    }

    /// <summary>
    /// Extracts username from URL path (e.g., /users/con-maboulnaga/permissions)
    /// </summary>
    public string GetUsernameFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return string.Empty;

        // Handle encoded characters
        url = Uri.UnescapeDataString(url);

        // Extract username from URL pattern: /users/{username}/...
        var parts = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var usersIndex = Array.FindIndex(parts, p => p.Equals("users", StringComparison.OrdinalIgnoreCase));
        
        if (usersIndex >= 0 && usersIndex < parts.Length - 1)
        {
            var username = parts[usersIndex + 1];
            
            // Remove email domain if present
            if (username.Contains("@"))
            {
                username = username.Split('@')[0];
            }
            
            return username;
        }

        return string.Empty;
    }
}
