-- =============================================
-- Partners Hub - Test Users Role Assignment Script
-- =============================================
-- This script assigns roles to test users for development and testing.
-- Run this AFTER the RBAC seed data has been created.
--
-- Test Users:
-- TST_PhubSA  - Super Admin (Global)
-- TST_PhubIA  - Infrabase Admin (Infrabase)
-- TST_PhubAM  - Asset Manager (Synergy)
-- TST_PhubSYA - Synergy Admin (Synergy)
-- TST_PhubIL  - Innovation Leadership (Innovation)
-- TST_PhubSL  - Sector Lead (Innovation)
-- TST_PhubIT  - Innovation Team Member (Innovation)
-- =============================================

USE ConfigurationHubDB;
GO

-- =============================================
-- STEP 1: Verify Modules and Roles exist
-- =============================================
PRINT 'Verifying modules and roles exist...';

IF NOT EXISTS (SELECT 1 FROM Modules WHERE Name = 'Global')
BEGIN
    RAISERROR('Global module not found. Please run RulesEngineSeeder first.', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'SuperAdmin')
BEGIN
    RAISERROR('SuperAdmin role not found. Please run RulesEngineSeeder first.', 16, 1);
    RETURN;
END

PRINT 'Modules and roles verified.';

-- =============================================
-- STEP 2: Declare Variables for Module and Role IDs
-- =============================================
DECLARE @GlobalModuleId UNIQUEIDENTIFIER;
DECLARE @InfraBaseModuleId UNIQUEIDENTIFIER;
DECLARE @SynergyModuleId UNIQUEIDENTIFIER;
DECLARE @InnovationModuleId UNIQUEIDENTIFIER;

DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER;
DECLARE @InfrabaseAdminRoleId UNIQUEIDENTIFIER;
DECLARE @AssetManagerRoleId UNIQUEIDENTIFIER;
DECLARE @SynergyAdminRoleId UNIQUEIDENTIFIER;
DECLARE @InnovationLeadershipRoleId UNIQUEIDENTIFIER;
DECLARE @SectorLeadRoleId UNIQUEIDENTIFIER;
DECLARE @InnovationTeamMemberRoleId UNIQUEIDENTIFIER;

-- Get Module IDs
SELECT @GlobalModuleId = Id FROM Modules WHERE Name = 'Global';
SELECT @InfraBaseModuleId = Id FROM Modules WHERE Name = 'InfraBase';
SELECT @SynergyModuleId = Id FROM Modules WHERE Name = 'Synergy';
SELECT @InnovationModuleId = Id FROM Modules WHERE Name = 'Innovation';

-- Get Role IDs
SELECT @SuperAdminRoleId = Id FROM Roles WHERE Name = 'SuperAdmin' AND IsSystemRole = 1;
SELECT @InfrabaseAdminRoleId = Id FROM Roles WHERE Name = 'InfrabaseAdmin' AND IsSystemRole = 1;
SELECT @AssetManagerRoleId = Id FROM Roles WHERE Name = 'AssetManager' AND IsSystemRole = 1;
SELECT @SynergyAdminRoleId = Id FROM Roles WHERE Name = 'SynergyAdmin' AND IsSystemRole = 1;
SELECT @InnovationLeadershipRoleId = Id FROM Roles WHERE Name = 'InnovationLeadership' AND IsSystemRole = 1;
SELECT @SectorLeadRoleId = Id FROM Roles WHERE Name = 'SectorLead' AND IsSystemRole = 1;
SELECT @InnovationTeamMemberRoleId = Id FROM Roles WHERE Name = 'InnovationTeamMember' AND IsSystemRole = 1;

-- Print IDs for verification
PRINT 'Module IDs:';
PRINT 'Global: ' + CAST(@GlobalModuleId AS NVARCHAR(50));
PRINT 'InfraBase: ' + CAST(@InfraBaseModuleId AS NVARCHAR(50));
PRINT 'Synergy: ' + CAST(@SynergyModuleId AS NVARCHAR(50));
PRINT 'Innovation: ' + CAST(@InnovationModuleId AS NVARCHAR(50));

PRINT '';
PRINT 'Role IDs:';
PRINT 'SuperAdmin: ' + CAST(@SuperAdminRoleId AS NVARCHAR(50));
PRINT 'InfrabaseAdmin: ' + CAST(@InfrabaseAdminRoleId AS NVARCHAR(50));
PRINT 'AssetManager: ' + CAST(@AssetManagerRoleId AS NVARCHAR(50));
PRINT 'SynergyAdmin: ' + CAST(@SynergyAdminRoleId AS NVARCHAR(50));
PRINT 'InnovationLeadership: ' + CAST(@InnovationLeadershipRoleId AS NVARCHAR(50));
PRINT 'SectorLead: ' + CAST(@SectorLeadRoleId AS NVARCHAR(50));
PRINT 'InnovationTeamMember: ' + CAST(@InnovationTeamMemberRoleId AS NVARCHAR(50));

