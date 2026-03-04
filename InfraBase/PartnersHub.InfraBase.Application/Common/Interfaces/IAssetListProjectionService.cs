using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

namespace PartnersHub.InfraBase.Application.Common.Interfaces;

public interface IAssetListProjectionService
{
    Task<List<AssetListDto>> MapAsync(
        IReadOnlyCollection<Asset> assets,
        bool includeOtherValuesForSectorAndSubSector,
        CancellationToken cancellationToken = default);
}
