using System;
using System.Threading;
using System.Threading.Tasks;

namespace PartnersHub.InfraBase.Application.Common.Interfaces;

public interface IConfigurationLookupService
{
    Task<string?> GetSectorNameAsync(Guid sectorId, CancellationToken cancellationToken = default);
    Task<string?> GetSubSectorNameAsync(Guid subSectorId, CancellationToken cancellationToken = default);
    Task<string?> GetAssetTypeNameAsync(Guid assetTypeId, CancellationToken cancellationToken = default);
    Task<string?> GetUomNameAsync(Guid uomId, CancellationToken cancellationToken = default);

    // Code-based lookups (stable across ConfigurationHub reseeding)
    Task<string?> GetSectorNameByCodeAsync(string sectorCode, CancellationToken cancellationToken = default);
    Task<string?> GetSubSectorNameByCodeAsync(string sectorCode, string subSectorCode, CancellationToken cancellationToken = default);
    Task<string?> GetAssetTypeNameByCodeAsync(string assetTypeCode, CancellationToken cancellationToken = default);
    Task<string?> GetUomNameByCodeAsync(string uomCode, CancellationToken cancellationToken = default);

    // Resolve codes from IDs (to persist stable references in InfraBase)
    Task<string?> GetSectorCodeAsync(Guid sectorId, CancellationToken cancellationToken = default);
    Task<string?> GetSubSectorCodeAsync(Guid subSectorId, CancellationToken cancellationToken = default);
    Task<string?> GetAssetTypeCodeAsync(Guid assetTypeId, CancellationToken cancellationToken = default);
    Task<string?> GetUomCodeAsync(Guid uomId, CancellationToken cancellationToken = default);
}