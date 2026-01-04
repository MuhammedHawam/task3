using MediatR;
using Microsoft.Extensions.Options;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Application.Common.Models;
using PartnersHub.InfraBase.Application.Common.Options;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetByIdQueryHandler : IRequestHandler<GetAssetByIdQuery, AssetDto?>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;

    public GetAssetByIdQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService)
    {
        _repository = repository;
        _lookupService = lookupService;
    }

    public async Task<AssetDto?> Handle(GetAssetByIdQuery query, 
        CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdWithDetailsAsync(query.Id, cancellationToken);
        if (asset == null)
        {
            return null;
        }

        var sectorName = asset.SectorId.HasValue 
            ? await _lookupService.GetSectorNameAsync(asset.SectorId.Value, cancellationToken)
            : "N/A";
        var subSectorName = asset.SubSectorId.HasValue 
            ? await _lookupService.GetSubSectorNameAsync(asset.SubSectorId.Value, cancellationToken)
            : "N/A";
        var assetTypeName = asset.AssetTypeId.HasValue 
            ? await _lookupService.GetAssetTypeNameAsync(asset.AssetTypeId.Value, cancellationToken)
            : asset.AssetTypeOther ?? "N/A";
        var uomName = asset.UnitOfMeasurementId.HasValue 
            ? await _lookupService.GetUomNameAsync(asset.UnitOfMeasurementId.Value, cancellationToken)
            : asset.UnitOfMeasurementOther ?? "N/A";

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
            AssetTypeOther = asset.AssetTypeOther,
            QuantityOfAsset = asset.QuantityOfAsset,
            CapacityPerAsset = asset.CapacityPerAsset,
            TotalCapacity = asset.TotalCapacity,
            UnitOfMeasurementId = asset.UnitOfMeasurementId,
            UnitOfMeasurementName = uomName,
            UnitOfMeasurementOther = asset.UnitOfMeasurementOther,
            Description = asset.Description?.Value,
            ConstructionStartingQuarter = asset.ConstructionStartingQuarter,
            ConstructionStartingYear = asset.ConstructionStartingYear,
            ConstructionCompletionQuarter = asset.ConstructionCompletionQuarter,
            ConstructionCompletionYear = asset.ConstructionCompletionYear,
            TenderingStage = asset.TenderingStage,
            DevelopmentType = asset.DevelopmentType,
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
            CompanyName = asset.CompanyName,
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
            }).OrderBy(h=>h.PerformedAt).ToList(),
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

public class GetAssetListQueryHandler : IRequestHandler<GetAssetListQuery, PaginatedList<AssetListDto>>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;

    public GetAssetListQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService)
    {
        _repository = repository;
        _lookupService = lookupService;
    }

    public async Task<PaginatedList<AssetListDto>> Handle(GetAssetListQuery query, 
        CancellationToken cancellationToken)
    {
        var paginatedAssets = await _repository.GetPagedAsync(
            query.PageNumber, 
            query.PageSize, 
            query.Status, 
            query.CompanyId, 
            query.SearchTerm,
            query.SortBy,
            query.SortDescending,
            cancellationToken);

        var items = new List<AssetListDto>();
        
        foreach (var asset in paginatedAssets.Items)
        {
            var sectorName = asset.SectorId.HasValue 
                ? await _lookupService.GetSectorNameAsync(asset.SectorId.Value, cancellationToken)
                : "N/A";
            var subSectorName = asset.SubSectorId.HasValue 
                ? await _lookupService.GetSubSectorNameAsync(asset.SubSectorId.Value, cancellationToken)
                : "N/A";
            var assetTypeName = asset.AssetTypeId.HasValue 
                ? await _lookupService.GetAssetTypeNameAsync(asset.AssetTypeId.Value, cancellationToken)
                : asset.AssetTypeOther ?? "N/A";
            
            items.Add(new AssetListDto
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName.Value,
                SectorName = sectorName,
                SubSectorName = subSectorName,
                AssetTypeName = assetTypeName,
                Status = asset.Status,
                SubmittedAt = asset.SubmittedAt,
                SubmittedBy = asset.SubmittedBy,
                TotalCapex = asset.TotalCapex,
                TotalOpex = asset.TotalOpex,
                CompanyName = asset.CompanyName,
                CreatedAt = asset.CreatedAt
            });
        }

        return new PaginatedList<AssetListDto>(
            items, 
            paginatedAssets.TotalCount, 
            query.PageNumber, 
            query.PageSize);
    }
}

public class GetAssetSummaryQueryHandler : IRequestHandler<GetAssetSummaryQuery, AssetSummaryDto>
{
    private readonly IAssetRepository _repository;