-- =============================================
-- STEP 3: Assign Roles to Test Users
-- =============================================
PRINT '';
PRINT 'Assigning roles to test users...';

-- TST_PhubSA - Super Admin
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = 'TST_PhubSA' AND RoleId = @SuperAdminRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, ModuleId, AssignedBy, AssignedAt)
    VALUES ('TST_PhubSA', @SuperAdminRoleId, @GlobalModuleId, 'System', GETUTCDATE());
    PRINT 'Assigned SuperAdmin role to TST_PhubSA';
END
ELSE
    PRINT 'TST_PhubSA already has SuperAdmin role';

-- TST_PhubIA - Infrabase Admin
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = 'TST_PhubIA' AND RoleId = @InfrabaseAdminRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, ModuleId, AssignedBy, AssignedAt)
    VALUES ('TST_PhubIA', @InfrabaseAdminRoleId, @InfraBaseModuleId, 'System', GETUTCDATE());
    PRINT 'Assigned InfrabaseAdmin role to TST_PhubIA';
END
ELSE
    PRINT 'TST_PhubIA already has InfrabaseAdmin role';

-- TST_PhubAM - Asset Manager
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = 'TST_PhubAM' AND RoleId = @AssetManagerRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, ModuleId, AssignedBy, AssignedAt)
    VALUES ('TST_PhubAM', @AssetManagerRoleId, @SynergyModuleId, 'System', GETUTCDATE());
    PRINT 'Assigned AssetManager role to TST_PhubAM';
END
ELSE
    PRINT 'TST_PhubAM already has AssetManager role';

-- TST_PhubSYA - Synergy Admin
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = 'TST_PhubSYA' AND RoleId = @SynergyAdminRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, ModuleId, AssignedBy, AssignedAt)
    VALUES ('TST_PhubSYA', @SynergyAdminRoleId, @SynergyModuleId, 'System', GETUTCDATE());
    PRINT 'Assigned SynergyAdmin role to TST_PhubSYA';
END
ELSE
    PRINT 'TST_PhubSYA already has SynergyAdmin role';

-- TST_PhubIL - Innovation Leadership
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = 'TST_PhubIL' AND RoleId = @InnovationLeadershipRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, ModuleId, AssignedBy, AssignedAt)
    VALUES ('TST_PhubIL', @InnovationLeadershipRoleId, @InnovationModuleId, 'System', GETUTCDATE());
    PRINT 'Assigned InnovationLeadership role to TST_PhubIL';
END
ELSE
    PRINT 'TST_PhubIL already has InnovationLeadership role';

-- TST_PhubSL - Sector Lead
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = 'TST_PhubSL' AND RoleId = @SectorLeadRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, ModuleId, AssignedBy, AssignedAt)
    VALUES ('TST_PhubSL', @SectorLeadRoleId, @InnovationModuleId, 'System', GETUTCDATE());
    PRINT 'Assigned SectorLead role to TST_PhubSL';
END
ELSE
    PRINT 'TST_PhubSL already has SectorLead role';

-- TST_PhubIT - Innovation Team Member
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = 'TST_PhubIT' AND RoleId = @InnovationTeamMemberRoleId)
BEGIN
    INSERT INTO UserRoles (UserId, RoleId, ModuleId, AssignedBy, AssignedAt)
    VALUES ('TST_PhubIT', @InnovationTeamMemberRoleId, @InnovationModuleId, 'System', GETUTCDATE());
    PRINT 'Assigned InnovationTeamMember role to TST_PhubIT';
END
ELSE
    PRINT 'TST_PhubIT already has InnovationTeamMember role';

-- =============================================
-- STEP 4: Assign Permissions to Test Users
-- =============================================
PRINT '';
PRINT 'Assigning permissions to test users...';

-- Assign all permissions to SuperAdmin (TST_PhubSA)
INSERT INTO UserPermissions (UserId, PermissionId, ModuleId)
SELECT 'TST_PhubSA', p.Id, p.ModuleId
FROM Permissions p
WHERE NOT EXISTS (
    SELECT 1 FROM UserPermissions up 
    WHERE up.UserId = 'TST_PhubSA' AND up.PermissionId = p.Id
);
PRINT 'Assigned all permissions to TST_PhubSA (SuperAdmin)';

