using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

namespace PartnersHub.InfraBase.Application.Common.Interfaces;

public interface IAssetSubmittedByResolver
{
    Task<string?> ResolveAsync(
        string? submittedBy,
        string? createdBy,
        CancellationToken cancellationToken = default);

    Task<string?> ResolveUserValueAsync(
        string? userValue,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, string?>> ResolveForAssetsAsync(
        IEnumerable<Asset> assets,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string?>> ResolveUserValuesAsync(
        IEnumerable<string?> userValues,
        CancellationToken cancellationToken = default);
}
