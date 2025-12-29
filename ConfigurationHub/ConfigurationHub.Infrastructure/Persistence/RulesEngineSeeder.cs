using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using PartnersHub.ConfigurationHub.Domain.Enums;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence;

public static class RulesEngineSeeder
{
    public static async Task SeedRbacDataAsync(ConfigurationHubDbContext context)
    {
        // Check if data already exists
        if (await context.Modules.AnyAsync() || await context.Roles.AnyAsync())
        {
            return;
        }

        // Step 1: Seed Modules
        var globalModule = new Module { Name = "Global", ModuleType = ModuleType.Global, Description = "System-wide administration", IsActive = true };
        var infrabaseModule = new Module { Name = "InfraBase", ModuleType = ModuleType.InfraBase, Description = "Infrastructure asset management", IsActive = true };
        var synergyModule = new Module { Name = "Synergy", ModuleType = ModuleType.Synergy, Description = "Opportunities and success stories", IsActive = true };
        var innovationModule = new Module { Name = "Innovation", ModuleType = ModuleType.Innovation, Description = "Innovation challenges and campaigns", IsActive = true };
        var communityModule = new Module { Name = "Community", ModuleType = ModuleType.Community, Description = "Community management", IsActive = true };

        await context.Modules.AddRangeAsync(new[] { globalModule, infrabaseModule, synergyModule, innovationModule, communityModule });
        await context.SaveChangesAsync();

        // Step 2: Seed Permissions by Module

        // Global Permissions
        var globalPermissions = new[]
        {
            new Permission { Name = "Global.ManageUsers", Description = "Manage users and assign roles", ModuleId = globalModule.Id },
            new Permission { Name = "Global.ManageRoles", Description = "Manage roles and permissions", ModuleId = globalModule.Id },
            new Permission { Name = "Global.ViewAuditLogs", Description = "View system audit logs", ModuleId = globalModule.Id },
            new Permission { Name = "Global.ManageLookups", Description = "Manage system lookups", ModuleId = globalModule.Id },
            new Permission { Name = "Global.ManageSettings", Description = "Manage system settings", ModuleId = globalModule.Id }
        };

        // InfraBase Permissions
        var infrabasePermissions = new[]
        {
            new Permission { Name = "InfraBase.ViewDashboard", Description = "View InfraBase dashboard", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.ViewAssets", Description = "View assets", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.CreateAssets", Description = "Create new assets", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.UpdateAssets", Description = "Update assets", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.DeleteAssets", Description = "Delete assets", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.SubmitAssets", Description = "Submit assets for approval", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.ApproveAssetsPcAdmin", Description = "Approve assets as PC Admin", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.RejectAssetsPcAdmin", Description = "Reject assets as PC Admin", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.ApproveAssetsInfrabaseAdmin", Description = "Approve assets as Infrabase Admin", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.RejectAssetsInfrabaseAdmin", Description = "Reject assets as Infrabase Admin", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.ViewAllCompanyAssets", Description = "View all company assets", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.ExportReports", Description = "Export reports", ModuleId = infrabaseModule.Id },
            new Permission { Name = "InfraBase.ManageAttachments", Description = "Manage asset attachments", ModuleId = infrabaseModule.Id }
        };

        // Synergy Permissions
        var synergyPermissions = new[]
        {
            new Permission { Name = "Synergy.ViewDashboard", Description = "View Synergy dashboard", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.ViewOpportunities", Description = "View opportunities", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.CreateOpportunities", Description = "Create opportunities", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.UpdateOpportunities", Description = "Update opportunities", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.DeleteOpportunities", Description = "Delete opportunities", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.SubmitOpportunities", Description = "Submit opportunities", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.ApproveOpportunities", Description = "Approve opportunities", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.RejectOpportunities", Description = "Reject opportunities", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.ViewSuccessStories", Description = "View success stories", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.CreateSuccessStories", Description = "Create success stories", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.UpdateSuccessStories", Description = "Update success stories", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.ApproveSuccessStories", Description = "Approve success stories", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.RejectSuccessStories", Description = "Reject success stories", ModuleId = synergyModule.Id },
            new Permission { Name = "Synergy.ManageCompanies", Description = "Manage companies", ModuleId = synergyModule.Id }
        };

        // Innovation Permissions
        var innovationPermissions = new[]
        {
            new Permission { Name = "Innovation.ViewChallenges", Description = "View challenges", ModuleId = innovationModule.Id },
            new Permission { Name = "Innovation.CreateChallenges", Description = "Create challenges", ModuleId = innovationModule.Id },
            new Permission { Name = "Innovation.UpdateChallenges", Description = "Update challenges", ModuleId = innovationModule.Id },
            new Permission { Name = "Innovation.DeleteChallenges", Description = "Delete challenges", ModuleId = innovationModule.Id },
            new Permission { Name = "Innovation.SubmitChallenges", Description = "Submit challenges", ModuleId = innovationModule.Id },
            new Permission { Name = "Innovation.ApproveChallenges", Description = "Approve challenges", ModuleId = innovationModule.Id },
            new Permission { Name = "Innovation.RejectChallenges", Description = "Reject challenges", ModuleId = innovationModule.Id },
            new Permission { Name = "Innovation.ReviewChallenges", Description = "Review challenges", ModuleId = innovationModule.Id },
            new Permission { Name = "Innovation.ViewCampaigns", Description = "View campaigns", ModuleId = innovationModule.Id },
            new Permission { Name = "Innovation.CreateCampaigns", Description = "Create campaigns", ModuleId = innovationModule.Id },
            new Permission { Name = "Innovation.ManageCampaigns", Description = "Manage campaigns", ModuleId = innovationModule.Id }
        };

        // Community Permissions
        var communityPermissions = new[]
        {
            new Permission { Name = "Community.ViewMembers", Description = "View community members", ModuleId = communityModule.Id },
            new Permission { Name = "Community.ManageMembers", Description = "Manage community members", ModuleId = communityModule.Id },
            new Permission { Name = "Community.ViewEvents", Description = "View events", ModuleId = communityModule.Id },
            new Permission { Name = "Community.CreateEvents", Description = "Create events", ModuleId = communityModule.Id },
            new Permission { Name = "Community.ManageEvents", Description = "Manage events", ModuleId = communityModule.Id },
            new Permission { Name = "Community.ManageSettings", Description = "Manage community settings", ModuleId = communityModule.Id }
        };

        var allPermissions = globalPermissions
            .Concat(infrabasePermissions)
            .Concat(synergyPermissions)
            .Concat(innovationPermissions)
            .Concat(communityPermissions)
            .ToArray();

        await context.Permissions.AddRangeAsync(allPermissions);
        await context.SaveChangesAsync();

        // Step 3: Seed Roles
        var superAdmin = new Role { Name = "SuperAdmin", Description = "Full system access", ModuleId = globalModule.Id, IsActive = true, IsSystemRole = true };
        var infrabaseAdmin = new Role { Name = "InfrabaseAdmin", Description = "Infrabase module administrator", ModuleId = infrabaseModule.Id, IsActive = true, IsSystemRole = true };
        var assetManager = new Role { Name = "AssetManager", Description = "Synergy asset manager", ModuleId = synergyModule.Id, IsActive = true, IsSystemRole = true };
        var synergyAdmin = new Role { Name = "SynergyAdmin", Description = "Synergy module administrator", ModuleId = synergyModule.Id, IsActive = true, IsSystemRole = true };
        var innovationLeadership = new Role { Name = "InnovationLeadership", Description = "Innovation leadership team", ModuleId = innovationModule.Id, IsActive = true, IsSystemRole = true };
        var sectorLead = new Role { Name = "SectorLead", Description = "Innovation sector lead", ModuleId = innovationModule.Id, IsActive = true, IsSystemRole = true };
        var innovationTeamMember = new Role { Name = "InnovationTeamMember", Description = "Innovation team member", ModuleId = innovationModule.Id, IsActive = true, IsSystemRole = true };
        var communityMenaAdmin = new Role { Name = "CommunityMenaAdmin", Description = "Community MENA administrator", ModuleId = communityModule.Id, IsActive = true, IsSystemRole = true };
        var communityCadAdmin = new Role { Name = "CommunityCadAdmin", Description = "Community CAD administrator", ModuleId = communityModule.Id, IsActive = true, IsSystemRole = true };

        await context.Roles.AddRangeAsync(new[] { superAdmin, infrabaseAdmin, assetManager, synergyAdmin, innovationLeadership, sectorLead, innovationTeamMember, communityMenaAdmin, communityCadAdmin });
        await context.SaveChangesAsync();

        // Step 4: Assign Permissions to Roles

        // SuperAdmin gets all permissions
        var superAdminPermissions = allPermissions.Select(p => new RolePermission
        {
            RoleId = superAdmin.Id,
            PermissionId = p.Id
        }).ToList();

        // InfrabaseAdmin gets all InfraBase permissions
        var infrabaseAdminPerms = infrabasePermissions.Select(p => new RolePermission
        {
            RoleId = infrabaseAdmin.Id,
            PermissionId = p.Id
        }).ToList();

        // AssetManager gets approval permissions for Synergy
        var assetManagerPerms = new[]
        {
            synergyPermissions.First(p => p.Name == "Synergy.ViewDashboard"),
            synergyPermissions.First(p => p.Name == "Synergy.ViewOpportunities"),
            synergyPermissions.First(p => p.Name == "Synergy.ApproveOpportunities"),
            synergyPermissions.First(p => p.Name == "Synergy.RejectOpportunities"),
            synergyPermissions.First(p => p.Name == "Synergy.ViewSuccessStories"),
            synergyPermissions.First(p => p.Name == "Synergy.ApproveSuccessStories"),
            synergyPermissions.First(p => p.Name == "Synergy.RejectSuccessStories")
        }.Select(p => new RolePermission { RoleId = assetManager.Id, PermissionId = p.Id }).ToList();

        // SynergyAdmin gets all Synergy permissions
        var synergyAdminPerms = synergyPermissions.Select(p => new RolePermission
        {
            RoleId = synergyAdmin.Id,
            PermissionId = p.Id
        }).ToList();

        // InnovationLeadership gets approval permissions
        var innovationLeadershipPerms = new[]
        {
            innovationPermissions.First(p => p.Name == "Innovation.ViewChallenges"),
            innovationPermissions.First(p => p.Name == "Innovation.ApproveChallenges"),
            innovationPermissions.First(p => p.Name == "Innovation.RejectChallenges"),
            innovationPermissions.First(p => p.Name == "Innovation.ViewCampaigns"),
            innovationPermissions.First(p => p.Name == "Innovation.ManageCampaigns")
        }.Select(p => new RolePermission { RoleId = innovationLeadership.Id, PermissionId = p.Id }).ToList();

        // SectorLead gets creation and review permissions
        var sectorLeadPerms = new[]
        {
            innovationPermissions.First(p => p.Name == "Innovation.ViewChallenges"),
            innovationPermissions.First(p => p.Name == "Innovation.CreateChallenges"),
            innovationPermissions.First(p => p.Name == "Innovation.UpdateChallenges"),
            innovationPermissions.First(p => p.Name == "Innovation.ReviewChallenges"),
            innovationPermissions.First(p => p.Name == "Innovation.ViewCampaigns")
        }.Select(p => new RolePermission { RoleId = sectorLead.Id, PermissionId = p.Id }).ToList();

        // InnovationTeamMember gets basic permissions
        var innovationTeamMemberPerms = new[]
        {
            innovationPermissions.First(p => p.Name == "Innovation.ViewChallenges"),
            innovationPermissions.First(p => p.Name == "Innovation.CreateChallenges"),
            innovationPermissions.First(p => p.Name == "Innovation.ViewCampaigns")
        }.Select(p => new RolePermission { RoleId = innovationTeamMember.Id, PermissionId = p.Id }).ToList();

        // CommunityMenaAdmin gets all Community permissions
        var communityMenaAdminPerms = communityPermissions.Select(p => new RolePermission
        {
            RoleId = communityMenaAdmin.Id,
            PermissionId = p.Id
        }).ToList();

        // CommunityCadAdmin gets all Community permissions
        var communityCadAdminPerms = communityPermissions.Select(p => new RolePermission
        {
            RoleId = communityCadAdmin.Id,
            PermissionId = p.Id
        }).ToList();

        var allRolePermissions = superAdminPermissions
            .Concat(infrabaseAdminPerms)
            .Concat(assetManagerPerms)
            .Concat(synergyAdminPerms)
            .Concat(innovationLeadershipPerms)
            .Concat(sectorLeadPerms)
            .Concat(innovationTeamMemberPerms)
            .Concat(communityMenaAdminPerms)
            .Concat(communityCadAdminPerms)
            .ToArray();

        await context.RolePermissions.AddRangeAsync(allRolePermissions);
        await context.SaveChangesAsync();
    }
}
