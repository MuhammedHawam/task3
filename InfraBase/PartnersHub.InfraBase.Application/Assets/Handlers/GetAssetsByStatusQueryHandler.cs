using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetsByStatusQueryHandler : IRequestHandler<GetAssetsByStatusQuery, List<AssetListDto>>
{
    private readonly IAssetRepository _repository;
    private readonly ITokenService _tokenService;
    private readonly IAssetListProjectionService _assetListProjectionService;

    public GetAssetsByStatusQueryHandler(
        IAssetRepository repository,
        ITokenService tokenService,
        IAssetListProjectionService assetListProjectionService)
    {
        _repository = repository;
        _tokenService = tokenService;
        _assetListProjectionService = assetListProjectionService;
    }

    public async Task<List<AssetListDto>> Handle(GetAssetsByStatusQuery query,
        CancellationToken cancellationToken)
    {
        var requestingUser = _tokenService.GetUserName();
        var paginatedAssets = await _repository.GetPagedAsync(
            1,
            int.MaxValue,
            query.Status,
            query.CompanyId,
            null,
            null,
            false,
            null,
            requestingUser,
            cancellationToken);

        return await _assetListProjectionService.MapAsync(
            paginatedAssets.Items,
            includeOtherValuesForSectorAndSubSector: false,
            cancellationToken);
    }
}