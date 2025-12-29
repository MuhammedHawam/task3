using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Application.Common.Models
{
    
    public class ScimUserListResponse
    {
        public int TotalResults { get; set; }
        public int StartIndex { get; set; }
        public int ItemsPerPage { get; set; }
        public List<ScimResource> Resources { get; set; } = new();
    }

    public class ScimResource
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public ScimName? Name { get; set; }
        public List<string>? Emails { get; set; }

        [JsonPropertyName("urn:scim:wso2:schema")]
        public CustomExtension? CustomExtension { get; set; }
    }

    public class ScimName
    {
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
    }

    public class CustomExtension
    {
        public string? ContactId { get; set; }
        public List<string>? RoleIds { get; set; }
    }

    public class SimpleUser
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string>? RoleId { get; set; }
        public Guid? UserId { get; set; }
    }

    /// <summary>
    /// LDAP User model for internal users (Active Directory via LDAP)
    /// Used for searching and retrieving internal company users for RBAC assignment
    /// </summary>
    public class LdapUser
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DistinguishedName { get; set; } = string.Empty;
    }
}