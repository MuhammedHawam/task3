using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Application.Common.Options;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;
using PartnersHub.InfraBase.Domain.Enums;
using PartnersHub.InfraBase.Infrastructure.Persistence;
using PartnersHub.InfraBase.Infrastructure.Persistence.Repositories;

namespace PartnersHub.Services.InfraBase.IntegrationTests;

[TestFixture]
public class AssetIntegrationTests
{
    private InfrabaseDbContext _context = null!;
    private IAssetRepository _repository = null!;
    private IUnitOfWork _unitOfWork = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<InfrabaseDbContext>()
            .UseInMemoryDatabase(databaseName: $"InfraBaseTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new InfrabaseDbContext(options);
        
        var assetCodeSettings = Options.Create(new AssetCodeSettings
        {
            Prefix = "Infra",
            NumberFormat = "000000",
            Separator = "-"
        });
        
        _repository = new AssetRepository(_context, assetCodeSettings);
        _unitOfWork = new UnitOfWork(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Required Field Validation Tests

    [Test]
    public void CreateAsset_WithoutAssetName_FailsValidation()
    {
        var result = Asset.Create(
            "", // Empty asset name
            "Riyadh", 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            null, 
            null, // Quantity is optional
            100, // Capacity MANDATORY
            Guid.NewGuid(), 
            null,
            "Test Description",
            null, // Construction dates optional
            null, 
            null, 
            null, 
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            0, 
            0, 
            false, 
            0, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Asset name"));
    }

    [Test]
    public void CreateAsset_WithoutLocationCity_FailsValidation()
    {
        var result = Asset.Create(
            "Test Asset", 
            "", // Empty location
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            null, 
            null, 
            100, 
            Guid.NewGuid(), 
            null,
            "Test Description",
            null, // Construction dates optional
            null, 
            null, 
            null, 
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            0, 
            0, 
            false, 
            0, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Location"));
    }

    [Test]
    public void CreateAsset_WithoutSectorId_FailsValidation()
    {
        var result = Asset.Create(
            "Test Asset", 
            "Riyadh", 
            Guid.Empty, // Empty sector
            Guid.NewGuid(), 
            Guid.NewGuid(),
            null, 
            null, 
            100, 
            Guid.NewGuid(), 
            null,
            "Test Description",
            null, // Construction dates optional
            null, 
            null, 
            null, 
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            0, 
            0, 
            false, 
            0, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Sector is required"));
    }

    [Test]
    public void CreateAsset_WithoutSubSectorId_FailsValidation()
    {
        var result = Asset.Create(
            "Test Asset", 
            "Riyadh", 
            Guid.NewGuid(), 
            Guid.Empty, // Empty sub-sector
            Guid.NewGuid(),
            null, 
            null, 
            100, 
            Guid.NewGuid(), 
            null,
            "Test Description",
            null, // Construction dates optional
            null, 
            null, 
            null, 
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            0, 
            0, 
            false, 
            0, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Sub sector is required"));
    }

    [Test]
    public void CreateAsset_WithoutCapacityPerAsset_FailsValidation()
    {
        var result = Asset.Create(
            "Test Asset", 
            "Riyadh", 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            null, 
            null, 
            0, // Zero capacity - MANDATORY field
            Guid.NewGuid(), 
            null,
            "Test Description",
            null, // Construction dates optional
            null, 
            null, 
            null, 
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            0, 
            0, 
            false, 
            0, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Capacity per asset must be greater than zero"));
    }

    [Test]
    public void CreateAsset_WithOptionalQuantityAsNull_Succeeds()
    {
        var result = Asset.Create(
            "Test Asset", 
            "Riyadh", 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            null, 
            null, // Quantity is OPTIONAL per user story
            100, 
            Guid.NewGuid(), 
            null,
            "Test Description",
            null, // Construction dates optional
            null, 
            null, 
            null, 
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            0, 
            0, 
            false, 
            0, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value!.QuantityOfAsset, Is.Null);
        Assert.That(result.Value!.TotalCapacity, Is.Null);
    }

    [Test]
    public void CreateAsset_WithOptionalConstructionDatesAsNull_Succeeds()
    {
        var result = Asset.Create(
            "Test Asset", 
            "Riyadh", 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            null, 
            null, // Quantity is OPTIONAL per user story
            100, 
            Guid.NewGuid(), 
            null,
            "Test Description",
            null, // Construction dates optional
            null, 
            null, 
            null, 
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            0, 
            0, 
            false, 
            0, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value!.ConstructionStartingYear, Is.Null);
        Assert.That(result.Value!.ConstructionCompletionYear, Is.Null);
    }

    [Test]
    public void CreateAsset_WithInvalidQuantity_FailsValidation()
    {
        var result = Asset.Create(
            "Test Asset", 
            "Riyadh", 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            null, 
            -5, // Negative quantity
            100, 
            Guid.NewGuid(), 
            null,
            "Test Description",
            null, // Construction dates optional
            null, 
            null, 
            null, 
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            0, 
            0, 
            false, 
            0, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Quantity of asset must be greater than zero"));
    }

    [Test]
    public void CreateAsset_WithInvalidConstructionYear_FailsValidation()
    {
        var result = Asset.Create(
            "Test Asset", 
            "Riyadh", 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            null, 
            null, 
            100, 
            Guid.NewGuid(), 
            null,
            null, 
            1, 
            2014, // Before 2015 - per user story
            1, 
            2025,
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            0, 
            0, 
            false, 
            0, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Construction starting year must be between 2015 and 2099"));
    }

    [Test]
    public void CreateAsset_WithEndDateBeforeStartDate_FailsValidation()
    {
        var result = Asset.Create(
            "Test Asset", 
            "Riyadh", 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            null, 
            null, 
            100, 
            Guid.NewGuid(), 
            null,
            null, 
            1, 
            2025, 
            1, 
            2024,
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            0, 
            0, 
            false, 
            0, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Construction completion date must be after starting date"));
    }

    #endregion

    #region Asset Creation and Update Tests

    [Test]
    public async Task CreateAsset_WithAllRequiredFields_Succeeds()
    {
        var userId = "test@example.com";
        var assetResult = Asset.Create(
            "Test Infrastructure Asset", 
            "Riyadh", 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            Guid.NewGuid(), 
            null, 
            null, 
            100, 
            Guid.NewGuid(), 
            null,
            "Test Description",
            null, 
            null, 
            null, 
            null, 
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            10000, 
            5000, 
            true, 
            12.5m, 
            false,
            userId,
            Guid.NewGuid(),
            "Test Company");

        Assert.That(assetResult.IsSuccess, Is.True);
        var asset = assetResult.Value!;
        
        await _repository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();

        var saved = await _repository.GetByIdAsync(asset.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.AssetName.Value, Is.EqualTo("Test Infrastructure Asset"));
        Assert.That(saved.Status, Is.EqualTo(AssetStatuses.Draft));
    }

    [Test]
    public async Task CreateAsset_WithCapexAndOpex_SavesCompleteAggregate()
    {
        var asset = CreateTestAsset();
        
        asset.AddCapexDetail(2024, 50000, asset.CreatedBy!);
        asset.AddCapexDetail(2025, 30000, asset.CreatedBy!);
        asset.AddOpexDetail(2024, 5000, asset.CreatedBy!);
        asset.AddOpexDetail(2025, 6000, asset.CreatedBy!);

        await _repository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();

        var saved = await _repository.GetByIdWithDetailsAsync(asset.Id);
        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.CapexDetails.Count, Is.EqualTo(2));
        Assert.That(saved.OpexDetails.Count, Is.EqualTo(2));
        Assert.That(saved.TotalCapex, Is.EqualTo(80000));
        Assert.That(saved.TotalOpex, Is.EqualTo(11000));
    }

    [Test]
    public async Task UpdateAsset_Information_UpdatesSuccessfully()
    {
        var asset = CreateTestAsset();
        
        await _repository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();

        var updateResult = asset.UpdateAssetInformation(
            "Updated Asset Name", 
            "Jeddah", 
            null, 
            null, 
            null, 
            null, 
            15m, 
            150m,
            null,
            null,
            "Updated Description", 
            null, 
            null, 
            null, 
            null,
            TenderingStages.Tendered, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null, 
            null,
            asset.CreatedBy!);
        
        await _unitOfWork.SaveChangesAsync();

        Assert.That(updateResult.IsSuccess, Is.True);
        
        var updated = await _repository.GetByIdAsync(asset.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.AssetName.Value, Is.EqualTo("Updated Asset Name"));
        Assert.That(updated.LocationCity.Value, Is.EqualTo("Jeddah"));
        Assert.That(updated.TenderingStage, Is.EqualTo(TenderingStages.Tendered));
        Assert.That(updated.QuantityOfAsset, Is.EqualTo(15m));
        Assert.That(updated.CapacityPerAsset, Is.EqualTo(150m));
    }

    #endregion

    #region Asset Workflow Tests

    [Test]
    public async Task SubmitAsset_ChangesStatusToSubmitted()
    {
        var asset = CreateTestAssetWithFinancials();
        
        await _repository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();

        var submitResult = asset.Submit(asset.CreatedBy!, "Infra-000001", false);
        await _unitOfWork.SaveChangesAsync();

        Assert.That(submitResult.IsSuccess, Is.True);
        
        var submitted = await _repository.GetByIdAsync(asset.Id);
        Assert.That(submitted!.Status, Is.EqualTo(AssetStatuses.Submitted));
        Assert.That(submitted.AssetCode, Is.EqualTo("Infra-000001"));
    }

    [Test]
    public void CannotSubmit_WithoutCapex()
    {
        var asset = CreateTestAsset();
        asset.AddOpexDetail(2024, 5000, asset.CreatedBy!);

        var result = asset.Submit(asset.CreatedBy!, "Infra-000001", false);
        
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("CAPEX"));
    }

    [Test]
    public void CannotSubmit_WithoutOpex()
    {
        var asset = CreateTestAsset();
        asset.AddCapexDetail(2024, 10000, asset.CreatedBy!);

        var result = asset.Submit(asset.CreatedBy!, "Infra-000001", false);
        
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("OPEX"));
    }

    [Test]
    public async Task AcceptByPcAdmin_ChangesStatusToPcAdminApproved()
    {
        var asset = CreateTestAssetWithFinancials();
        
        asset.Submit(asset.CreatedBy!, "Infra-000001", false);
        await _repository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();
        
        var adminId = "pcadmin@example.com";
        var acceptResult = asset.AcceptByPcAdmin(adminId);
        await _unitOfWork.SaveChangesAsync();

        Assert.That(acceptResult.IsSuccess, Is.True);
        
        var approved = await _repository.GetByIdAsync(asset.Id);
        Assert.That(approved!.Status, Is.EqualTo(AssetStatuses.AcceptedByPcAdmin));
        Assert.That(approved.ApprovedBy, Is.EqualTo(adminId));
    }

    [Test]
    public async Task RejectByPcAdmin_ChangesStatusToRejected()
    {
        var asset = CreateTestAssetWithFinancials();
        
        asset.Submit(asset.CreatedBy!, "Infra-000001", false);
        await _repository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();
        
        var adminId = "pcadmin@example.com";
        var rejectResult = asset.RejectByPcAdmin(adminId, "Missing information");
        await _unitOfWork.SaveChangesAsync();

        Assert.That(rejectResult.IsSuccess, Is.True);
        
        var rejected = await _repository.GetByIdAsync(asset.Id);
        Assert.That(rejected!.Status, Is.EqualTo(AssetStatuses.RejectedByPcAdmin));
        Assert.That(rejected.RejectionReason!.Value, Is.EqualTo("Missing information"));
    }

    [Test]
    public async Task CheckByInfrabaseAdmin_ChangesStatusToChecked()
    {
        var asset = CreateTestAssetWithFinancials();
        
        asset.Submit(asset.CreatedBy!, "Infra-000001", false);
        asset.AcceptByPcAdmin("pcadmin@example.com");
        await _repository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();
        
        var adminId = "infrabaseadmin@example.com";
        var checkResult = asset.CheckByInfrabaseAdmin(adminId);
        await _unitOfWork.SaveChangesAsync();

        Assert.That(checkResult.IsSuccess, Is.True);
        
        var checkedAsset = await _repository.GetByIdAsync(asset.Id);
        Assert.That(checkedAsset!.Status, Is.EqualTo(AssetStatuses.AcceptedByInfrabase));
        Assert.That(checkedAsset.ApprovedBy, Is.EqualTo(adminId));
    }

    [Test]
    public async Task ReturnForCorrection_ChangesStatusToReturnedForCorrection()
    {
        var asset = CreateTestAssetWithFinancials();
        
        asset.Submit(asset.CreatedBy!, "Infra-000001", false);
        asset.AcceptByPcAdmin("pcadmin@example.com");
        await _repository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();
        
        var adminId = "infrabaseadmin@example.com";
        var returnResult = asset.ReturnForCorrectionByInfrabaseAdmin(adminId, "Please update financial data");
        await _unitOfWork.SaveChangesAsync();

        Assert.That(returnResult.IsSuccess, Is.True);
        
        var returned = await _repository.GetByIdAsync(asset.Id);
        Assert.That(returned!.Status, Is.EqualTo(AssetStatuses.RejectedByInfrabase));
    }

    #endregion

    #region Repository Tests

    [Test]
    public async Task AddCapexDetail_AddsSuccessfully()
    {
        var asset = CreateTestAsset();
        
        await _repository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();

        var addResult = asset.AddCapexDetail(2024, 10000, asset.CreatedBy!);
        await _unitOfWork.SaveChangesAsync();

        Assert.That(addResult.IsSuccess, Is.True);
        
        var updated = await _repository.GetByIdWithDetailsAsync(asset.Id);
        Assert.That(updated!.CapexDetails.Count, Is.EqualTo(1));
        Assert.That(updated.TotalCapex, Is.EqualTo(10000));
    }

    [Test]
    public async Task GetByStatusAsync_ReturnsAssetsWithStatus()
    {
        var asset1 = CreateTestAssetWithFinancials();
        var asset2 = CreateTestAssetWithFinancials();
        
        asset1.Submit(asset1.CreatedBy!, "Infra-000001", false);
        
        await _repository.AddAsync(asset1);
        await _repository.AddAsync(asset2);
        await _unitOfWork.SaveChangesAsync();

        var submitted = await _repository.GetPagedAsync(1, 100, AssetStatuses.Submitted);
        var draft = await _repository.GetPagedAsync(1, 100, AssetStatuses.Draft);

        Assert.That(submitted.TotalCount, Is.EqualTo(1));
        Assert.That(draft.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetPagedAsync_ReturnsPaginatedResults()
    {
        for (int i = 0; i < 15; i++)
        {
            var asset = CreateTestAsset();
            await _repository.AddAsync(asset);
        }
        await _unitOfWork.SaveChangesAsync();

        var paginatedAssets = await _repository.GetPagedAsync(1, 10);

        Assert.That(paginatedAssets.Items.Count, Is.EqualTo(10));
        Assert.That(paginatedAssets.TotalCount, Is.EqualTo(15));
    }

    [Test]
    public async Task GetStatusCountsAsync_ReturnsCorrectCounts()
    {
        var asset1 = CreateTestAssetWithFinancials();
        var asset2 = CreateTestAssetWithFinancials();
        var asset3 = CreateTestAssetWithFinancials();
        
        asset1.Submit(asset1.CreatedBy!, "Infra-000001", false);
        asset2.Submit(asset2.CreatedBy!, "Infra-000002", false);

        await _repository.AddAsync(asset1);
        await _repository.AddAsync(asset2);
        await _repository.AddAsync(asset3);
        await _unitOfWork.SaveChangesAsync();

        var counts = await _repository.GetStatusCountsAsync();

        Assert.That(counts[AssetStatuses.Draft], Is.EqualTo(1));
        Assert.That(counts[AssetStatuses.Submitted], Is.EqualTo(2));
    }

    [Test]
    public async Task GetNextAssetNumberAsync_ReturnsCorrectNumber()
    {
        var asset1 = CreateTestAssetWithFinancials();
        
        asset1.Submit(asset1.CreatedBy!, "Infra-000001", false);
        await _repository.AddAsync(asset1);
        await _unitOfWork.SaveChangesAsync();

        var nextNumber = await _repository.GetNextAssetNumberAsync();

        Assert.That(nextNumber, Is.EqualTo(2));
    }

    #endregion

    #region Helper Methods

    private Asset CreateTestAsset()
    {
        var result = Asset.Create(
            "Test Asset", 
            "Riyadh", 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid(),
            null, 
            null, 
            100, 
            Guid.NewGuid(),
            null,
            "Test Description", 
            null, 
            null, 
            null, 
            null,
            TenderingStages.PreTender, 
            DevelopmentTypes.Greenfield,
            FundingModels.FullyGovernmentFunded, 
            10000, 
            5000, 
            true, 
            12.5m, 
            false,
            "test@example.com",
            Guid.NewGuid(),
            "Test Company");
        
        if (result.IsFailure)
        {
            Assert.Fail($"Failed to create test asset: {result.Error}");
        }
        
        return result.Value!;
    }

    private Asset CreateTestAssetWithFinancials()
    {
        var asset = CreateTestAsset();
        
        var capexResult1 = asset.AddCapexDetail(2024, 50000, asset.CreatedBy!);
        Assert.That(capexResult1.IsSuccess, Is.True, $"Failed to add CAPEX 2024: {capexResult1.Error}");
        
        var capexResult2 = asset.AddCapexDetail(2025, 30000, asset.CreatedBy!);
        Assert.That(capexResult2.IsSuccess, Is.True, $"Failed to add CAPEX 2025: {capexResult2.Error}");
        
        var opexResult1 = asset.AddOpexDetail(2024, 5000, asset.CreatedBy!);
        Assert.That(opexResult1.IsSuccess, Is.True, $"Failed to add OPEX 2024: {opexResult1.Error}");
        
        var opexResult2 = asset.AddOpexDetail(2025, 6000, asset.CreatedBy!);
        Assert.That(opexResult2.IsSuccess, Is.True, $"Failed to add OPEX 2025: {opexResult2.Error}");
        
        return asset;
    }

    #endregion
}
