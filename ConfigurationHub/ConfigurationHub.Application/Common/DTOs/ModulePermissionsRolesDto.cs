using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Application.Common.DTOs
{
    public class ModulePermissionsRolesDto
    {
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public List<PermissionDto> Permissions { get; set; }
    }

    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string PermissionName { get; set; }
    }
}
