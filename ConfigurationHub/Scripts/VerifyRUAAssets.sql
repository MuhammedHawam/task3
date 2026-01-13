-- Verify Script Execution
USE InfraBaseDb;
GO

-- Check if any RUA AlHaram assets exist
SELECT TOP 10 
    AssetName, 
    LocationCity, 
    CompanyName,
    Status,
    CreatedAt
FROM Assets 
WHERE CompanyName = 'RUA AlHaram AlMakki'
ORDER BY CreatedAt DESC;

-- If no results, the script didn't commit successfully
-- Check for blocking or incomplete transactions:

-- Check for open transactions
DBCC OPENTRAN;

-- Check for locks
SELECT 
    resource_type,
    resource_database_id,
    request_mode,
    request_status
FROM sys.dm_tran_locks
WHERE resource_database_id = DB_ID('InfraBaseDb');
