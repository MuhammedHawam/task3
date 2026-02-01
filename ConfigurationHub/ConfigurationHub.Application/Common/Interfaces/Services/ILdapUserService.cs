using PartnersHub.ConfigurationHub.Application.Common.Models;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;

/// <summary>
/// Service for searching internal users via LDAP (Active Directory)
/// Used by Super Admin to search and assign roles to internal PIF staff
/// </summary>
public interface ILdapUserService
{
    Task<PaginatedList<LdapUser>> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 20);
    Task<LdapUser?> GetUserByUsernameAsync(string? username, string? useremail);

    Task<List<LdapUser>> GetUsersByUsernameAsync(string? username);
}
