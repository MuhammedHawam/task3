-- Test Database Connectivity and Table Access
USE InfraBaseDb;
GO

PRINT 'Testing InfraBaseDb connectivity...';
PRINT '';

-- 1. Check database connection
IF DB_NAME() = 'InfraBaseDb'
    PRINT '? Connected to InfraBaseDb successfully';
ELSE
BEGIN
    PRINT '? ERROR: Not connected to InfraBaseDb';
    PRINT '   Current database: ' + DB_NAME();
END

PRINT '';

-- 2. Check if tables exist
PRINT 'Checking table structure...';
IF OBJECT_ID('dbo.Assets', 'U') IS NOT NULL
    PRINT '? Assets table exists';
ELSE
    PRINT '? Assets table NOT found - run migrations first!';

IF OBJECT_ID('dbo.AssetCapexDetails', 'U') IS NOT NULL
    PRINT '? AssetCapexDetails table exists';
ELSE
    PRINT '? AssetCapexDetails table NOT found';

IF OBJECT_ID('dbo.AssetOpexDetails', 'U') IS NOT NULL
    PRINT '? AssetOpexDetails table exists';
ELSE
    PRINT '? AssetOpexDetails table NOT found';

IF OBJECT_ID('dbo.AssetAttachments', 'U') IS NOT NULL
    PRINT '? AssetAttachments table exists';
ELSE
    PRINT '? AssetAttachments table NOT found';

IF OBJECT_ID('dbo.AssetHistories', 'U') IS NOT NULL
    PRINT '? AssetHistories table exists';
ELSE
    PRINT '? AssetHistories table NOT found';

PRINT '';

-- 3. Check for existing data
PRINT 'Checking existing data...';
DECLARE @AssetCount INT = (SELECT COUNT(*) FROM Assets);
DECLARE @CapexCount INT = (SELECT COUNT(*) FROM AssetCapexDetails);
DECLARE @OpexCount INT = (SELECT COUNT(*) FROM AssetOpexDetails);

PRINT 'Total Assets: ' + CAST(@AssetCount AS NVARCHAR(10));
PRINT 'Total CAPEX records: ' + CAST(@CapexCount AS NVARCHAR(10));
PRINT 'Total OPEX records: ' + CAST(@OpexCount AS NVARCHAR(10));

PRINT '';

-- 4. Check for RUA AlHaram assets specifically
DECLARE @RUACount INT = (
    SELECT COUNT(*) 
    FROM Assets 
    WHERE CompanyId = '4c4bf721-70e6-f011-a4e0-005056992b12'
);

IF @RUACount > 0
BEGIN
    PRINT '? Found ' + CAST(@RUACount AS NVARCHAR(10)) + ' RUA AlHaram AlMakki assets';
    PRINT '';
    PRINT 'Sample assets:';
    SELECT TOP 5 
        AssetName, 
        LocationCity, 
        Status,
        CreatedAt
    FROM Assets 
    WHERE CompanyId = '4c4bf721-70e6-f011-a4e0-005056992b12'
    ORDER BY CreatedAt DESC;
END
ELSE
BEGIN
    PRINT '??  No RUA AlHaram AlMakki assets found';
    PRINT '   The insert script may have failed or not been run yet';
END

PRINT '';
PRINT '============================================';
PRINT 'Connectivity Test Complete';
PRINT '============================================';

GO
