using PartnersHub.ConfigurationHub.Domain.Common;

namespace PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

public class Role : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ModuleId { get; set; }
    public Module? Module { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystemRole { get; set; } = true;
    public List<RolePermission> RolePermissions { get; set; } = new();
}
