using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetByIdQueryHandler : IRequestHandler<GetAssetByIdQuery, AssetDto?>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly IAssetSubmittedByResolver _assetSubmittedByResolver;

    public GetAssetByIdQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService,
        IMiddlewareIntegrationService middlewareService,
        IAssetSubmittedByResolver assetSubmittedByResolver)
    {
        _repository = repository;
        _lookupService = lookupService;
        _middlewareService = middlewareService;
        _assetSubmittedByResolver = assetSubmittedByResolver;
    }

    public async Task<AssetDto?> Handle(GetAssetByIdQuery query,
        CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdWithDetailsAsync(query.Id, cancellationToken);
        if (asset == null)
        {
            return null;
        }

        var usesSectorOther = !string.IsNullOrWhiteSpace(asset.SectorOther);
        var usesSubSectorOther = !string.IsNullOrWhiteSpace(asset.SubSectorOther);
        var usesAssetTypeOther = !string.IsNullOrWhiteSpace(asset.AssetTypeOther);
        var usesUomOther = !string.IsNullOrWhiteSpace(asset.UnitOfMeasurementOther);

        var sectorName = asset.SectorId.HasValue && asset.SectorId.Value != Guid.Empty
            ? await _lookupService.GetSectorNameAsync(asset.SectorId.Value, cancellationToken)
            : null;
        sectorName = string.IsNullOrWhiteSpace(sectorName)
            ? (asset.SectorOther ?? "N/A")
            : sectorName;

        var subSectorName = asset.SubSectorId.HasValue && asset.SubSectorId.Value != Guid.Empty
            ? await _lookupService.GetSubSectorNameAsync(asset.SubSectorId.Value, cancellationToken)
            : null;
        subSectorName = string.IsNullOrWhiteSpace(subSectorName)
            ? (asset.SubSectorOther ?? "N/A")
            : subSectorName;

        var assetTypeName = asset.AssetTypeId.HasValue && asset.AssetTypeId.Value != Guid.Empty
            ? await _lookupService.GetAssetTypeNameAsync(asset.AssetTypeId.Value, cancellationToken)
            : null;
        assetTypeName = string.IsNullOrWhiteSpace(assetTypeName)
            ? (asset.AssetTypeOther ?? "N/A")
            : assetTypeName;

        var uomName = asset.UnitOfMeasurementId.HasValue && asset.UnitOfMeasurementId.Value != Guid.Empty
            ? await _lookupService.GetUomNameAsync(asset.UnitOfMeasurementId.Value, cancellationToken)
            : null;
        uomName = string.IsNullOrWhiteSpace(uomName)
            ? (asset.UnitOfMeasurementOther ?? "N/A")
            : uomName;

        var effectiveSectorId = asset.SectorId;
        if (usesSectorOther)
        {
            var otherSectorId = await _lookupService.GetOtherSectorIdAsync(cancellationToken);
            if (otherSectorId.HasValue)
            {
                effectiveSectorId = otherSectorId.Value;
            }
        }

        var effectiveSubSectorId = asset.SubSectorId;
        if (usesSubSectorOther)
        {
            var sectorIdForOther = effectiveSectorId ?? asset.SectorId;
            if (sectorIdForOther.HasValue)
            {
                var otherSubSectorId = await _lookupService.GetOtherSubSectorIdAsync(
                    sectorIdForOther.Value,
                    cancellationToken);
                if (otherSubSectorId.HasValue)
                {
                    effectiveSubSectorId = otherSubSectorId.Value;
                }
            }
        }

        var effectiveUomId = asset.UnitOfMeasurementId;
        if (usesUomOther)
        {
            var otherUomId = await _lookupService.GetOtherUomIdAsync(cancellationToken);
            if (otherUomId.HasValue)
            {
                effectiveUomId = otherUomId.Value;
            }
        }

        var effectiveAssetTypeId = asset.AssetTypeId;
        if (usesAssetTypeOther)
        {
            var otherAssetTypeId = await _lookupService.GetOtherAssetTypeIdAsync(cancellationToken);
            if (otherAssetTypeId.HasValue)
            {
                effectiveAssetTypeId = otherAssetTypeId.Value;
            }
        }

        var companyName = asset.CompanyName;
        try
        {
            var company = await _middlewareService.GetCompanyByIdAsync(asset.CompanyId);
            if (!string.IsNullOrWhiteSpace(company?.Name))
            {
                companyName = company.Name;
            }
        }
        catch
        {
            // ignore and fall back to stored name
        }

        var submittedByDisplayName = await _assetSubmittedByResolver.ResolveAsync(
            asset.SubmittedBy,
            asset.CreatedBy,
            cancellationToken);
        var historyPerformedByNames = await _assetSubmittedByResolver.ResolveUserValuesAsync(
            asset.History.Select(h => h.PerformedBy),
            cancellationToken);

        var history = asset.History.Select(h =>
        {
            var normalizedPerformedBy = string.IsNullOrWhiteSpace(h.PerformedBy)
                ? null
                : h.PerformedBy.Trim();
            var resolvedPerformedBy = normalizedPerformedBy != null &&
                                      historyPerformedByNames.TryGetValue(normalizedPerformedBy, out var displayName)
                ? displayName
                : normalizedPerformedBy;

            return new AssetHistoryDto
            {
                Id = h.Id,
                Status = h.Status,
                StatusDisplayName = h.Status.GetDisplayName(),
                StatusShortDisplayName = h.Status.GetShortDisplayName(),
                Action = h.Action,
                PerformedBy = resolvedPerformedBy ?? h.PerformedBy,
                PerformedByName = resolvedPerformedBy,
                PerformedAt = h.PerformedAt,
                Comments = h.Comments
            };
        }).OrderBy(h => h.PerformedAt).ToList();

        return new AssetDto
        {
            Id = asset.Id,
            AssetCode = asset.AssetCode,
            AssetName = asset.AssetName.Value,
            LocationCity = asset.LocationCity.Value,
            SectorId = effectiveSectorId,
            SectorName = sectorName,
            // Keep "Other" text when it was used, even if we map to the lookup id for editing.
            SectorOther = usesSectorOther ? asset.SectorOther : null,
            SubSectorId = effectiveSubSectorId,
            SubSectorName = subSectorName,
            // Keep "Other" text when it was used, even if we map to the lookup id for editing.
            SubSectorOther = usesSubSectorOther ? asset.SubSectorOther : null,
            AssetTypeId = effectiveAssetTypeId,
            AssetTypeName = assetTypeName,
            // Keep "Other" text when it was used, even if we map to the lookup id for editing.
            AssetTypeOther = usesAssetTypeOther ? asset.AssetTypeOther : null,
            QuantityOfAsset = (double?)asset.QuantityOfAsset,
            CapacityPerAsset = (double?)asset.CapacityPerAsset,
            TotalCapacity = asset.TotalCapacity.ToString(),
            UnitOfMeasurementId = effectiveUomId,
            UnitOfMeasurementName = uomName,
            // Keep "Other" text when it was used, even if we map to the lookup id for editing.
            UnitOfMeasurementOther = usesUomOther ? asset.UnitOfMeasurementOther : null,
            Description = asset.Description?.Value,
            ConstructionStartingQuarter = asset.ConstructionStartingQuarter,
            ConstructionStartingYear = asset.ConstructionStartingYear,
            ConstructionCompletionQuarter = asset.ConstructionCompletionQuarter,
            ConstructionCompletionYear = asset.ConstructionCompletionYear,
            TenderingStage = asset.TenderingStage,
            DevelopmentType = asset.DevelopmentType,
            CapexEntryMode = asset.CapexEntryMode,
            OpexEntryMode = asset.OpexEntryMode,
            TotalCapex = asset.TotalCapex,
            TotalOpex = asset.TotalOpex,
            FundingModel = asset.FundingModel,
            ExpectedDebt = NormalizeOptionalDecimal(asset.ExpectedDebt),
            ExpectedEquity = NormalizeOptionalDecimal(asset.ExpectedEquity),
            IsRevenueGenerating = NormalizeOptionalBool(asset.IsRevenueGenerating),
            IRR = NormalizeOptionalDecimal(asset.IRR),
            IsPifGuaranteesRequired = NormalizeOptionalBool(asset.IsPifGuaranteesRequired),
            Status = asset.Status,
            SubmittedBy = submittedByDisplayName,
            SubmittedAt = asset.SubmittedAt,
            RejectionReason = asset.RejectionReason?.Value,
            RejectedBy = asset.RejectedBy,
            RejectedAt = asset.RejectedAt,
            ApprovedBy = asset.ApprovedBy,
            ApprovedAt = asset.ApprovedAt,
            CompanyId = asset.CompanyId,
            CompanyName = companyName,
            CreatedBy = asset.CreatedBy,
            CreatedAt = asset.CreatedAt,
            UpdatedBy = asset.UpdatedBy,
            UpdatedAt = asset.UpdatedAt,
            CapexDetails = asset.CapexDetails.Select(c => new AssetCapexDto
            {
                Id = c.Id,
                Year = c.Year,
                Amount = c.Amount
            }).ToList(),
            OpexDetails = asset.OpexDetails.Select(o => new AssetOpexDto
            {
                Id = o.Id,
                Year = o.Year,
                Amount = o.Amount
            }).ToList(),
            History = history,
            Attachments = asset.GetAttachments().Select(a => new AssetAttachmentDto
            {
                Id = a.Id,
                FileName = a.Metadata.FileName,
                FileSizeInBytes = a.Metadata.FileSizeInBytes,
                ContentType = a.Metadata.ContentType,
                SharePointUrl = a.SharePointUrl,
                UploadedBy = a.UploadedBy,
                UploadedAt = a.UploadedAt
            }).ToList()
        };
    }
    private static decimal? NormalizeOptionalDecimal(decimal? value)
    {
        return value.HasValue && value.Value != 0 ? value : null;
    }

    private static bool? NormalizeOptionalBool(bool? value)
    {
        return value == true ? true : null;
    }
}