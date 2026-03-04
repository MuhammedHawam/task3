using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;
using System.Collections.Concurrent;

namespace PartnersHub.InfraBase.Infrastructure.Services;

public class AssetListProjectionService : IAssetListProjectionService
{
    private const int MaxConcurrency = 8;

    private readonly IConfigurationLookupService _lookupService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly IAssetSubmittedByResolver _assetSubmittedByResolver;

    public AssetListProjectionService(
        IConfigurationLookupService lookupService,
        IMiddlewareIntegrationService middlewareService,
        IAssetSubmittedByResolver assetSubmittedByResolver)
    {
        _lookupService = lookupService;
        _middlewareService = middlewareService;
        _assetSubmittedByResolver = assetSubmittedByResolver;
    }

    public async Task<List<AssetListDto>> MapAsync(
        IReadOnlyCollection<Asset> assets,
        bool includeOtherValuesForSectorAndSubSector,
        CancellationToken cancellationToken = default)
    {
        if (assets.Count == 0)
        {
            return [];
        }

        var sectorNamesById = await LoadLookupNamesAsync(
            assets
                .Where(a => a.SectorId.HasValue)
                .Select(a => a.SectorId!.Value)
                .Distinct(),
            (id, ct) => _lookupService.GetSectorNameAsync(id, ct),
            cancellationToken);

        var subSectorNamesById = await LoadLookupNamesAsync(
            assets
                .Where(a => a.SubSectorId.HasValue)
                .Select(a => a.SubSectorId!.Value)
                .Distinct(),
            (id, ct) => _lookupService.GetSubSectorNameAsync(id, ct),
            cancellationToken);

        var assetTypeNamesById = await LoadLookupNamesAsync(
            assets
                .Where(a => a.AssetTypeId.HasValue)
                .Select(a => a.AssetTypeId!.Value)
                .Distinct(),
            (id, ct) => _lookupService.GetAssetTypeNameAsync(id, ct),
            cancellationToken);

        var companyNamesById = await LoadCompanyNamesAsync(
            assets.Select(a => a.CompanyId).Distinct(),
            cancellationToken);

        var submittedByNamesByAssetId = await _assetSubmittedByResolver.ResolveForAssetsAsync(
            assets,
            cancellationToken);

        var dtos = new List<AssetListDto>(assets.Count);

        foreach (var asset in assets)
        {
            var sectorName = ResolveLookupName(
                asset.SectorId,
                sectorNamesById,
                includeOtherValuesForSectorAndSubSector ? asset.SectorOther : null);

            var subSectorName = ResolveLookupName(
                asset.SubSectorId,
                subSectorNamesById,
                includeOtherValuesForSectorAndSubSector ? asset.SubSectorOther : null);

            var assetTypeName = ResolveLookupName(
                asset.AssetTypeId,
                assetTypeNamesById,
                asset.AssetTypeOther);

            var companyName = companyNamesById.TryGetValue(asset.CompanyId, out var resolvedCompanyName)
                ? resolvedCompanyName
                : asset.CompanyName;

            submittedByNamesByAssetId.TryGetValue(asset.Id, out var submittedByDisplayName);

            dtos.Add(new AssetListDto
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName.Value,
                SectorName = sectorName,
                SubSectorName = subSectorName,
                AssetTypeName = assetTypeName,
                Status = asset.Status,
                SubmittedAt = asset.SubmittedAt,
                SubmittedBy = submittedByDisplayName,
                TotalCapex = asset.TotalCapex,
                TotalOpex = asset.TotalOpex,
                CompanyName = companyName,
                CreatedAt = asset.CreatedAt
            });
        }

        return dtos;
    }

    private static string ResolveLookupName(
        Guid? lookupId,
        IReadOnlyDictionary<Guid, string> namesById,
        string? fallbackValue)
    {
        if (lookupId.HasValue && namesById.TryGetValue(lookupId.Value, out var resolved))
        {
            return resolved;
        }

        return fallbackValue ?? "N/A";
    }

    private async Task<IReadOnlyDictionary<Guid, string>> LoadCompanyNamesAsync(
        IEnumerable<Guid> companyIds,
        CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
        var dict = new ConcurrentDictionary<Guid, string>();

        var tasks = companyIds.Select(async companyId =>
        {
            if (companyId == Guid.Empty)
            {
                return;
            }

            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var company = await _middlewareService.GetCompanyByIdAsync(companyId);
                if (!string.IsNullOrWhiteSpace(company?.Name))
                {
                    dict.TryAdd(companyId, company.Name);
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return dict;
    }

    private static async Task<IReadOnlyDictionary<Guid, string>> LoadLookupNamesAsync(
        IEnumerable<Guid> ids,
        Func<Guid, CancellationToken, Task<string?>> getNameAsync,
        CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(MaxConcurrency, MaxConcurrency);
        var dict = new ConcurrentDictionary<Guid, string>();

        var tasks = ids.Select(async id =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var name = await getNameAsync(id, cancellationToken);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    dict.TryAdd(id, name);
                }
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return dict;
    }
}
