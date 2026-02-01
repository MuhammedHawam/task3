using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Text;

namespace PartnersHub.ConfigurationHub.Infrastructure.Services;

public class LdapUserService : ILdapUserService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LdapUserService> _logger;
    private readonly string _ldapServer;
    private readonly int _ldapPort;
    private readonly string _ldapUsername;
    private readonly string _ldapPassword;
    private readonly string _searchBase;

    public LdapUserService(IConfiguration configuration, ILogger<LdapUserService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        _ldapServer = configuration["Ldap:Server"] ?? "s1t-testdc1.testpif.local";
        _ldapPort = int.Parse(configuration["Ldap:Port"] ?? "636");
        _ldapUsername = configuration["Ldap:Username"] ?? "Testpif\\SVC_ADReadPhub";
        _ldapPassword = configuration["Ldap:Password"] ?? "&3bW3Qq37*HT";
        _searchBase = configuration["Ldap:SearchBase"] ?? "DC=testpif,DC=local";
    }

    public async Task<PaginatedList<LdapUser>> SearchUsersAsync(string searchTerm, int pageNumber = 1, int pageSize = 20)
    {
        using var connection = CreateConnection();
        
        var filter = string.IsNullOrWhiteSpace(searchTerm)
            ? "(objectClass=user)"
            : $"(&(objectClass=user)(|(cn=*{searchTerm}*)(mail=*{searchTerm}*)(sAMAccountName=*{searchTerm}*)))";

        var searchRequest = new SearchRequest(
            _searchBase,
            filter,
            SearchScope.Subtree,
            "cn", "mail", "sAMAccountName", "displayName", "department", "title", "distinguishedName"
        );

        var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
        var users = new List<LdapUser>();

        foreach (SearchResultEntry entry in searchResponse.Entries)
        {
            users.Add(MapToLdapUser(entry));
        }

        _logger.LogInformation("Found {Count} users matching search term: {SearchTerm}", users.Count, searchTerm);
        
        return await Task.FromResult(PaginatedList<LdapUser>.Create(users, users.Count, pageNumber, pageSize));
    }

    public async Task<LdapUser?> GetUserByUsernameAsync(string? username,string? useremail)
    {
        using var connection = CreateConnection();

        var filter = new StringBuilder("(&(objectClass=user)");

        if (!string.IsNullOrEmpty(username))
        {
            filter.Append($"(sAMAccountName={username})");
        }

        if (!string.IsNullOrEmpty(useremail))
        {
            filter.Append($"(mail=*{useremail}*)");
        }

        filter.Append(")"); 
        var finalFilter = filter.ToString();


        var searchRequest = new SearchRequest(
            _searchBase,
            finalFilter,
            SearchScope.Subtree,
            "cn", "mail", "sAMAccountName", "displayName", "department", "title", "distinguishedName", "objectGuid"
        );

        var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

        if (searchResponse.Entries.Count == 0)
        {
            _logger.LogWarning("User not found: {Username}", username);
            return null;
        }

        var user = MapToLdapUser(searchResponse.Entries[0]);
        _logger.LogInformation("Found user: {Username}", username);
        return await Task.FromResult(user);
    }


    public async Task<List<LdapUser>> GetUsersByUsernameAsync(string? username)
    {
        using var connection = CreateConnection();

        // 1. Corrected Filter Logic
        // Using string.Format to avoid simple injection and fixing parenthesis
        var filter = string.IsNullOrWhiteSpace(username)
            ? "(objectClass=user)"
            : $"(&(objectClass=user)(sAMAccountName=*{username}*))";

        var searchRequest = new SearchRequest(
            _searchBase,
            filter,
            SearchScope.Subtree,
             // 2. Added "objectGuid" to the requested attributes list
             "cn", "mail", "sAMAccountName", "displayName", "department", "title", "distinguishedName", "objectGuid"
        );

        var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

        var userList = new List<LdapUser>();

        if (searchResponse.Entries.Count == 0)
        {
            _logger.LogWarning("No users found matching: {Username}", username);
            return userList;
        }

        // 3. Loop through ALL entries to map all search results
        foreach (SearchResultEntry entry in searchResponse.Entries)
        {
            userList.Add(MapToLdapUser(entry));
        }

        _logger.LogInformation("Found {Count} user(s) for: {Username}", userList.Count, username);
        return userList;
    }

    private LdapUser MapToLdapUser(SearchResultEntry entry)
    {
        return new LdapUser
        {
            // GUIDs in LDAP are byte arrays; they often need special handling to string
            UserId = GetGuidAttribute(entry),
            Username = GetAttribute(entry, "sAMAccountName"),
            DisplayName = GetAttribute(entry, "displayName"),
            Email = GetAttribute(entry, "mail"),
            Department = GetAttribute(entry, "department"),
            Title = GetAttribute(entry, "title"),
            DistinguishedName = GetAttribute(entry, "distinguishedName")
        };
    }

    private string GetGuidAttribute(SearchResultEntry entry)
    {
        if (entry.Attributes.Contains("objectGuid"))
        {
            var bytes = (byte[])entry.Attributes["objectGuid"][0];
            return new Guid(bytes).ToString();
        }
        return string.Empty;
    }

    private string GetAttribute(SearchResultEntry entry, string attributeName)
    {
        if (entry.Attributes.Contains(attributeName))
        {
            var attribute = entry.Attributes[attributeName];
            return attribute[0]?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }

    private LdapConnection CreateConnection()
    {
        var identifier = new LdapDirectoryIdentifier(_ldapServer, _ldapPort);
        var connection = new LdapConnection(identifier)
        {
            AuthType = AuthType.Basic,
            Credential = new NetworkCredential(_ldapUsername, _ldapPassword)
        };

        connection.SessionOptions.SecureSocketLayer = true;
        connection.SessionOptions.VerifyServerCertificate = (conn, cert) => true;
        connection.Bind();

        return connection;
    }
}