-- Assign role-based permissions to other users
-- TST_PhubIA - Infrabase Admin permissions
INSERT INTO UserPermissions (UserId, PermissionId, ModuleId)
SELECT 'TST_PhubIA', p.Id, p.ModuleId
FROM Permissions p
INNER JOIN RolePermissions rp ON rp.PermissionId = p.Id
WHERE rp.RoleId = @InfrabaseAdminRoleId
AND NOT EXISTS (
    SELECT 1 FROM UserPermissions up 
    WHERE up.UserId = 'TST_PhubIA' AND up.PermissionId = p.Id
);
PRINT 'Assigned InfrabaseAdmin permissions to TST_PhubIA';

-- TST_PhubAM - Asset Manager permissions
INSERT INTO UserPermissions (UserId, PermissionId, ModuleId)
SELECT 'TST_PhubAM', p.Id, p.ModuleId
FROM Permissions p
INNER JOIN RolePermissions rp ON rp.PermissionId = p.Id
WHERE rp.RoleId = @AssetManagerRoleId
AND NOT EXISTS (
    SELECT 1 FROM UserPermissions up 
    WHERE up.UserId = 'TST_PhubAM' AND up.PermissionId = p.Id
);
PRINT 'Assigned AssetManager permissions to TST_PhubAM';

-- TST_PhubSYA - Synergy Admin permissions
INSERT INTO UserPermissions (UserId, PermissionId, ModuleId)
SELECT 'TST_PhubSYA', p.Id, p.ModuleId
FROM Permissions p
INNER JOIN RolePermissions rp ON rp.PermissionId = p.Id
WHERE rp.RoleId = @SynergyAdminRoleId
AND NOT EXISTS (
    SELECT 1 FROM UserPermissions up 
    WHERE up.UserId = 'TST_PhubSYA' AND up.PermissionId = p.Id
);
PRINT 'Assigned SynergyAdmin permissions to TST_PhubSYA';

-- TST_PhubIL - Innovation Leadership permissions
INSERT INTO UserPermissions (UserId, PermissionId, ModuleId)
SELECT 'TST_PhubIL', p.Id, p.ModuleId
FROM Permissions p
INNER JOIN RolePermissions rp ON rp.PermissionId = p.Id
WHERE rp.RoleId = @InnovationLeadershipRoleId
AND NOT EXISTS (
    SELECT 1 FROM UserPermissions up 
    WHERE up.UserId = 'TST_PhubIL' AND up.PermissionId = p.Id
);
PRINT 'Assigned InnovationLeadership permissions to TST_PhubIL';

-- TST_PhubSL - Sector Lead permissions
INSERT INTO UserPermissions (UserId, PermissionId, ModuleId)
SELECT 'TST_PhubSL', p.Id, p.ModuleId
FROM Permissions p
INNER JOIN RolePermissions rp ON rp.PermissionId = p.Id
WHERE rp.RoleId = @SectorLeadRoleId
AND NOT EXISTS (
    SELECT 1 FROM UserPermissions up 
    WHERE up.UserId = 'TST_PhubSL' AND up.PermissionId = p.Id
);
PRINT 'Assigned SectorLead permissions to TST_PhubSL';

-- TST_PhubIT - Innovation Team Member permissions
INSERT INTO UserPermissions (UserId, PermissionId, ModuleId)
SELECT 'TST_PhubIT', p.Id, p.ModuleId
FROM Permissions p
INNER JOIN RolePermissions rp ON rp.PermissionId = p.Id
WHERE rp.RoleId = @InnovationTeamMemberRoleId
AND NOT EXISTS (
    SELECT 1 FROM UserPermissions up 
    WHERE up.UserId = 'TST_PhubIT' AND up.PermissionId = p.Id
);
PRINT 'Assigned InnovationTeamMember permissions to TST_PhubIT';

-- =============================================
-- STEP 5: Verification Query
-- =============================================
PRINT '';
PRINT 'Verification - Test Users and their Roles:';
PRINT '==========================================';

SELECT 
    ur.UserId,
    r.Name AS RoleName,
    m.Name AS ModuleName,
    r.Description AS RoleDescription,
    ur.AssignedAt,
    (SELECT COUNT(*) FROM UserPermissions up WHERE up.UserId = ur.UserId) AS PermissionCount
FROM UserRoles ur
INNER JOIN Roles r ON ur.RoleId = r.Id
INNER JOIN Modules m ON ur.ModuleId = m.Id
WHERE ur.UserId IN ('TST_PhubSA', 'TST_PhubIA', 'TST_PhubAM', 'TST_PhubSYA', 'TST_PhubIL', 'TST_PhubSL', 'TST_PhubIT')
ORDER BY ur.UserId;

PRINT '';
PRINT 'Test users seeding completed successfully!';
GO