    public GetAssetSummaryQueryHandler(IAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<AssetSummaryDto> Handle(GetAssetSummaryQuery query, 
        CancellationToken cancellationToken)
    {
        var statusCounts = await _repository.GetStatusCountsAsync(query.CompanyId, 
            cancellationToken);

        return new AssetSummaryDto
        {
            TotalAssets = statusCounts.Values.Sum(),
            DraftAssets = statusCounts.GetValueOrDefault(AssetStatuses.Draft, 0),
            SubmittedAssets = statusCounts.GetValueOrDefault(AssetStatuses.Submitted, 0),
            PcAdminApprovedAssets = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByPcAdmin, 0),
            RejectedAssets = statusCounts.GetValueOrDefault(AssetStatuses.RejectedByPcAdmin, 0),
            CheckedAssets = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByInfrabase, 0),
            ReturnedForCorrectionAssets = statusCounts.GetValueOrDefault(
                AssetStatuses.RejectedByInfrabase, 0)
        };
    }
}

public class GetAssetsByStatusQueryHandler : IRequestHandler<GetAssetsByStatusQuery, List<AssetListDto>>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;

    public GetAssetsByStatusQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService)
    {
        _repository = repository;
        _lookupService = lookupService;
    }

    public async Task<List<AssetListDto>> Handle(GetAssetsByStatusQuery query, 
        CancellationToken cancellationToken)
    {
        var paginatedAssets = await _repository.GetPagedAsync(
            1, 
            int.MaxValue, 
            query.Status, 
            query.CompanyId, 
            null,
            null,
            false,
            cancellationToken);

        var items = new List<AssetListDto>();
        
        foreach (var asset in paginatedAssets.Items)
        {
            var sectorName = asset.SectorId.HasValue 
                ? await _lookupService.GetSectorNameAsync(asset.SectorId.Value, cancellationToken)
                : "N/A";
            var subSectorName = asset.SubSectorId.HasValue 
                ? await _lookupService.GetSubSectorNameAsync(asset.SubSectorId.Value, cancellationToken)
                : "N/A";
            var assetTypeName = asset.AssetTypeId.HasValue 
                ? await _lookupService.GetAssetTypeNameAsync(asset.AssetTypeId.Value, cancellationToken)
                : asset.AssetTypeOther ?? "N/A";
            
            items.Add(new AssetListDto
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName.Value,
                SectorName = sectorName,
                SubSectorName = subSectorName,
                AssetTypeName = assetTypeName,
                Status = asset.Status,
                SubmittedAt = asset.SubmittedAt,
                SubmittedBy = asset.SubmittedBy,
                TotalCapex = asset.TotalCapex,
                TotalOpex = asset.TotalOpex,
                CompanyName = asset.CompanyName,
                CreatedAt = asset.CreatedAt
            });
        }

        return items;
    }
}

public class GetAssetHistoryQueryHandler : IRequestHandler<GetAssetHistoryQuery, List<AssetHistoryDto>>
{
    private readonly IAssetRepository _repository;

    public GetAssetHistoryQueryHandler(IAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AssetHistoryDto>> Handle(GetAssetHistoryQuery query, 
        CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdWithDetailsAsync(query.AssetId, cancellationToken);
        
        if (asset == null)
            return new List<AssetHistoryDto>();

        return asset.History
            .OrderBy(h => h.PerformedAt)
            .Select(h => new AssetHistoryDto
            {
                Id = h.Id,
                Status = h.Status,
                Action = h.Action,
                PerformedBy = h.PerformedBy,
                PerformedAt = h.PerformedAt,
                Comments = h.Comments
            })
            .ToList();
    }
}

public class GetAssetAttachmentsQueryHandler : IRequestHandler<GetAssetAttachmentsQuery, List<AssetAttachmentDto>>
{
    private readonly IAssetRepository _repository;

    public GetAssetAttachmentsQueryHandler(IAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AssetAttachmentDto>> Handle(GetAssetAttachmentsQuery query, 
        CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdWithDetailsAsync(query.AssetId, cancellationToken);
        
        if (asset == null)
            return new List<AssetAttachmentDto>();

        return asset.GetAttachments()
            .Select(a => new AssetAttachmentDto
            {
                Id = a.Id,
                FileName = a.Metadata.FileName,
                FileSizeInBytes = a.Metadata.FileSizeInBytes,
                ContentType = a.Metadata.ContentType,
                SharePointUrl = a.SharePointUrl,
                UploadedBy = a.UploadedBy,
                UploadedAt = a.UploadedAt
            })
            .ToList();
    }
}

public class GetNextAssetCodeQueryHandler : IRequestHandler<GetNextAssetCodeQuery, string>
{
    private readonly IAssetRepository _repository;
    private readonly AssetCodeSettings _assetCodeSettings;

    public GetNextAssetCodeQueryHandler(
        IAssetRepository repository, 
        IOptions<AssetCodeSettings> assetCodeSettings)
    {
        _repository = repository;
        _assetCodeSettings = assetCodeSettings.Value;
    }

    public async Task<string> Handle(GetNextAssetCodeQuery query, 
        CancellationToken cancellationToken)
    {
        var nextNumber = await _repository.GetNextAssetNumberAsync(cancellationToken);
        return _assetCodeSettings.GenerateCode(nextNumber);
    }
}
