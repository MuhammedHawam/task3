using System;
using System.Threading;
using System.Threading.Tasks;

namespace PartnersHub.InfraBase.Application.Common.Interfaces;

public interface IConfigurationLookupService
{
    Task<string?> GetSectorNameAsync(Guid sectorId, CancellationToken cancellationToken = default);
    Task<string?> GetSubSectorNameAsync(Guid subSectorId, CancellationToken cancellationToken = default);
    Task<string?> GetAssetTypeNameAsync(Guid assetTypeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, string>> GetAssetTypeSearchValuesAsync(
        CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, Guid>> GetAssetTypeIdsByCodeAsync(
        CancellationToken cancellationToken = default);
    Task<string?> GetUomNameAsync(Guid uomId, CancellationToken cancellationToken = default);
    Task<Guid?> GetOtherSectorIdAsync(CancellationToken cancellationToken = default);
    Task<Guid?> GetOtherSubSectorIdAsync(Guid sectorId, CancellationToken cancellationToken = default);
    Task<Guid?> GetOtherAssetTypeIdAsync(CancellationToken cancellationToken = default);
    Task<Guid?> GetOtherUomIdAsync(CancellationToken cancellationToken = default);
}