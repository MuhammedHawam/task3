using PartnersHub.ConfigurationHub.Domain.Common;
using PartnersHub.ConfigurationHub.Domain.Enums;

namespace PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

public class Module : Entity
{
    public string Name { get; set; } = string.Empty;
    public ModuleType ModuleType { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
