using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

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
