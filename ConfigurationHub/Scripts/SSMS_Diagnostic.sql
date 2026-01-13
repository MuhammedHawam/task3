-- =============================================
-- SSMS Connection Diagnostic Script
-- Save this as a .SQL file and try to execute it
-- =============================================

-- Test 1: Basic SELECT statement
PRINT '=== Test 1: Basic Query ===';
SELECT GETDATE() AS CurrentDateTime, @@VERSION AS SQLServerVersion;
GO

-- Test 2: Check current connection
PRINT '';
PRINT '=== Test 2: Connection Info ===';
SELECT 
    DB_NAME() AS CurrentDatabase,
    SUSER_NAME() AS CurrentUser,
    @@SERVERNAME AS ServerName,
    @@SPID AS SessionID;
GO

-- Test 3: List all databases
PRINT '';
PRINT '=== Test 3: Available Databases ===';
SELECT name, state_desc, recovery_model_desc 
FROM sys.databases 
WHERE name IN ('master', 'ConfigurationHubDb', 'InfraBaseDb', 'SynergyDb', 'InnovationHubDb')
ORDER BY name;
GO

-- Test 4: Check for blocking
PRINT '';
PRINT '=== Test 4: Blocking Sessions ===';
SELECT 
    blocking_session_id,
    session_id,
    wait_type,
    wait_time,
    wait_resource
FROM sys.dm_exec_requests
WHERE blocking_session_id <> 0;
GO

PRINT '';
PRINT '==============================================';
PRINT 'If you can see this, SSMS is working correctly';
PRINT '==============================================';
