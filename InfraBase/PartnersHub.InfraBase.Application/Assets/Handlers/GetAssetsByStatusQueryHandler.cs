using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using System.Collections.Concurrent;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetsByStatusQueryHandler : IRequestHandler<GetAssetsByStatusQuery, List<AssetListDto>>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly ITokenService _tokenService;

    public GetAssetsByStatusQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService,
        IMiddlewareIntegrationService middlewareService,
        ITokenService tokenService)
    {
        _repository = repository;
        _lookupService = lookupService;
        _middlewareService = middlewareService;
        _tokenService = tokenService;
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

        var items = new List<AssetListDto>();

        var sectorNamesById = await LoadLookupNamesAsync(
            paginatedAssets.Items
                .Where(a => a.SectorId.HasValue)
                .Select(a => a.SectorId!.Value)
                .Distinct(),
            (id, ct) => _lookupService.GetSectorNameAsync(id, ct),
            cancellationToken);

        var subSectorNamesById = await LoadLookupNamesAsync(
            paginatedAssets.Items
                .Where(a => a.SubSectorId.HasValue)
                .Select(a => a.SubSectorId!.Value)
                .Distinct(),
            (id, ct) => _lookupService.GetSubSectorNameAsync(id, ct),
            cancellationToken);

        var assetTypeNamesById = await LoadLookupNamesAsync(
            paginatedAssets.Items
                .Where(a => a.AssetTypeId.HasValue)
                .Select(a => a.AssetTypeId!.Value)
                .Distinct(),
            (id, ct) => _lookupService.GetAssetTypeNameAsync(id, ct),
            cancellationToken);

        var companyNamesById = await LoadCompanyNamesAsync(
            paginatedAssets.Items.Select(a => a.CompanyId).Distinct(),
            cancellationToken);

        foreach (var asset in paginatedAssets.Items)
        {
            var sectorName = asset.SectorId.HasValue &&
                            sectorNamesById.TryGetValue(asset.SectorId.Value, out var foundSector)
                            ? foundSector
                            : "N/A";
            var subSectorName = asset.SubSectorId.HasValue &&
                                subSectorNamesById.TryGetValue(asset.SubSectorId.Value, out var foundSubSector)
                                ? foundSubSector
                                : "N/A";
            // Business rule: Use AssetTypeOther when AssetTypeId is null
            var assetTypeName = asset.AssetTypeId.HasValue &&
                                assetTypeNamesById.TryGetValue(asset.AssetTypeId.Value, out var foundAssetType)
                                ? foundAssetType
                                : asset.AssetTypeOther ?? "N/A";

            var companyName = companyNamesById.TryGetValue(asset.CompanyId, out var resolvedCompanyName)
                ? resolvedCompanyName
                : asset.CompanyName;

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
                CompanyName = companyName,
                CreatedAt = asset.CreatedAt
            });
        }

        return items;
    }

    private static async Task<IReadOnlyDictionary<Guid, string>> LoadLookupNamesAsync(
        IEnumerable<Guid> ids,
        Func<Guid, CancellationToken, Task<string?>> getNameAsync,
        CancellationToken cancellationToken)
    {
        const int maxConcurrency = 8;
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

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

    private async Task<IReadOnlyDictionary<Guid, string>> LoadCompanyNamesAsync(
        IEnumerable<Guid> companyIds,
        CancellationToken cancellationToken)
    {
        const int maxConcurrency = 8;
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

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
}