using PartnersHub.ConfigurationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission
{
    public class Permission:Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid ModuleId { get; set; }
        public Module Module { get; set; } = default!;
    }
}
