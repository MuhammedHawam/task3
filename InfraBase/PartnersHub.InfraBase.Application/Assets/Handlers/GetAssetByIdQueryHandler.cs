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

    public GetAssetByIdQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService,
        IMiddlewareIntegrationService middlewareService)
    {
        _repository = repository;
        _lookupService = lookupService;
        _middlewareService = middlewareService;
    }

    public async Task<AssetDto?> Handle(GetAssetByIdQuery query,
        CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdWithDetailsAsync(query.Id, cancellationToken);
        if (asset == null)
        {
            return null;
        }

        var sectorName = asset.SectorId.HasValue && asset.SectorId.Value != Guid.Empty
            ? await _lookupService.GetSectorNameAsync(asset.SectorId.Value, cancellationToken)
            : null;
        sectorName = string.IsNullOrWhiteSpace(sectorName) ? "N/A" : sectorName;

        var subSectorName = asset.SubSectorId.HasValue && asset.SubSectorId.Value != Guid.Empty
            ? await _lookupService.GetSubSectorNameAsync(asset.SubSectorId.Value, cancellationToken)
            : null;
        subSectorName = string.IsNullOrWhiteSpace(subSectorName) ? "N/A" : subSectorName;

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

        return new AssetDto
        {
            Id = asset.Id,
            AssetCode = asset.AssetCode,
            AssetName = asset.AssetName.Value,
            LocationCity = asset.LocationCity.Value,
            SectorId = asset.SectorId,
            SectorName = sectorName,
            SubSectorId = asset.SubSectorId,
            SubSectorName = subSectorName,
            AssetTypeId = asset.AssetTypeId,
            AssetTypeName = assetTypeName,
            // Only expose "Other" when no predefined lookup is selected.
            AssetTypeOther = asset.AssetTypeId.HasValue && asset.AssetTypeId.Value != Guid.Empty ? null : asset.AssetTypeOther,
            QuantityOfAsset = asset.QuantityOfAsset,
            CapacityPerAsset = asset.CapacityPerAsset,
            TotalCapacity = asset.TotalCapacity,
            UnitOfMeasurementId = asset.UnitOfMeasurementId,
            UnitOfMeasurementName = uomName,
            // Only expose "Other" when no predefined lookup is selected.
            UnitOfMeasurementOther = asset.UnitOfMeasurementId.HasValue && asset.UnitOfMeasurementId.Value != Guid.Empty ? null : asset.UnitOfMeasurementOther,
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
            ExpectedDebt = asset.ExpectedDebt,
            ExpectedEquity = asset.ExpectedEquity,
            IsRevenueGenerating = asset.IsRevenueGenerating,
            IRR = asset.IRR,
            IsPifGuaranteesRequired = asset.IsPifGuaranteesRequired,
            Status = asset.Status,
            SubmittedBy = asset.SubmittedBy,
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
            History = asset.History.Select(h => new AssetHistoryDto
            {
                Id = h.Id,
                Status = h.Status,
                Action = h.Action,
                PerformedBy = h.PerformedBy,
                PerformedAt = h.PerformedAt,
                Comments = h.Comments
            }).OrderBy(h => h.PerformedAt).ToList(),
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
}