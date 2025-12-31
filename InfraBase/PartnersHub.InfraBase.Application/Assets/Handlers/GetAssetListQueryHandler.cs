using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Application.Common.Models;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

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
