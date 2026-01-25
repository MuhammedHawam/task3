using PartnersHub.ConfigurationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission
{
    public class RegisteredCompany : AuditableEntity
    {
        public string CompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid ModuleId { get; set; }
        public Module Module { get; set; }
        public string SectorId { get; set; }
        public string SectorName { get; set; }
    }
}
