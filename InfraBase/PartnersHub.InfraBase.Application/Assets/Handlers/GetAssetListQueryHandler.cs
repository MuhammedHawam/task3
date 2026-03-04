using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Application.Common.Models;
using System;


namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetListQueryHandler : IRequestHandler<GetAssetListQuery, PaginatedList<AssetListDto>>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;
    private readonly ITokenService _tokenService;
    private readonly IAssetListProjectionService _assetListProjectionService;

    public GetAssetListQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService,
        ITokenService tokenService,
        IAssetListProjectionService assetListProjectionService)
    {
        _repository = repository;
        _lookupService = lookupService;
        _tokenService = tokenService;
        _assetListProjectionService = assetListProjectionService;
    }

    public async Task<PaginatedList<AssetListDto>> Handle(GetAssetListQuery query,
        CancellationToken cancellationToken)
    {
        var requestingUser = _tokenService.GetUserName();
        var (sortBy, sortDescending) = ParseSort(query.SortBy, query.SortDescending);
        var requiresInMemorySort = RequiresInMemorySort(sortBy);
        var assetTypeIdsForSearch = await GetAssetTypeIdsForSearchAsync(query.SearchTerm, cancellationToken);
        var paginatedAssets = await _repository.GetPagedAsync(
            requiresInMemorySort ? 1 : query.PageNumber,
            requiresInMemorySort ? int.MaxValue : query.PageSize,
            query.Status,
            query.CompanyId,
            query.SearchTerm,
            requiresInMemorySort ? null : sortBy,
            requiresInMemorySort ? false : sortDescending,
            assetTypeIdsForSearch,
            requestingUser,
            cancellationToken);

        var items = await _assetListProjectionService.MapAsync(
            paginatedAssets.Items,
            includeOtherValuesForSectorAndSubSector: true,
            cancellationToken);

        if (requiresInMemorySort && !string.IsNullOrWhiteSpace(sortBy))
        {
            var sortedItems = SortItems(items, sortBy, sortDescending);
            var skip = Math.Max(0, (query.PageNumber - 1) * query.PageSize);
            var pagedItems = sortedItems
                .Skip(skip)
                .Take(query.PageSize)
                .ToList();

            return new PaginatedList<AssetListDto>(
                pagedItems,
                paginatedAssets.TotalCount,
                query.PageNumber,
                query.PageSize);
        }

        return new PaginatedList<AssetListDto>(
            items,
            paginatedAssets.TotalCount,
            query.PageNumber,
            query.PageSize);
    }

    private static (string? SortBy, bool SortDescending) ParseSort(string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return (null, sortDescending);
        }

        var trimmed = sortBy.Trim();
        var parts = trimmed.Split(':', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            var direction = parts[1];
            sortDescending = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
            trimmed = parts[0];
        }

        return string.IsNullOrWhiteSpace(trimmed) ? (null, sortDescending) : (trimmed, sortDescending);
    }

    private static bool RequiresInMemorySort(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return false;
        }

        return sortBy.Equals("sectorName", StringComparison.OrdinalIgnoreCase) ||
               sortBy.Equals("subSectorName", StringComparison.OrdinalIgnoreCase) ||
               sortBy.Equals("assetTypeName", StringComparison.OrdinalIgnoreCase);
    }

    private static List<AssetListDto> SortItems(IEnumerable<AssetListDto> items, string sortBy, bool sortDescending)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var normalized = sortBy.Trim();

        return normalized.Equals("sectorName", StringComparison.OrdinalIgnoreCase)
            ? (sortDescending
                ? items.OrderByDescending(x => x.SectorName ?? string.Empty, comparer)
                       .ThenBy(x => x.AssetName ?? string.Empty, comparer)
                       .ToList()
                : items.OrderBy(x => x.SectorName ?? string.Empty, comparer)
                       .ThenBy(x => x.AssetName ?? string.Empty, comparer)
                       .ToList())
            : normalized.Equals("subSectorName", StringComparison.OrdinalIgnoreCase)
                ? (sortDescending
                    ? items.OrderByDescending(x => x.SubSectorName ?? string.Empty, comparer)
                           .ThenBy(x => x.AssetName ?? string.Empty, comparer)
                           .ToList()
                    : items.OrderBy(x => x.SubSectorName ?? string.Empty, comparer)
                           .ThenBy(x => x.AssetName ?? string.Empty, comparer)
                           .ToList())
                : normalized.Equals("assetTypeName", StringComparison.OrdinalIgnoreCase)
                    ? (sortDescending
                        ? items.OrderByDescending(x => x.AssetTypeName ?? string.Empty, comparer)
                               .ThenBy(x => x.AssetName ?? string.Empty, comparer)
                               .ToList()
                        : items.OrderBy(x => x.AssetTypeName ?? string.Empty, comparer)
                               .ThenBy(x => x.AssetName ?? string.Empty, comparer)
                               .ToList())
                    : items.ToList();
    }

    private async Task<IReadOnlyCollection<Guid>?> GetAssetTypeIdsForSearchAsync(
       string? searchTerm,
       CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return null;
        }

        var searchValues = await _lookupService.GetAssetTypeSearchValuesAsync(cancellationToken);
        if (searchValues.Count == 0)
        {
            return null;
        }

        var matchedCodes = searchValues
            .Where(kvp => kvp.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Select(kvp => kvp.Key)
            .ToList();
        if (matchedCodes.Count == 0)
        {
            return null;
        }

        var codeToId = await _lookupService.GetAssetTypeIdsByCodeAsync(cancellationToken);
        if (codeToId.Count == 0)
        {
            return null;
        }

        var ids = matchedCodes
            .Where(codeToId.ContainsKey)
            .Select(code => codeToId[code])
            .ToList();

        return ids.Count == 0 ? null : ids;
    }
}