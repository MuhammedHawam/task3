using PartnersHub.InfraBase.Domain.Common;
using PartnersHub.InfraBase.Domain.Enums;
using PartnersHub.InfraBase.Domain.Events;
using PartnersHub.InfraBase.Domain.ValueObjects;

namespace PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

public class Asset : AggregateRoot
{
    private readonly List<AssetCapex> _capexDetails = new();
    private readonly List<AssetOpex> _opexDetails = new();
    private readonly List<AssetHistory> _history = new();
    private readonly List<AssetAttachment> _attachments = new();

    public string? AssetCode { get; private set; }
    public AssetName AssetName { get; private set; } = null!;
    public LocationCity LocationCity { get; private set; } = null!;

    public Guid? SectorId { get; private set; }
    public Guid? SubSectorId { get; private set; }
    public Guid? AssetTypeId { get; private set; }
    public string? AssetTypeOther { get; private set; }
    public decimal? QuantityOfAsset { get; private set; }
    public decimal CapacityPerAsset { get; private set; }
    public decimal? TotalCapacity => QuantityOfAsset.HasValue
        ? QuantityOfAsset.Value * CapacityPerAsset
        : (decimal?)null;
    public Guid? UnitOfMeasurementId { get; private set; }
    public string? UnitOfMeasurementOther { get; private set; }
    public AssetDescription? Description { get; private set; }

    public int? ConstructionStartingQuarter { get; private set; }
    public int? ConstructionStartingYear { get; private set; }
    public int? ConstructionCompletionQuarter { get; private set; }
    public int? ConstructionCompletionYear { get; private set; }

    public TenderingStages? TenderingStage { get; private set; }
    public DevelopmentTypes? DevelopmentType { get; private set; }
    public FinancialEntryMode CapexEntryMode { get; private set; } = FinancialEntryMode.MultiYear;
    public FinancialEntryMode OpexEntryMode { get; private set; } = FinancialEntryMode.MultiYear;
    public decimal TotalCapex => _capexDetails.Sum(c => c.Amount);
    public decimal TotalOpex => _opexDetails.Sum(o => o.Amount);
    public FundingModels? FundingModel { get; private set; }
    public decimal? ExpectedDebt { get; private set; }
    public decimal? ExpectedEquity { get; private set; }
    public bool? IsRevenueGenerating { get; private set; }
    public decimal? IRR { get; private set; }
    public bool? IsPifGuaranteesRequired { get; private set; }

