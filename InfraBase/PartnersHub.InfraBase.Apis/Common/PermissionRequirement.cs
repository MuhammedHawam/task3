using Microsoft.AspNetCore.Authorization;

namespace PartnersHub.InnovationHub.Apis.Common
{
    public class PermissionRequirement:IAuthorizationRequirement
    {
        public PermissionRequirement(string permission) => Permission = permission;
        public string Permission { get; set; }
    }
}
