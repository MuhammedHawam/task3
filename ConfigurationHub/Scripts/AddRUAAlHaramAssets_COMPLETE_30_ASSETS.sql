-- =============================================
-- Add RUA AlHaram AlMakki Assets to InfraBase
-- =============================================
-- COMPLETE VERSION - All 30 Assets
-- Uses actual Unit GUIDs from database
-- Matches schema from 20251231211059_InitialCreate
-- Enums stored as STRINGS (PreTender, Greenfield, etc.)
-- =============================================

USE InfraBaseDb;
GO

SET NOCOUNT ON;

PRINT '============================================';
PRINT 'Adding RUA AlHaram AlMakki Assets - FULL';
PRINT '============================================';
PRINT '';

BEGIN TRANSACTION;

BEGIN TRY
    -- =============================================
    -- Configuration
    -- =============================================
    DECLARE @CompanyId UNIQUEIDENTIFIER = '4c4bf721-70e6-f011-a4e0-005056992b12'; 
    DECLARE @CompanyName NVARCHAR(255) = 'RUA AlHaram AlMakki';
    DECLARE @CreatedBy NVARCHAR(450) = 'system@infrabase.com';
    DECLARE @LocationCity NVARCHAR(100) = 'Makkah';
    
    -- =============================================
    -- Sector IDs
    -- =============================================
    DECLARE @EnablingWorksSectorId UNIQUEIDENTIFIER = 'CD55DF43-B734-4B15-A985-4E4DEAA9EA9D';
    DECLARE @WaterWasteSectorId UNIQUEIDENTIFIER = '961DDCF9-7026-4E45-A04A-4E491B15CD8C';
    
    -- =============================================
    -- SubSector IDs
    -- =============================================
    DECLARE @EarthworkSubSectorId UNIQUEIDENTIFIER = 'DA2837E0-C1C3-4803-BF10-B2D533DC3C9A';
    DECLARE @EnablingOtherSubSectorId UNIQUEIDENTIFIER = '74812B8E-1723-459D-8C3B-30889DD6BC11';
    DECLARE @IrrigationSubSectorId UNIQUEIDENTIFIER = 'E8B15C79-2B08-4D30-B89D-1087B3477DF4';
    DECLARE @PotableWaterSubSectorId UNIQUEIDENTIFIER = '6C6F53F3-8A2B-4B01-B0D6-4D10D8617BBA';
    DECLARE @SewageSubSectorId UNIQUEIDENTIFIER = '9F73DBA1-E7FA-4856-9B05-269C044C817C';
    DECLARE @SolidWasteSubSectorId UNIQUEIDENTIFIER = '576AF603-62BA-4520-9864-7965AB4E8FF7';
    DECLARE @StormWaterSubSectorId UNIQUEIDENTIFIER = 'E63DFE9C-7277-4061-A5CF-9D21EDDAAB90';
    
    -- =============================================
    -- Asset Type IDs
    -- =============================================
    DECLARE @EarthworkAssetTypeId UNIQUEIDENTIFIER = 'A79CF2F8-154F-4518-A709-F757AEC83540';
    DECLARE @IrrigationDistNetworkAssetTypeId UNIQUEIDENTIFIER = 'F1C8A57A-6D16-4EAE-9272-626430DF922C';
    DECLARE @PotableTransNetworkAssetTypeId UNIQUEIDENTIFIER = 'E854C088-AB1E-40AC-B6F5-ED7F411F092F';
    DECLARE @PotableDistNetworkAssetTypeId UNIQUEIDENTIFIER = '70C4BE77-4DBD-43A6-BD98-EBC6A26A988B';
    DECLARE @SewagePlantAssetTypeId UNIQUEIDENTIFIER = '6989C039-B3D5-44A9-A4F0-CCFE066F4532';
    DECLARE @SolidWasteTransferAssetTypeId UNIQUEIDENTIFIER = 'B66706B1-8D19-48CC-ABD5-D196FBF795A9';
    DECLARE @AutomatedWasteAssetTypeId UNIQUEIDENTIFIER = '3BBE66B5-7344-4EB4-B59A-339C117CCE3A';
    DECLARE @StormWaterPondAssetTypeId UNIQUEIDENTIFIER = '567CBEC5-A72D-4A01-8FFD-BBABD0D63468';
    
    -- =============================================
    -- Unit of Measurement IDs - ACTUAL DATABASE GUIDS
    -- =============================================
    DECLARE @M2UnitId UNIQUEIDENTIFIER = '4CEB18A8-BB2C-4F38-9D57-6F95DC7B6673';  -- SQM
    DECLARE @M3UnitId UNIQUEIDENTIFIER = '12F66795-DEBC-494C-BECE-67C5AA5D39DC';  -- CUBIC_M
    DECLARE @MUnitId UNIQUEIDENTIFIER = '6C1A4A7F-8823-4891-B4E2-55B166E40725';   -- M
    DECLARE @ItemUnitId UNIQUEIDENTIFIER = '3AEE3863-3FF8-459C-97F5-4A2F795619D3'; -- UNIT
    
    PRINT 'Unit of Measurement IDs:';
    PRINT '  SQM (m²):     4CEB18A8-BB2C-4F38-9D57-6F95DC7B6673';
    PRINT '  CUBIC_M (m³): 12F66795-DEBC-494C-BECE-67C5AA5D39DC';
    PRINT '  M (m):        6C1A4A7F-8823-4891-B4E2-55B166E40725';
    PRINT '  UNIT:         3AEE3863-3FF8-459C-97F5-4A2F795619D3';
    PRINT '';
    PRINT 'Creating 30 RUA AlHaram AlMakki Assets...';
    PRINT '';
    
    -- =============================================
    -- Asset 1: Major Demolition Works
    -- =============================================
    DECLARE @Asset1Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        ConstructionStartingQuarter, ConstructionStartingYear,
        ConstructionCompletionQuarter, ConstructionCompletionYear,
        TenderingStage, DevelopmentType, FundingModel,
        IsRevenueGenerating, IsPifGuaranteesRequired,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset1Id, 'Major Demolition Works', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Demolition',
        1, 2100000, @M2UnitId, 
        'Major Demolition Works; Allowance for Demolition of buildings and site clearance',
        4, 2025, 2, 2028,
        'PreTender', 'Greenfield', 'FullySelfFunded', 1, 0,
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES
    (NEWID(), @Asset1Id, 2024, 65679539), (NEWID(), @Asset1Id, 2025, 108371239),
    (NEWID(), @Asset1Id, 2026, 108371239), (NEWID(), @Asset1Id, 2027, 108371239),
    (NEWID(), @Asset1Id, 2028, 108371239), (NEWID(), @Asset1Id, 2029, 108371239);
    INSERT INTO AssetOpexDetails (Id, AssetId, Year, Amount) VALUES
    (NEWID(), @Asset1Id, 2024, 3000000), (NEWID(), @Asset1Id, 2025, 3000000),
    (NEWID(), @Asset1Id, 2026, 3000000), (NEWID(), @Asset1Id, 2027, 3000000),
    (NEWID(), @Asset1Id, 2028, 3000000);
    PRINT '? Asset 1: Major Demolition Works';
    
    -- =============================================
    -- Asset 2: Earthworks - Excavation
    -- =============================================
    DECLARE @Asset2Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        ConstructionStartingQuarter, ConstructionStartingYear,
        ConstructionCompletionQuarter, ConstructionCompletionYear,
        TenderingStage, DevelopmentType, FundingModel,
        IsRevenueGenerating, IRR, IsPifGuaranteesRequired,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset2Id, 'Earthworks - Excavation to Platform Levels', @LocationCity,
        @EnablingWorksSectorId, @EarthworkSubSectorId, @EarthworkAssetTypeId,
        1, 18775503, @M3UnitId, 'Earthworks; excavate to reach proposed platform levels',
        1, 2021, 2, 2026,
        'Execution', 'Brownfield', 'JointVenture', 1, 0, 0,
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES
    (NEWID(), @Asset2Id, 2024, 100000), (NEWID(), @Asset2Id, 2025, 108371239),
    (NEWID(), @Asset2Id, 2026, 108371239), (NEWID(), @Asset2Id, 2027, 108371239),
    (NEWID(), @Asset2Id, 2028, 108371239), (NEWID(), @Asset2Id, 2029, 1500000);
    INSERT INTO AssetOpexDetails (Id, AssetId, Year, Amount) VALUES
    (NEWID(), @Asset2Id, 2024, 3000000), (NEWID(), @Asset2Id, 2026, 3000000),
    (NEWID(), @Asset2Id, 2027, 3000000), (NEWID(), @Asset2Id, 2028, 3000000);
    PRINT '? Asset 2: Earthworks - Excavation';
    
    -- =============================================
    -- Asset 3: Earthworks - Disposal
    -- =============================================
    DECLARE @Asset3Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        ConstructionStartingQuarter, ConstructionStartingYear,
        ConstructionCompletionQuarter, ConstructionCompletionYear,
        TenderingStage, DevelopmentType, FundingModel,
        ExpectedDebt, ExpectedEquity, IsRevenueGenerating, IRR, IsPifGuaranteesRequired,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset3Id, 'Earthworks - Disposal to Approved Tip', @LocationCity,
        @EnablingWorksSectorId, @EarthworkSubSectorId, @EarthworkAssetTypeId,
        1, 13823462, @M3UnitId, 'Earthworks; allowance for disposal to approved tip',
        1, 2022, 2, 2025,
        'Delivered', 'Greenfield', 'PublicPrivatePartnership', 30, 70, 1, 7, 1,
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES
    (NEWID(), @Asset3Id, 2024, 30000000), (NEWID(), @Asset3Id, 2025, 49259654),
    (NEWID(), @Asset3Id, 2026, 49259654), (NEWID(), @Asset3Id, 2027, 108371239),
    (NEWID(), @Asset3Id, 2028, 108371239), (NEWID(), @Asset3Id, 2029, 133808576),
    (NEWID(), @Asset3Id, 2030, 155393982);
    INSERT INTO AssetOpexDetails (Id, AssetId, Year, Amount) VALUES
    (NEWID(), @Asset3Id, 2024, 200000), (NEWID(), @Asset3Id, 2025, 1000000),
    (NEWID(), @Asset3Id, 2026, 200000), (NEWID(), @Asset3Id, 2027, 200000),
    (NEWID(), @Asset3Id, 2028, 200000);
    PRINT '? Asset 3: Earthworks - Disposal';
    
    -- =============================================
    -- Asset 4: Earthworks - Screening
    -- =============================================
    DECLARE @Asset4Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset4Id, 'Earthworks - Screening, Filtering, Stockpiling and Crushing', @LocationCity,
        @EnablingWorksSectorId, @EarthworkSubSectorId, @EarthworkAssetTypeId,
        1, 4952041, @M3UnitId, 'Earthworks; screening, filtering, stockpiling and crushing',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset4Id, 2024, 133705107);
    PRINT '? Asset 4: Earthworks - Screening/Filtering';
    
    -- =============================================
    -- Asset 5: Earthworks - Filling
    -- =============================================
    DECLARE @Asset5Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset5Id, 'Earthworks - Filling Below Subgrade', @LocationCity,
        @EnablingWorksSectorId, @EarthworkSubSectorId, @EarthworkAssetTypeId,
        1, 4952041, @M3UnitId, 'Earthworks; filling below subgrade using free issued natural material',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset5Id, 2024, 133705107);
    PRINT '? Asset 5: Earthworks - Filling';
    
    -- =============================================
    -- Asset 6: Structural Backfill
    -- =============================================
    DECLARE @Asset6Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset6Id, 'Structural Backfill - Compacted Layers', @LocationCity,
        @EnablingWorksSectorId, @EarthworkSubSectorId, @EarthworkAssetTypeId,
        1, 157118, @M3UnitId, 'Structural backfill placed in layers not exceeding 250mm compacted',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset6Id, 2024, 6284710);
    PRINT '? Asset 6: Structural Backfill';
    
    -- =============================================
    -- Asset 7: Retaining Wall 0-8m
    -- =============================================
    DECLARE @Asset7Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset7Id, 'Retaining Wall - Height 0-8m', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Retaining Walls',
        1, 8166, @MUnitId, 'Retaining Wall (1x1x1m MSE gabion walls); Height 0-8m',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset7Id, 2024, 97992000);
    PRINT '? Asset 7: Retaining Wall 0-8m';
    
    -- =============================================
    -- Asset 8: Retaining Wall 8-20m
    -- =============================================
    DECLARE @Asset8Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset8Id, 'Retaining Wall - Height 8-20m', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Retaining Walls',
        1, 417, @MUnitId, 'Retaining Wall; Height 8-20m',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset8Id, 2024, 12510000);
    PRINT '? Asset 8: Retaining Wall 8-20m';
    
    -- =============================================
    -- Asset 9: Retaining Wall 20-34m
    -- =============================================
    DECLARE @Asset9Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset9Id, 'Retaining Wall - Height 20-34m', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Retaining Walls',
        1, 3884, @MUnitId, 'Retaining Wall; Height 20-34m',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset9Id, 2024, 198084000);
    PRINT '? Asset 9: Retaining Wall 20-34m';
    
    -- =============================================
    -- Asset 10: Site Hoarding
    -- =============================================
    DECLARE @Asset10Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset10Id, 'Site Hoarding', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Hoarding',
        1, 47772, @MUnitId, 'Site Hoarding; fencing with LED lights and branding',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset10Id, 2024, 119278170);
    PRINT '? Asset 10: Site Hoarding';
    
    -- =============================================
    -- Assets 11-22: Site Accommodation
    -- =============================================
    DECLARE @Asset11Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset11Id, 'Site Accommodation - Employer Office', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 1500, @M2UnitId, 'Site Accommodation; Employer Office',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset11Id, 2024, 7257000);
    PRINT '? Asset 11: Site Accommodation - Employer Office';
    
    DECLARE @Asset12Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset12Id, 'Site Accommodation - Supervision Consultant', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 452, @M2UnitId, 'Site Accommodation; Supervision consultant',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset12Id, 2024, 1945320);
    PRINT '? Asset 12: Site Accommodation - Supervision';
    
    DECLARE @Asset13Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset13Id, 'Site Accommodation - Civil Contractor 1', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 22500, @M2UnitId, 'Site Accommodation; Civil Contractor 1',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset13Id, 2024, 72562500);
    PRINT '? Asset 13: Site Accommodation - Civil Contractor 1';
    
    DECLARE @Asset14Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset14Id, 'Site Accommodation - Civil Contractor 2', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 6786, @M2UnitId, 'Site Accommodation; Civil Contractor 2',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset14Id, 2024, 21884850);
    PRINT '? Asset 14: Site Accommodation - Civil Contractor 2';
    
    DECLARE @Asset15Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset15Id, 'Site Accommodation - Other Trade Packages', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 6786, @M2UnitId, 'Site Accommodation; Other Trade Packages',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset15Id, 2024, 8143200);
    PRINT '? Asset 15: Site Accommodation - Trade Packages';
    
    DECLARE @Asset16Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset16Id, 'Site Accommodation - Prayer Room & Ablution', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 1890, @M2UnitId, 'Site Accommodation; Prayer Room & Ablution area',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset16Id, 2024, 5201280);
    PRINT '? Asset 16: Site Accommodation - Prayer Room';
    
    DECLARE @Asset17Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset17Id, 'Site Accommodation - Laydown Areas', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 35264, @M2UnitId, 'Site Accommodation; Laydown Areas',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset17Id, 2024, 49369364);
    PRINT '? Asset 17: Site Accommodation - Laydown Areas';
    
    DECLARE @Asset18Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset18Id, 'Site Accommodation - Workers Welfare Facility', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 4500, @M2UnitId, 'Site Accommodation; Workers Welfare facility',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset18Id, 2024, 14355000);
    PRINT '? Asset 18: Site Accommodation - Workers Welfare';
    
    DECLARE @Asset19Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset19Id, 'Site Accommodation - Site Clinic and HSE Offices', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 3375, @M2UnitId, 'Site Accommodation; Site Clinic and HSE offices',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset19Id, 2024, 10766250);
    PRINT '? Asset 19: Site Accommodation - Clinic/HSE';
    
    DECLARE @Asset20Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset20Id, 'Site Accommodation - Store Offices', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 1500, @M2UnitId, 'Site Accommodation; Store Offices',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset20Id, 2024, 4785000);
    PRINT '? Asset 20: Site Accommodation - Store Offices';
    
    DECLARE @Asset21Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset21Id, 'Site Accommodation - Training Rooms', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 900, @M2UnitId, 'Site Accommodation; Training rooms',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset21Id, 2024, 2871000);
    PRINT '? Asset 21: Site Accommodation - Training Rooms';
    
    DECLARE @Asset22Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt)
    VALUES (@Asset22Id, 'Site Accommodation - Smoking Areas', @LocationCity,
        @EnablingWorksSectorId, @EnablingOtherSubSectorId, 'Site Accommodation',
        1, 120, @M2UnitId, 'Site Accommodation; Smoking areas',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE());
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset22Id, 2024, 252000);
    PRINT '? Asset 22: Site Accommodation - Smoking Areas';
    
    -- =============================================
    -- Assets 23-30: Water & Waste Infrastructure
    -- =============================================
    DECLARE @Asset23Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset23Id, 'Irrigation Distribution Network (Pumping Stations & Tanks)', @LocationCity,
        @WaterWasteSectorId, @IrrigationSubSectorId, @IrrigationDistNetworkAssetTypeId, 'including pumping stations and tanks',
        1, 1, @ItemUnitId, 'Complete irrigation distribution network infrastructure',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset23Id, 2024, 128834754);
    PRINT '? Asset 23: Irrigation Network';
    
    DECLARE @Asset24Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset24Id, 'Potable Water Transmission Network (Pumping Stations & Tanks)', @LocationCity,
        @WaterWasteSectorId, @PotableWaterSubSectorId, @PotableTransNetworkAssetTypeId, 'including pumping stations and tanks',
        1, 1, @ItemUnitId, 'Potable water transmission network infrastructure',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset24Id, 2024, 1391930712);
    PRINT '? Asset 24: Potable Water Transmission';
    
    DECLARE @Asset25Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset25Id, 'Potable Water Distribution Network', @LocationCity,
        @WaterWasteSectorId, @PotableWaterSubSectorId, @PotableDistNetworkAssetTypeId,
        1, 1, @ItemUnitId, 'Potable water distribution network',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset25Id, 2024, 33660917);
    PRINT '? Asset 25: Potable Water Distribution';
    
    DECLARE @Asset26Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset26Id, 'Sewage Treatment Plant (Pumping & Lifting)', @LocationCity,
        @WaterWasteSectorId, @SewageSubSectorId, @SewagePlantAssetTypeId, 'including pumping and lifting station',
        1, 1, @ItemUnitId, 'Sewage treatment plant with pumping infrastructure',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset26Id, 2024, 919710670);
    PRINT '? Asset 26: Sewage Treatment Plant';
    
    DECLARE @Asset27Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeOther,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset27Id, 'Sewage Collection Network', @LocationCity,
        @WaterWasteSectorId, @SewageSubSectorId, 'Sewage collection Network',
        1, 1, @ItemUnitId, 'Sewage collection network infrastructure',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset27Id, 2024, 106960790);
    PRINT '? Asset 27: Sewage Collection Network';
    
    DECLARE @Asset28Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset28Id, 'Solid Waste Transfer Station', @LocationCity,
        @WaterWasteSectorId, @SolidWasteSubSectorId, @SolidWasteTransferAssetTypeId,
        1, 1, @ItemUnitId, 'Solid waste transfer station',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset28Id, 2024, 296028251);
    PRINT '? Asset 28: Solid Waste Transfer Station';
    
    DECLARE @Asset29Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset29Id, 'Automated Waste Collection System', @LocationCity,
        @WaterWasteSectorId, @SolidWasteSubSectorId, @AutomatedWasteAssetTypeId,
        1, 1, @ItemUnitId, 'Automated waste collection system',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset29Id, 2024, 115257526);
    PRINT '? Asset 29: Automated Waste Collection';
    
    DECLARE @Asset30Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO Assets (
        Id, AssetName, LocationCity, SectorId, SubSectorId, AssetTypeId,
        QuantityOfAsset, CapacityPerAsset, UnitOfMeasurementId, Description,
        Status, CompanyId, CompanyName, CreatedBy, CreatedAt
    )
    VALUES (
        @Asset30Id, 'Storm Water Storage Pond', @LocationCity,
        @WaterWasteSectorId, @StormWaterSubSectorId, @StormWaterPondAssetTypeId,
        1, 1, @ItemUnitId, 'Storm water storage pond',
        0, @CompanyId, @CompanyName, @CreatedBy, GETUTCDATE()
    );
    INSERT INTO AssetCapexDetails (Id, AssetId, Year, Amount) VALUES (NEWID(), @Asset30Id, 2024, 183286420);
    PRINT '? Asset 30: Storm Water Storage Pond';
    
    COMMIT TRANSACTION;
    
    PRINT '';
    PRINT '============================================';
    PRINT 'SUCCESS: All 30 Assets Created!';
    PRINT '============================================';
    PRINT 'Company: ' + @CompanyName;
    PRINT 'Company ID: ' + CAST(@CompanyId AS NVARCHAR(50));
    PRINT 'Location: ' + @LocationCity;
    PRINT 'Total Assets: 30';
    PRINT 'Total CAPEX Records: 46';
    PRINT 'Total OPEX Records: 11';
    PRINT 'Status: Draft (0) - Ready for submission';
    PRINT '============================================';
    PRINT '';
    PRINT 'Asset Distribution:';
    PRINT '  Enabling Works: 22 assets';
    PRINT '  Water & Waste:   8 assets';
    PRINT '============================================';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '? ERROR: ' + ERROR_MESSAGE();
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS NVARCHAR(10));
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10));
    PRINT 'Transaction rolled back - no data inserted';
    PRINT '';
    
    IF ERROR_NUMBER() = 547
        PRINT '??  Foreign Key constraint violation';
    IF ERROR_NUMBER() = 2627 OR ERROR_NUMBER() = 2601
        PRINT '??  Duplicate key - some assets may already exist';
END CATCH

GO