    public AssetStatuses Status { get; private set; }
    public string? SubmittedBy { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public RejectionReason? RejectionReason { get; private set; }
    public string? RejectedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid CompanyId { get; private set; }
    public string? CompanyName { get; private set; }

    public IReadOnlyCollection<AssetCapex> CapexDetails => _capexDetails.AsReadOnly();
    public IReadOnlyCollection<AssetOpex> OpexDetails => _opexDetails.AsReadOnly();
    public IReadOnlyCollection<AssetHistory> History => _history.AsReadOnly();
    public IReadOnlyCollection<AssetAttachment> Attachments => _attachments.AsReadOnly();

    private Asset() { }

    private Asset(AssetName assetName, LocationCity locationCity, Guid? sectorId,
        Guid? subSectorId, Guid? assetTypeId, string? assetTypeOther, decimal? quantityOfAsset,
        decimal capacityPerAsset, Guid? unitOfMeasurementId, string? unitOfMeasurementOther,
        AssetDescription? description,
        int? constructionStartingQuarter, int? constructionStartingYear,
        int? constructionCompletionQuarter, int? constructionCompletionYear,
        TenderingStages? tenderingStage, DevelopmentTypes? developmentType,
        FundingModels? fundingModel, decimal? expectedDebt, decimal? expectedEquity,
        bool? isRevenueGenerating, decimal? irr, bool? isPifGuaranteesRequired,
        string createdBy, Guid companyId, string? companyName)
    {
        AssetName = assetName;
        LocationCity = locationCity;
        SectorId = sectorId;
        SubSectorId = subSectorId;
        AssetTypeId = assetTypeId;
        AssetTypeOther = assetTypeOther;
        QuantityOfAsset = quantityOfAsset;
        CapacityPerAsset = capacityPerAsset;
        UnitOfMeasurementId = unitOfMeasurementId;
        UnitOfMeasurementOther = unitOfMeasurementOther;
        Description = description;
        ConstructionStartingQuarter = constructionStartingQuarter;
        ConstructionStartingYear = constructionStartingYear;
        ConstructionCompletionQuarter = constructionCompletionQuarter;
        ConstructionCompletionYear = constructionCompletionYear;
        TenderingStage = tenderingStage;
        DevelopmentType = developmentType;
        CapexEntryMode = FinancialEntryMode.MultiYear;
        OpexEntryMode = FinancialEntryMode.MultiYear;
        FundingModel = fundingModel;
        ExpectedDebt = expectedDebt;
        ExpectedEquity = expectedEquity;
        IsRevenueGenerating = isRevenueGenerating;
        IRR = irr;
        IsPifGuaranteesRequired = isPifGuaranteesRequired;
        Status = AssetStatuses.Draft;
        CreatedBy = createdBy;
        CreatedAt = DateTime.Now;
        CompanyId = companyId;
        CompanyName = companyName;

        AddHistory("Created", createdBy, "Asset created as draft");
    }

    public static Result<Asset> Create(string assetName, string locationCity,
        Guid? sectorId, Guid? subSectorId, Guid? assetTypeId, string? assetTypeOther,
        decimal? quantityOfAsset, decimal capacityPerAsset, Guid? unitOfMeasurementId,
        string? unitOfMeasurementOther, string? description,
        int? constructionStartingQuarter, int? constructionStartingYear,
        int? constructionCompletionQuarter, int? constructionCompletionYear,
        TenderingStages? tenderingStage, DevelopmentTypes? developmentType,
        FundingModels? fundingModel, decimal? expectedDebt, decimal? expectedEquity,
        bool? isRevenueGenerating, decimal? irr, bool? isPifGuaranteesRequired,
        string createdBy, Guid companyId, string? companyName = null)
    {
        var assetNameResult = AssetName.Create(assetName);
        if (assetNameResult.IsFailure)
        {
            return Result<Asset>.Failure(assetNameResult.Error!);
        }

        var locationCityResult = LocationCity.Create(locationCity);
        if (locationCityResult.IsFailure)
        {
            return Result<Asset>.Failure(locationCityResult.Error!);
        }

        if (companyId == Guid.Empty)
        {
            return Result<Asset>.Failure("Company is required");
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            return Result<Asset>.Failure("User is required");
        }

        if (!sectorId.HasValue || sectorId.Value == Guid.Empty)
        {
            return Result<Asset>.Failure("Sector is required");
        }

        if (!subSectorId.HasValue || subSectorId.Value == Guid.Empty)
        {
            return Result<Asset>.Failure("Sub sector is required");
        }

        // Set AssetTypeId to null if empty or if AssetTypeOther is provided
        if (!assetTypeId.HasValue || assetTypeId.Value == Guid.Empty)
        {
            assetTypeId = null;
        }
        else if (!string.IsNullOrWhiteSpace(assetTypeOther))
        {
            assetTypeId = null;
        }

        // Set UnitOfMeasurementId to null if empty or if UnitOfMeasurementOther is provided
        if (!unitOfMeasurementId.HasValue || unitOfMeasurementId.Value == Guid.Empty)
        {
            unitOfMeasurementId = null;
        }
        else if (!string.IsNullOrWhiteSpace(unitOfMeasurementOther))
        {
            unitOfMeasurementId = null;
        }

        var normalizedQuantity = quantityOfAsset == 0 ? null : quantityOfAsset;

        if (normalizedQuantity.HasValue)
        {
            if (normalizedQuantity.Value < 0)
            {
                return Result<Asset>.Failure("Quantity of asset must be greater than zero");
            }

            if (normalizedQuantity.Value.ToString().Replace(".", "").Replace(",", "").Length > 5)
            {
                return Result<Asset>.Failure("Quantity of asset cannot exceed 5 digits");
            }
        }

        if (capacityPerAsset <= 0)
        {
            return Result<Asset>.Failure("Capacity per asset must be greater than zero");
        }

        if (capacityPerAsset.ToString().Replace(".", "").Replace(",", "").Length > 5)
        {
            return Result<Asset>.Failure("Capacity per asset cannot exceed 5 digits");
        }

        var descriptionResult = AssetDescription.Create(description);
        if (descriptionResult.IsFailure)
        {
            return Result<Asset>.Failure(descriptionResult.Error!);
        }

        var normalizedStartQuarter = constructionStartingQuarter == 0 ? null : constructionStartingQuarter;
        var normalizedStartYear = constructionStartingYear == 0 ? null : constructionStartingYear;
        var normalizedEndQuarter = constructionCompletionQuarter == 0 ? null : constructionCompletionQuarter;
        var normalizedEndYear = constructionCompletionYear == 0 ? null : constructionCompletionYear;

        if (normalizedStartQuarter.HasValue &&
            (normalizedStartQuarter.Value < 1 || normalizedStartQuarter.Value > 4))
        {
            return Result<Asset>.Failure("Construction starting quarter must be between 1 and 4");
        }

        if (normalizedStartYear.HasValue &&
            (normalizedStartYear.Value < 2015 || normalizedStartYear.Value > 2099))
        {
            return Result<Asset>.Failure("Construction starting year must be between 2015 and 2099");
        }

        if (normalizedEndQuarter.HasValue &&
            (normalizedEndQuarter.Value < 1 || normalizedEndQuarter.Value > 4))
        {
            return Result<Asset>.Failure("Construction completion quarter must be between 1 and 4");
        }

        if (normalizedEndYear.HasValue &&
            (normalizedEndYear.Value < 2015 || normalizedEndYear.Value > 2099))
        {
            return Result<Asset>.Failure("Construction completion year must be between 2015 and 2099");
        }

        if (normalizedStartYear.HasValue && normalizedEndYear.HasValue)
        {
            if (normalizedEndYear.Value < normalizedStartYear.Value)
            {
                return Result<Asset>.Failure("Construction completion date must be after starting date");
            }

            if (normalizedEndYear.Value == normalizedStartYear.Value &&
                normalizedStartQuarter.HasValue && normalizedEndQuarter.HasValue &&
                normalizedEndQuarter.Value < normalizedStartQuarter.Value)
            {
                return Result<Asset>.Failure("Construction completion date must be after starting date");
            }
        }

        // Validate expectedDebt if provided
        if (expectedDebt.HasValue)
        {
            if (expectedDebt.Value < 0)
            {
                return Result<Asset>.Failure("Expected debt cannot be negative");
            }

            if (expectedDebt.Value.ToString().Replace(".", "").Replace(",", "").Length > 5)
            {
                return Result<Asset>.Failure("Expected debt cannot exceed 5 digits");
            }
        }

        // Validate expectedEquity if provided
        if (expectedEquity.HasValue)
        {
            if (expectedEquity.Value < 0)
            {
                return Result<Asset>.Failure("Expected equity cannot be negative");
            }

            if (expectedEquity.Value.ToString().Replace(".", "").Replace(",", "").Length > 5)
            {
                return Result<Asset>.Failure("Expected equity cannot exceed 5 digits");
            }
        }

        // Validate IRR if provided
        if (irr.HasValue)
        {
            if (irr.Value < 0)
            {
                return Result<Asset>.Failure("IRR cannot be negative");
            }

            if (irr.Value.ToString().Replace(".", "").Replace(",", "").Length > 5)
            {
                return Result<Asset>.Failure("IRR cannot exceed 5 digits");
            }
        }

        var asset = new Asset(assetNameResult.Value!, locationCityResult.Value!,
            sectorId, subSectorId, assetTypeId, assetTypeOther, normalizedQuantity,
            capacityPerAsset, unitOfMeasurementId, unitOfMeasurementOther,
            descriptionResult.Value!,
            normalizedStartQuarter, normalizedStartYear,
            normalizedEndQuarter, normalizedEndYear,
            tenderingStage, developmentType, fundingModel, expectedDebt,
            expectedEquity, isRevenueGenerating, irr, isPifGuaranteesRequired,
            createdBy, companyId, companyName);

        return Result<Asset>.Success(asset);
    }

    public void AssignAssetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Asset code cannot be empty", nameof(code));
        }

