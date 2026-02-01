using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence;

/// <summary>
/// Seeds test users with their respective roles for development and testing environments.
/// 
/// Test Users:
/// - TST_PhubSA: Super Admin (Global)
/// - TST_PhubIA: Infrabase Admin (Infrabase)
/// - TST_PhubAM: Asset Manager (Synergy)
/// - TST_PhubSYA: Synergy Admin (Synergy)
/// - TST_PhubIL: Innovation Leadership (Innovation)
/// - TST_PhubSL: Sector Lead (Innovation)
/// - TST_PhubIT: Innovation Team Member (Innovation)
/// </summary>
public static class TestUsersSeeder
{
    /// <summary>
    /// Test user definitions with their corresponding role names
    /// </summary>
    private static readonly (string Username, string RoleName, string ModuleName, string Description)[] TestUsers = new[]
    {
        ("TST_PhubSA", "SuperAdmin", "Global", "Super Admin - Full system access"),
        ("TST_PhubIA", "InfrabaseAdmin", "InfraBase", "Infrabase Admin - Infrastructure asset management"),
        ("TST_PhubAM", "AssetManager", "Synergy", "Asset Manager - Synergy approval workflow"),
        ("TST_PhubSYA", "SynergyAdmin", "Synergy", "Synergy Admin - Full Synergy access"),
        ("TST_PhubIL", "InnovationLeadership", "Innovation", "Innovation Leadership - Approval authority"),
        ("TST_PhubSL", "SectorLead", "Innovation", "Sector Lead - Challenge creation and review"),
        ("TST_PhubIT", "InnovationTeamMember", "Innovation", "Innovation Team Member - Basic access")
    };

    /// <summary>
    /// Seeds test users with their roles. Call this after RulesEngineSeeder.
    /// </summary>
    public static async Task SeedTestUsersAsync(
        ConfigurationHubDbContext context,
        IConfiguration configuration,
        ILogger logger)
    {
        try
        {
            var seedTestUsers = configuration.GetValue<bool>("SeedTestUsers", true);
            if (!seedTestUsers)
            {
                logger.LogInformation("Test users seeding is disabled in configuration");
                return;
            }

            var domain = configuration["DefaultSuperAdmin:Domain"] ?? "testpif";
            
            logger.LogInformation("Starting test users seeding...");

            foreach (var (username, roleName, moduleName, description) in TestUsers)
            {
                await AssignRoleToUserAsync(context, username, domain, roleName, moduleName, description, logger);
            }

            logger.LogInformation("Test users seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding test users");
        }
    }

    private static async Task AssignRoleToUserAsync(
        ConfigurationHubDbContext context,
        string username,
        string domain,
        string roleName,
        string moduleName,
        string description,
        ILogger logger)
    {
        try
        {
            // Get the role
            var role = await context.Roles
                .Include(r => r.Module)
                .FirstOrDefaultAsync(r => r.Name == roleName && r.IsSystemRole);

            if (role == null)
            {
                logger.LogWarning("Role {RoleName} not found. Skipping user {Username}", roleName, username);
                return;
            }

            // Get the module
            var module = await context.Modules.FirstOrDefaultAsync(m => m.Name == moduleName);
            if (module == null)
            {
                logger.LogWarning("Module {ModuleName} not found. Skipping user {Username}", moduleName, username);
                return;
            }

            // Check if assignment already exists
            var existingAssignment = await context.UserRoles
                .AnyAsync(ur => ur.UserId == username && ur.RoleId == role.Id && ur.ModuleId == module.Id);

            if (!existingAssignment)
            {
                var userRole = new UserRole
                {
                    UserId = username,
                    RoleId = role.Id,
                    ModuleId = module.Id,
                    UserName= username,
                    AssignedBy = "System",
                    AssignedAt = DateTime.UtcNow
                };

                await context.UserRoles.AddAsync(userRole);
                await context.SaveChangesAsync();

                logger.LogInformation("Assigned role {RoleName} to user {Username} ({Description})", 
                    roleName, username, description);

                // Assign permissions based on role
                await AssignRolePermissionsToUserAsync(context, username, role.Id, module.Id, logger);
            }
            else
            {
                logger.LogInformation("User {Username} already has role {RoleName}", username, roleName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning role {RoleName} to user {Username}", roleName, username);
        }
    }

    private static async Task AssignRolePermissionsToUserAsync(
        ConfigurationHubDbContext context,
        string userId,
        Guid roleId,
        Guid moduleId,
        ILogger logger)
    {
        try
        {
            // Get all permissions assigned to this role
            var rolePermissions = await context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            if (!rolePermissions.Any())
            {
                logger.LogWarning("No permissions found for role {RoleId}", roleId);
                return;
            }

            // Get existing user permissions
            var existingPermissions = await context.UserPermissions
                .Where(up => up.UserId == userId)
                .Select(up => up.PermissionId)
                .ToListAsync();

            var permissionsToAdd = new List<UserPermission>();

            foreach (var permissionId in rolePermissions)
            {
                if (!existingPermissions.Contains(permissionId))
                {
                    var permission = await context.Permissions.FindAsync(permissionId);
                    if (permission != null)
                    {
                        permissionsToAdd.Add(new UserPermission
                        {
                            UserId = userId,
                            PermissionId = permissionId,
                            ModuleId = permission.ModuleId
                        });
                    }
                }
            }

            if (permissionsToAdd.Any())
            {
                await context.UserPermissions.AddRangeAsync(permissionsToAdd);
                await context.SaveChangesAsync();
                logger.LogInformation("Assigned {Count} permissions to user {UserId}", 
                    permissionsToAdd.Count, userId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning permissions to user {UserId}", userId);
        }
    }

    /// <summary>
    /// Gets the list of test usernames for reference
    /// </summary>
    public static IEnumerable<string> GetTestUsernames() => TestUsers.Select(u => u.Username);

    /// <summary>
    /// Gets the test user info by username
    /// </summary>
    public static (string Username, string RoleName, string ModuleName, string Description)? GetTestUserInfo(string username)
        => TestUsers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
}
