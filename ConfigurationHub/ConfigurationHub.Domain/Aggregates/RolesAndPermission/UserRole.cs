namespace PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

public class UserRole
{
    public string UserId { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public Guid ModuleId { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public Role Role { get; set; } = default!;
    public Module Module { get; set; } = default!;
}
