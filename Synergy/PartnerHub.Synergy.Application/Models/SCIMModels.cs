using System.Text.Json.Serialization;

namespace PartnersHub.Synergy.Application.Common.Models
{
    public class SimpleUser
    {
        public string Name { get; set; }
        public List<string> RoleId { get; set; }
        public string Email { get; set; }
    }

    public class ScimUserListResponse
    {
        public int TotalResults { get; set; }
        public List<ScimUserResource> Resources { get; set; }
    }

    public class ScimUserResource
    {
        public List<string> Emails { get; set; }

        public ScimUserName Name { get; set; }

        [JsonPropertyName("urn:scim:schemas:extension:custom:User")]
        public ScimCustomExtension CustomExtension { get; set; }
    }

    public class ScimUserName
    {
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
    }

    public class ScimCustomExtension
    {
        [JsonPropertyName("RoleID")]
        public List<string> RoleIds { get; set; }
    }
}