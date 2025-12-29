using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission
{
    public class UserPermission
    {
        public Guid PermissionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid ModuleId { get; set; }
        public Permission Permission { get; set; } = default!;
        public Module Module { get; set; } = default!;
    }
}