        if (string.IsNullOrEmpty(AssetCode))
        {
            AssetCode = code;
        }
    }

    public Result<bool> AddCapexDetail(int year, decimal amount, string userId)
    {
        if (CapexEntryMode == FinancialEntryMode.SingleYear && _capexDetails.Count >= 1)
        {
            return Result<bool>.Failure("CAPEX entry mode is Single-Year; only one year row is allowed");
        }

        if (_capexDetails.Any(c => c.Year == year))
        {
            return Result<bool>.Failure($"CAPEX for year {year} already exists");
        }

        try
        {
            var capex = new AssetCapex(Id, year, amount);
            _capexDetails.Add(capex);
            UpdatedBy = userId;
            UpdatedAt = DateTime.Now;
            AddHistory("CAPEX Added", userId, $"CAPEX for year {year} added with amount {amount:C}");
            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public Result<bool> UpdateCapexDetail(int year, decimal amount, string userId)
    {
        var capex = _capexDetails.FirstOrDefault(c => c.Year == year);
        if (capex == null)
        {
            return Result<bool>.Failure($"CAPEX for year {year} not found");
        }

        var oldAmount = capex.Amount;
        var result = capex.UpdateAmount(amount);
        if (result.IsFailure)
        {
            return result;
        }

        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("CAPEX Updated", userId, $"CAPEX for year {year} updated",
            "Amount", oldAmount.ToString("C"), amount.ToString("C"));
        return Result<bool>.Success(true);
    }

    public Result<bool> RemoveCapexDetail(int year, string userId)
    {
        var capex = _capexDetails.FirstOrDefault(c => c.Year == year);
        if (capex == null)
        {
            return Result<bool>.Failure($"CAPEX for year {year} not found");
        }

        _capexDetails.Remove(capex);
        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("CAPEX Removed", userId, $"CAPEX for year {year} removed");
        return Result<bool>.Success(true);
    }

    public Result<bool> AddOpexDetail(int year, decimal amount, string userId)
    {
        if (OpexEntryMode == FinancialEntryMode.SingleYear && _opexDetails.Count >= 1)
        {
            return Result<bool>.Failure("OPEX entry mode is Single-Year; only one year row is allowed");
        }

        if (_opexDetails.Any(o => o.Year == year))
        {
            return Result<bool>.Failure($"OPEX for year {year} already exists");
        }

        try
        {
            var opex = new AssetOpex(Id, year, amount);
            _opexDetails.Add(opex);
            UpdatedBy = userId;
            UpdatedAt = DateTime.Now;
            AddHistory("OPEX Added", userId, $"OPEX for year {year} added with amount {amount:C}");
            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public Result<bool> UpdateOpexDetail(int year, decimal amount, string userId)
    {
        var opex = _opexDetails.FirstOrDefault(o => o.Year == year);
        if (opex == null)
        {
            return Result<bool>.Failure($"OPEX for year {year} not found");
        }

        var oldAmount = opex.Amount;
        var result = opex.UpdateAmount(amount);
        if (result.IsFailure)
        {
            return result;
        }

        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("OPEX Updated", userId, $"OPEX for year {year} updated",
            "Amount", oldAmount.ToString("C"), amount.ToString("C"));
        return Result<bool>.Success(true);
    }

    public Result<bool> RemoveOpexDetail(int year, string userId)
    {
        var opex = _opexDetails.FirstOrDefault(o => o.Year == year);
        if (opex == null)
        {
            return Result<bool>.Failure($"OPEX for year {year} not found");
        }

        _opexDetails.Remove(opex);
        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("OPEX Removed", userId, $"OPEX for year {year} removed");
        return Result<bool>.Success(true);
    }

    public Result<bool> SaveAsDraft(string userId)
    {
        if (Status != AssetStatuses.Draft && Status != AssetStatuses.RejectedByPcAdmin &&
            Status != AssetStatuses.RejectedByInfrabase)
        {
            return Result<bool>.Failure("Only draft, rejected, or returned for correction assets can be saved as draft");
        }

        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("Saved as Draft", userId, "Asset saved as draft");
        return Result<bool>.Success(true);
    }

    public Result<bool> SetCapexEntryMode(FinancialEntryMode mode, string userId)
    {
        if (mode == FinancialEntryMode.SingleYear && _capexDetails.Count > 1)
        {
            return Result<bool>.Failure("CAPEX Single-Year mode supports only one year row");
        }

        if (CapexEntryMode == mode)
        {
            return Result<bool>.Success(true);
        }

        CapexEntryMode = mode;
        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("CAPEX Entry Mode Updated", userId, $"CAPEX entry mode changed to {mode}");
        return Result<bool>.Success(true);
    }

    public Result<bool> SetOpexEntryMode(FinancialEntryMode mode, string userId)
    {
        if (mode == FinancialEntryMode.SingleYear && _opexDetails.Count > 1)
        {
            return Result<bool>.Failure("OPEX Single-Year mode supports only one year row");
        }

        if (OpexEntryMode == mode)
        {
            return Result<bool>.Success(true);
        }

        OpexEntryMode = mode;
        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("OPEX Entry Mode Updated", userId, $"OPEX entry mode changed to {mode}");
        return Result<bool>.Success(true);
    }

    public Result<bool> Submit(string userId, string assetCode, bool isPcAdmin = false)
    {
        if (Status != AssetStatuses.Draft && Status != AssetStatuses.RejectedByPcAdmin &&
            Status != AssetStatuses.RejectedByInfrabase)
        {
            return Result<bool>.Failure("Can only submit assets with status Draft, Rejected, or Returned for correction");
        }

        if (_capexDetails.Count == 0)
        {
            return Result<bool>.Failure("Cannot submit asset without CAPEX details");
        }

        if (_opexDetails.Count == 0)
        {
            return Result<bool>.Failure("Cannot submit asset without OPEX details");
        }

        if (CapexEntryMode == FinancialEntryMode.SingleYear && _capexDetails.Count != 1)
        {
            return Result<bool>.Failure("CAPEX Single-Year mode requires exactly one year row");
        }

        if (OpexEntryMode == FinancialEntryMode.SingleYear && _opexDetails.Count > 1)
        {
            return Result<bool>.Failure("OPEX Single-Year mode supports only one year row");
        }

        var previousStatus = Status;
        Status = isPcAdmin ? AssetStatuses.AcceptedByPcAdmin : AssetStatuses.Submitted;
        SubmittedBy = userId;
        SubmittedAt = DateTime.Now;
        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        RejectionReason = null;
        RejectedBy = null;
        RejectedAt = null;

        AssignAssetCode(assetCode);

        var action = previousStatus == AssetStatuses.Draft ? "Submitted" : "Resubmitted";
        var comments = isPcAdmin
            ? "Asset submitted by PC Admin and automatically accepted"
            : previousStatus == AssetStatuses.Draft
                ? "Asset submitted for PC Admin approval"
                : "Asset resubmitted after addressing rejection reasons";

        AddHistory(action, userId ?? "Admin", comments);
        AddDomainEvent(new AssetSubmittedEvent(Id, AssetCode, userId ?? "Admin", CompanyId, CreatedBy ?? userId ?? "Admin", !isPcAdmin));
        return Result<bool>.Success(true);
    }

    public Result<bool> AcceptByPcAdmin(string userId)
    {
        //TODO: Review
        //if (Status != AssetStatuses.Submitted || Status == AssetStatuses.Draft)
        //{
        //    return Result<bool>.Failure("Only submitted assets can be accepted by PC Admin");
        //}

        if (string.IsNullOrEmpty(AssetCode))
        {
            return Result<bool>.Failure("Asset must have an asset code");
        }

        Status = AssetStatuses.AcceptedByPcAdmin;
        ApprovedBy = userId;
        ApprovedAt = DateTime.Now;
        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("Accepted by PC Admin", userId, "Asset accepted and forwarded to Infrabase admin");
        AddDomainEvent(new AssetAcceptedByPcAdminEvent(Id, AssetCode, userId ?? "Admin", CreatedBy ?? userId ?? "Admin", CompanyId));
        return Result<bool>.Success(true);
    }

    public Result<bool> RejectByPcAdmin(string userId, string rejectionReason)
    {
        if (Status != AssetStatuses.Submitted)
        {
            return Result<bool>.Failure("Only submitted assets can be rejected by PC Admin");
        }

        if (string.IsNullOrEmpty(AssetCode))
        {
            return Result<bool>.Failure("Asset must have an asset code");
        }

        var rejectionReasonResult = RejectionReason.Create(rejectionReason);
        if (rejectionReasonResult.IsFailure)
        {
            return Result<bool>.Failure(rejectionReasonResult.Error!);
        }

        Status = AssetStatuses.RejectedByPcAdmin;
        RejectionReason = rejectionReasonResult.Value;
        RejectedBy = userId;
        RejectedAt = DateTime.Now;
        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("Rejected by PC Admin", !string.IsNullOrWhiteSpace(userId) ? userId: "Admin", rejectionReason);
        AddDomainEvent(new AssetRejectedByPcAdminEvent(Id, AssetCode, rejectionReason, !string.IsNullOrWhiteSpace(userId) ? userId : "Admin", CreatedBy ?? userId ?? "Admin"));
        return Result<bool>.Success(true);
    }

    public Result<bool> CheckByInfrabaseAdmin(string userId)
    {
        if (Status != AssetStatuses.AcceptedByPcAdmin)
        {
            return Result<bool>.Failure("Only PC Admin accepted assets can be checked by Infrabase Admin");
        }

        if (string.IsNullOrEmpty(AssetCode))
        {
            return Result<bool>.Failure("Asset must have an asset code");
        }

        Status = AssetStatuses.AcceptedByInfrabase;
        ApprovedBy = userId;
        ApprovedAt = DateTime.Now;
        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("Checked by Infrabase Admin", !string.IsNullOrWhiteSpace(userId) ? userId : "Admin", "Asset checked and approved - Final approval");
        AddDomainEvent(new AssetCheckedByInfrabaseAdminEvent(Id, AssetCode, !string.IsNullOrWhiteSpace(userId) ? userId : "Admin", CreatedBy ?? userId ?? "Admin", CompanyId));
        return Result<bool>.Success(true);
    }

    public Result<bool> ReturnForCorrectionByInfrabaseAdmin(string userId, string correctionReason)
    {
        if (Status != AssetStatuses.AcceptedByPcAdmin)
        {
            return Result<bool>.Failure("Only PC Admin accepted assets can be returned for correction by Infrabase Admin");
        }

        if (string.IsNullOrEmpty(AssetCode))
        {
            return Result<bool>.Failure("Asset must have an asset code");
        }

        var rejectionReasonResult = RejectionReason.Create(correctionReason);
        if (rejectionReasonResult.IsFailure)
        {
            return Result<bool>.Failure(rejectionReasonResult.Error!);
        }

        Status = AssetStatuses.RejectedByInfrabase;
        RejectionReason = rejectionReasonResult.Value;
        RejectedBy = userId;
        RejectedAt = DateTime.Now;
        UpdatedBy = userId;
        UpdatedAt = DateTime.Now;
        AddHistory("Returned for Correction by Infrabase Admin", userId ?? "Admin", correctionReason);
        AddDomainEvent(new AssetReturnedForCorrectionByInfrabaseAdminEvent(Id, AssetCode, correctionReason, userId ?? "Admin", CreatedBy ?? userId ?? "Admin", CompanyId));
        return Result<bool>.Success(true);
    }

    public Result<bool> UpdateAssetInformation(string? assetName, string? locationCity,
        Guid? sectorId, Guid? subSectorId, Guid? assetTypeId, string? assetTypeOther,
        decimal? quantityOfAsset, decimal? capacityPerAsset, Guid? unitOfMeasurementId,
        string? unitOfMeasurementOther, string? description,
        int? constructionStartingQuarter, int? constructionStartingYear,
        int? constructionCompletionQuarter, int? constructionCompletionYear,
        TenderingStages? tenderingStage, DevelopmentTypes? developmentType,
        FundingModels? fundingModel, decimal? expectedDebt, decimal? expectedEquity,
        bool? isRevenueGenerating, decimal? irr, bool? isPifGuaranteesRequired, string userId)
    {
        if (Status != AssetStatuses.Draft && Status != AssetStatuses.RejectedByPcAdmin &&
            Status != AssetStatuses.RejectedByInfrabase)
        {
            return Result<bool>.Failure("Only draft, rejected, or returned for correction assets can be updated");
        }

        var changes = new List<string>();
        var oldValues = new List<string>();
        var newValues = new List<string>();

        if (assetName != null)
        {
            var assetNameResult = AssetName.Create(assetName);
            if (assetNameResult.IsFailure)
            {
                return Result<bool>.Failure(assetNameResult.Error!);
            }

            if (AssetName.Value != assetName)
            {
                changes.Add("AssetName");
                oldValues.Add(AssetName.Value);
                newValues.Add(assetName);
                AssetName = assetNameResult.Value!;
            }
        }

        if (locationCity != null)
        {
            var locationCityResult = LocationCity.Create(locationCity);
            if (locationCityResult.IsFailure)
            {
                return Result<bool>.Failure(locationCityResult.Error!);
            }

            if (LocationCity.Value != locationCity)
            {
                changes.Add("LocationCity");
                oldValues.Add(LocationCity.Value);
                newValues.Add(locationCity);
                LocationCity = locationCityResult.Value!;
            }
        }

        if (sectorId.HasValue)
        {
            if (sectorId.Value == Guid.Empty)
            {
                return Result<bool>.Failure("Sector is required");
            }

            var normalizedSectorId = sectorId.Value;
            if (SectorId != normalizedSectorId)
            {
                changes.Add("SectorId");
                oldValues.Add(SectorId?.ToString() ?? "Not set");
                newValues.Add(normalizedSectorId.ToString());
                SectorId = normalizedSectorId;
            }
        }

        if (subSectorId.HasValue)
        {
            if (subSectorId.Value == Guid.Empty)
            {
                return Result<bool>.Failure("Sub sector is required");
            }

            var normalizedSubSectorId = subSectorId.Value;
            if (SubSectorId != normalizedSubSectorId)
            {
                changes.Add("SubSectorId");
                oldValues.Add(SubSectorId?.ToString() ?? "Not set");
                newValues.Add(normalizedSubSectorId.ToString());
                SubSectorId = normalizedSubSectorId;
            }
        }

        if (assetTypeId.HasValue)
        {
            var normalizedAssetTypeId = assetTypeId.Value == Guid.Empty ? (Guid?)null : assetTypeId.Value;
            if (AssetTypeId != normalizedAssetTypeId)
            {
                changes.Add("AssetTypeId");
                oldValues.Add(AssetTypeId?.ToString() ?? "Not set");
                newValues.Add(normalizedAssetTypeId?.ToString() ?? "Not set");
                AssetTypeId = normalizedAssetTypeId;
            }
        }

        if (assetTypeOther != null && AssetTypeOther != assetTypeOther)
        {
            changes.Add("AssetTypeOther");
            oldValues.Add(AssetTypeOther ?? "");
            newValues.Add(assetTypeOther);
            AssetTypeOther = assetTypeOther;
            
            if (!string.IsNullOrWhiteSpace(assetTypeOther))
            {
                AssetTypeId = null;
            }
        }

        var normalizedQuantity = quantityOfAsset == 0 ? null : quantityOfAsset;

        if (normalizedQuantity.HasValue)
        {
            if (normalizedQuantity.Value < 0)
            {
                return Result<bool>.Failure("Quantity of asset cannot be negative");
            }

            if (normalizedQuantity.Value.ToString().Replace(".", "").Replace(",", "").Length > 5)
            {
                return Result<bool>.Failure("Quantity of asset cannot exceed 5 digits");
            }

            if (QuantityOfAsset != normalizedQuantity.Value)
            {
                changes.Add("QuantityOfAsset");
                oldValues.Add(QuantityOfAsset?.ToString() ?? "Not set");
                newValues.Add(normalizedQuantity.Value.ToString());
                QuantityOfAsset = normalizedQuantity.Value;
            }
        }

        if (capacityPerAsset.HasValue)
        {
            if (capacityPerAsset.Value <= 0)
            {
                return Result<bool>.Failure("Capacity per asset must be greater than zero");
            }

            if (capacityPerAsset.Value.ToString().Replace(".", "").Replace(",", "").Length > 5)
            {
                return Result<bool>.Failure("Capacity per asset cannot exceed 5 digits");
            }

            if (CapacityPerAsset != capacityPerAsset.Value)
            {
                changes.Add("CapacityPerAsset");
                oldValues.Add(CapacityPerAsset.ToString());
                newValues.Add(capacityPerAsset.Value.ToString());
                CapacityPerAsset = capacityPerAsset.Value;
            }
        }

        if (unitOfMeasurementId.HasValue)
        {
            var normalizedUomId = unitOfMeasurementId.Value == Guid.Empty ? (Guid?)null : unitOfMeasurementId.Value;
            if (UnitOfMeasurementId != normalizedUomId)
            {
                changes.Add("UnitOfMeasurementId");
                oldValues.Add(UnitOfMeasurementId?.ToString() ?? "Not set");
                newValues.Add(normalizedUomId?.ToString() ?? "Not set");
                UnitOfMeasurementId = normalizedUomId;
            }
        }

        if (unitOfMeasurementOther != null && UnitOfMeasurementOther != unitOfMeasurementOther)
        {
            changes.Add("UnitOfMeasurementOther");
            oldValues.Add(UnitOfMeasurementOther ?? "");
            newValues.Add(unitOfMeasurementOther);
            UnitOfMeasurementOther = unitOfMeasurementOther;
            
            if (!string.IsNullOrWhiteSpace(unitOfMeasurementOther))
            {
                UnitOfMeasurementId = null;
            }
        }

        if (description != null)
        {
            var descriptionResult = AssetDescription.Create(description);
            if (descriptionResult.IsFailure)
            {
                return Result<bool>.Failure(descriptionResult.Error!);
            }

            var oldDesc = Description?.Value ?? "";
            if (oldDesc != description)
            {
                changes.Add("Description");
                oldValues.Add(oldDesc);
                newValues.Add(description);
                Description = descriptionResult.Value!;
            }
        }

        var normalizedStartQuarter = constructionStartingQuarter == 0 ? null : constructionStartingQuarter;
        var normalizedStartYear = constructionStartingYear == 0 ? null : constructionStartingYear;
        var normalizedEndQuarter = constructionCompletionQuarter == 0 ? null : constructionCompletionQuarter;
        var normalizedEndYear = constructionCompletionYear == 0 ? null : constructionCompletionYear;

        if (normalizedStartQuarter.HasValue)
        {
            if (normalizedStartQuarter.Value < 1 || normalizedStartQuarter.Value > 4)
            {
                return Result<bool>.Failure("Construction starting quarter must be between 1 and 4");
            }

            if (ConstructionStartingQuarter != normalizedStartQuarter.Value)
            {
                changes.Add("ConstructionStartingQuarter");
                oldValues.Add(ConstructionStartingQuarter?.ToString() ?? "Not set");
                newValues.Add(normalizedStartQuarter.Value.ToString());
                ConstructionStartingQuarter = normalizedStartQuarter.Value;
            }
        }

        if (normalizedStartYear.HasValue)
        {
            if (normalizedStartYear.Value < 2015 || normalizedStartYear.Value > 2099)
            {
                return Result<bool>.Failure("Construction starting year must be between 2015 and 2099");
            }

            if (ConstructionStartingYear != normalizedStartYear.Value)
            {
                changes.Add("ConstructionStartingYear");
                oldValues.Add(ConstructionStartingYear?.ToString() ?? "Not set");
                newValues.Add(normalizedStartYear.Value.ToString());
                ConstructionStartingYear = normalizedStartYear.Value;
            }
        }

        if (normalizedEndQuarter.HasValue)
        {
            if (normalizedEndQuarter.Value < 1 || normalizedEndQuarter.Value > 4)
            {
                return Result<bool>.Failure("Construction completion quarter must be between 1 and 4");
            }

            if (ConstructionCompletionQuarter != normalizedEndQuarter.Value)
            {
                changes.Add("ConstructionCompletionQuarter");
                oldValues.Add(ConstructionCompletionQuarter?.ToString() ?? "Not set");
                newValues.Add(normalizedEndQuarter.Value.ToString());
                ConstructionCompletionQuarter = normalizedEndQuarter.Value;
            }
        }

        if (normalizedEndYear.HasValue)
        {
            if (normalizedEndYear.Value < 2015 || normalizedEndYear.Value > 2099)
            {
                return Result<bool>.Failure("Construction completion year must be between 2015 and 2099");
            }

            if (ConstructionCompletionYear != normalizedEndYear.Value)
            {
                changes.Add("ConstructionCompletionYear");
                oldValues.Add(ConstructionCompletionYear?.ToString() ?? "Not set");
                newValues.Add(normalizedEndYear.Value.ToString());
                ConstructionCompletionYear = normalizedEndYear.Value;
            }
        }

        if (ConstructionStartingYear.HasValue && ConstructionCompletionYear.HasValue)
        {
            if (ConstructionCompletionYear.Value < ConstructionStartingYear.Value)
            {
                return Result<bool>.Failure("Construction completion date must be after starting date");
            }

            if (ConstructionCompletionYear.Value == ConstructionStartingYear.Value &&
                ConstructionStartingQuarter.HasValue && ConstructionCompletionQuarter.HasValue &&
                ConstructionCompletionQuarter.Value < ConstructionStartingQuarter.Value)
            {
                return Result<bool>.Failure("Construction completion date must be after starting date");
            }
        }

        if (tenderingStage.HasValue && TenderingStage != tenderingStage.Value)
        {
            changes.Add("TenderingStage");
            oldValues.Add(TenderingStage.ToString());
            newValues.Add(tenderingStage.Value.ToString());
            TenderingStage = tenderingStage.Value;
        }

        if (developmentType.HasValue && DevelopmentType != developmentType.Value)
        {
            changes.Add("DevelopmentType");
            oldValues.Add(DevelopmentType.ToString());
            newValues.Add(developmentType.Value.ToString());
            DevelopmentType = developmentType.Value;
        }

        if (fundingModel.HasValue && FundingModel != fundingModel.Value)
        {
            changes.Add("FundingModel");
            oldValues.Add(FundingModel.ToString());
            newValues.Add(fundingModel.Value.ToString());
            FundingModel = fundingModel.Value;
        }

        if (expectedDebt.HasValue)
        {
            if (expectedDebt.Value < 0)
            {
                return Result<bool>.Failure("Expected debt cannot be negative");
            }

            if (ExpectedDebt != expectedDebt.Value)
            {
                changes.Add("ExpectedDebt");
                oldValues.Add(ExpectedDebt?.ToString() ?? "Not set");
                newValues.Add(expectedDebt.Value.ToString());
                ExpectedDebt = expectedDebt.Value;
            }
        }

        if (expectedEquity.HasValue)
        {
            if (expectedEquity.Value < 0)
            {
                return Result<bool>.Failure("Expected equity cannot be negative");
            }

            if (ExpectedEquity != expectedEquity.Value)
            {
                changes.Add("ExpectedEquity");
                oldValues.Add(ExpectedEquity?.ToString() ?? "Not set");
                newValues.Add(expectedEquity.Value.ToString());
                ExpectedEquity = expectedEquity.Value;
            }
        }

        if (isRevenueGenerating.HasValue && IsRevenueGenerating != isRevenueGenerating.Value)
        {
            changes.Add("IsRevenueGenerating");
            oldValues.Add(IsRevenueGenerating?.ToString() ?? "Not set");
            newValues.Add(isRevenueGenerating.Value.ToString());
            IsRevenueGenerating = isRevenueGenerating.Value;
        }

        if (irr.HasValue)
        {
            if (irr.Value < 0)
            {
                return Result<bool>.Failure("IRR cannot be negative");
            }

            if (IRR != irr.Value)
            {
                changes.Add("IRR");
                oldValues.Add(IRR?.ToString() ?? "Not set");
                newValues.Add(irr.Value.ToString());
                IRR = irr.Value;
            }
        }

        if (isPifGuaranteesRequired.HasValue && IsPifGuaranteesRequired != isPifGuaranteesRequired.Value)
        {
            changes.Add("IsPifGuaranteesRequired");
            oldValues.Add(IsPifGuaranteesRequired?.ToString() ?? "Not set");
            newValues.Add(isPifGuaranteesRequired.Value.ToString());
            IsPifGuaranteesRequired = isPifGuaranteesRequired.Value;
        }

        if (changes.Any())
        {
            UpdatedBy = userId;
            UpdatedAt = DateTime.Now;
            AddHistory("Updated", userId, "Asset information updated",
                string.Join(", ", changes), string.Join(", ", oldValues), string.Join(", ", newValues));
        }

        return Result<bool>.Success(true);
    }

    public Result<AssetAttachment> AddAttachment(string fileName, long fileSizeInBytes,
        string contentType, string sharePointUrl, string uploadedBy)
    {
        if (Status == AssetStatuses.AcceptedByInfrabase)
        {
            return Result<AssetAttachment>.Failure("Cannot add attachments to accepted assets");
        }

        try
        {
            var attachment = new AssetAttachment(Id, fileName, fileSizeInBytes,
                contentType, sharePointUrl, uploadedBy);

            _attachments.Add(attachment);
            UpdatedBy = uploadedBy;
            UpdatedAt = DateTime.Now;
            AddHistory("Attachment Added", uploadedBy,
                $"Attachment '{fileName}' ({fileSizeInBytes / 1024:N0} KB) uploaded");
            return Result<AssetAttachment>.Success(attachment);
        }
        catch (ArgumentException ex)
        {
            return Result<AssetAttachment>.Failure(ex.Message);
        }
    }

    public Result<bool> RemoveAttachment(Guid attachmentId, string deletedBy)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId && !a.IsDeleted);
        if (attachment == null)
        {
            return Result<bool>.Failure("Attachment not found");
        }

        if (Status == AssetStatuses.AcceptedByInfrabase)
        {
            return Result<bool>.Failure("Cannot remove attachments from accepted assets");
        }

        var result = attachment.MarkAsDeleted(deletedBy);
        if (result.IsFailure)
        {
            return result;
        }

        UpdatedAt = DateTime.Now;
        AddHistory("Attachment Removed", deletedBy, $"Attachment '{attachment.Metadata.FileName}' removed");
        return Result<bool>.Success(true);
    }

    public IReadOnlyCollection<AssetAttachment> GetAttachments()
    {
        return _attachments.Where(a => !a.IsDeleted).ToList().AsReadOnly();
    }

    public AssetAttachment? GetAttachment(Guid attachmentId)
    {
        return _attachments.FirstOrDefault(a => a.Id == attachmentId && !a.IsDeleted);
    }

    private void AddHistory(string action, string performedBy, string? comments = null,
        string? fieldsChanged = null, string? oldValues = null, string? newValues = null)
    {
        var history = new AssetHistory(Id, Status, action, performedBy, comments,
            fieldsChanged, oldValues, newValues);
        _history.Add(history);
    }
}
