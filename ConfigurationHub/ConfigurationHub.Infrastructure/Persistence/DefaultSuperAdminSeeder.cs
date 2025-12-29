using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartnersHub.ConfigurationHub.Infrastructure.Persistence;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence;

public static class DefaultSuperAdminSeeder
{
    public static async Task AssignDefaultSuperAdminAsync(
        ConfigurationHubDbContext context, 
        IConfiguration configuration,
        ILogger logger)
    {
        try
        {
            var assignOnStartup = configuration.GetValue<bool>("DefaultSuperAdmin:AssignRoleOnStartup");
            if (!assignOnStartup)
            {
                logger.LogInformation("Default super admin assignment is disabled in configuration");
                return;
            }

            var username = configuration["DefaultSuperAdmin:Username"];
            var domain = configuration["DefaultSuperAdmin:Domain"];
            
            if (string.IsNullOrWhiteSpace(username))
            {
                logger.LogWarning("Default super admin username not configured");
                return;
            }

            var fullUsername = !string.IsNullOrWhiteSpace(domain) 
                ? $"{username}" 
                : username;

            logger.LogInformation("Checking default super admin assignment for user: {Username}", fullUsername);

            var superAdminRole = await context.Roles
                .Include(r => r.Module)
                .FirstOrDefaultAsync(r => r.Name == "SuperAdmin" && r.IsSystemRole);

            if (superAdminRole == null)
            {
                logger.LogWarning("SuperAdmin role not found in database. Run RBAC seeder first.");
                return;
            }

            // Get Global module (SuperAdmin's module)
            var globalModule = superAdminRole.Module ?? await context.Modules
                .FirstOrDefaultAsync(m => m.Name == "Global");

            if (globalModule == null)
            {
                logger.LogWarning("Global module not found in database");
                return;
            }

            // Check if user already has SuperAdmin role
            var existingAssignment = await context.UserRoles
                .AnyAsync(ur => ur.UserId == fullUsername 
                    && ur.RoleId == superAdminRole.Id 
                    && ur.ModuleId == globalModule.Id);

            if (!existingAssignment)
            {
                // Assign SuperAdmin role
                var userRole = new Domain.Aggregates.RolesAndPermission.UserRole
                {
                    UserId = fullUsername,
                    RoleId = superAdminRole.Id,
                    ModuleId = globalModule.Id,
                    AssignedBy = "System",
                    AssignedAt = DateTime.UtcNow
                };

                await context.UserRoles.AddAsync(userRole);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully assigned SuperAdmin role to user: {Username}", fullUsername);
            }
            else
            {
                logger.LogInformation("User {Username} already has SuperAdmin role", fullUsername);
            }

            await AssignAllPermissionsToUserAsync(context, username, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning default super admin role");
            // Don't throw - this shouldn't prevent application startup
        }
    }

    private static async Task AssignAllPermissionsToUserAsync(
        ConfigurationHubDbContext context,
        string userId,
        ILogger logger)
    {
        try
        {
            var allPermissions = await context.Permissions
                .Include(p => p.Module)
                .ToListAsync();

            if (!allPermissions.Any())
            {
                logger.LogWarning("No permissions found in database");
                return;
            }

            var existingUserPermissions = await context.UserPermissions
                .Where(up => up.UserId == userId)
                .Select(up => new { up.PermissionId, up.ModuleId })
                .ToListAsync();

            var permissionsToAdd = new List<Domain.Aggregates.RolesAndPermission.UserPermission>();

            foreach (var permission in allPermissions)
            {
                var alreadyExists = existingUserPermissions.Any(ep => 
                    ep.PermissionId == permission.Id && 
                    ep.ModuleId == permission.ModuleId);

                if (!alreadyExists)
                {
                    permissionsToAdd.Add(new Domain.Aggregates.RolesAndPermission.UserPermission
                    {
                        UserId = userId,
                        PermissionId = permission.Id,
                        ModuleId = permission.ModuleId
                    });
                }
            }

            if (permissionsToAdd.Any())
            {
                await context.UserPermissions.AddRangeAsync(permissionsToAdd);
                await context.SaveChangesAsync();
                logger.LogInformation("Successfully assigned {Count} permissions to user: {Username}", 
                    permissionsToAdd.Count, userId);
            }
            else
            {
                logger.LogInformation("User {Username} already has all permissions", userId);
            }

            logger.LogInformation("Total permissions in system: {TotalCount}, User now has all permissions", 
                allPermissions.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning all permissions to user {UserId}", userId);
        }
    }
}
