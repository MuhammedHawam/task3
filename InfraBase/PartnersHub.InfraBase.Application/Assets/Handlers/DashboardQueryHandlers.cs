using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Application.Common.Models;
using PartnersHub.InfraBase.Domain.Enums;
using System;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetContributorDashboardQueryHandler
    : IRequestHandler<GetContributorDashboardQuery, ContributorDashboardDto>
{
    private readonly IAssetRepository _repository;
    private readonly IAssetListProjectionService _assetListProjectionService;

    public GetContributorDashboardQueryHandler(
        IAssetRepository repository,
        IAssetListProjectionService assetListProjectionService)
    {
        _repository = repository;
        _assetListProjectionService = assetListProjectionService;
    }

    public async Task<ContributorDashboardDto> Handle(
        GetContributorDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var statusCounts = await _repository.GetStatusCountsByUserAsync(
            request.UserId,
            cancellationToken);

        var paginatedAssets = await _repository.GetPaginatedByUserAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            request.StatusFilter,
            request.SearchTerm,
            cancellationToken);

        var assetDtos = await _assetListProjectionService.MapAsync(
            paginatedAssets.Items,
            includeOtherValuesForSectorAndSubSector: false,
            cancellationToken);

        return new ContributorDashboardDto
        {
            StatusCards = new ContributorStatusCardsDto
            {
                TotalAssets = statusCounts.Values.Sum(),
                CheckedAssets = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByInfrabase, 0),
                PendingOnPcAdmin = statusCounts.GetValueOrDefault(AssetStatuses.Submitted, 0),
                PendingOnInfrabaseAdmin = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByPcAdmin, 0),
                Draft = statusCounts.GetValueOrDefault(AssetStatuses.Draft, 0),
                ReturnForCorrection =
                    statusCounts.GetValueOrDefault(AssetStatuses.RejectedByPcAdmin, 0) +
                    statusCounts.GetValueOrDefault(AssetStatuses.RejectedByInfrabase, 0)
            },
            Assets = new PaginatedList<AssetListDto>(
                assetDtos,
                paginatedAssets.TotalCount,
                request.PageNumber,
                request.PageSize)
        };
    }
}

public class GetPcAdminDashboardQueryHandler
    : IRequestHandler<GetPcAdminDashboardQuery, PcAdminDashboardDto>
{
    private readonly IAssetRepository _repository;
    private readonly IAssetListProjectionService _assetListProjectionService;

    public GetPcAdminDashboardQueryHandler(
        IAssetRepository repository,
        IAssetListProjectionService assetListProjectionService)
    {
        _repository = repository;
        _assetListProjectionService = assetListProjectionService;
    }

    public async Task<PcAdminDashboardDto> Handle(
        GetPcAdminDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var statusCounts = await _repository.GetStatusCountsByUserAsync(
            request.UserId,
            cancellationToken);

        var paginatedAssets = await _repository.GetPaginatedByUserAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            request.StatusFilter,
            request.SearchTerm,
            cancellationToken);

        var assetDtos = await _assetListProjectionService.MapAsync(
            paginatedAssets.Items,
            includeOtherValuesForSectorAndSubSector: false,
            cancellationToken);

        return new PcAdminDashboardDto
        {
            MyAssetsStatusCards = new PcAdminStatusCardsDto
            {
                TotalAssets = statusCounts.Values.Sum(),
                Draft = statusCounts.GetValueOrDefault(AssetStatuses.Draft, 0),
                CheckedAssets = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByInfrabase, 0),
                PendingOnInfrabaseAdmin = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByPcAdmin, 0),
                ReturnForCorrection =
                    statusCounts.GetValueOrDefault(AssetStatuses.RejectedByPcAdmin, 0) +
                    statusCounts.GetValueOrDefault(AssetStatuses.RejectedByInfrabase, 0)
            },
            MyAssets = new PaginatedList<AssetListDto>(
                assetDtos,
                paginatedAssets.TotalCount,
                request.PageNumber,
                request.PageSize)
        };
    }
}

public class GetTeamAssetsDashboardQueryHandler
    : IRequestHandler<GetTeamAssetsDashboardQuery, TeamAssetsDashboardDto>
{
    private readonly IAssetRepository _repository;
    private readonly IAssetListProjectionService _assetListProjectionService;

    public GetTeamAssetsDashboardQueryHandler(
        IAssetRepository repository,
        IAssetListProjectionService assetListProjectionService)
    {
        _repository = repository;
        _assetListProjectionService = assetListProjectionService;
    }

    public async Task<TeamAssetsDashboardDto> Handle(
        GetTeamAssetsDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var statusCounts = await _repository.GetTeamAssetsStatusCountsAsync(
            request.CompanyId,
            request.UserId,
            cancellationToken);

        var paginatedAssets = await _repository.GetTeamAssetsPaginatedAsync(
            request.CompanyId,
            request.UserId,
            request.PageNumber,
            request.PageSize,
            request.StatusFilter,
            request.SearchTerm,
            cancellationToken);

        var assetDtos = await _assetListProjectionService.MapAsync(
            paginatedAssets.Items,
            includeOtherValuesForSectorAndSubSector: false,
            cancellationToken);

        return new TeamAssetsDashboardDto
        {
            StatusCards = new TeamAssetsStatusCardsDto
            {
                TotalAssets = statusCounts.Values.Sum(),
                CheckedAssets = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByInfrabase, 0),
                ReturnForCorrection =
                    statusCounts.GetValueOrDefault(AssetStatuses.RejectedByPcAdmin, 0) +
                    statusCounts.GetValueOrDefault(AssetStatuses.RejectedByInfrabase, 0),
                PendingAssets = statusCounts.GetValueOrDefault(AssetStatuses.Submitted, 0)
            },
            Assets = new PaginatedList<AssetListDto>(
                assetDtos,
                paginatedAssets.TotalCount,
                request.PageNumber,
                request.PageSize)
        };
    }
}

public class GetInfrabaseAdminDashboardQueryHandler
    : IRequestHandler<GetInfrabaseAdminDashboardQuery, InfrabaseAdminDashboardDto>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;
    private readonly ITokenService _tokenService;
    private readonly IAssetListProjectionService _assetListProjectionService;

    public GetInfrabaseAdminDashboardQueryHandler(
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

    public async Task<InfrabaseAdminDashboardDto> Handle(
        GetInfrabaseAdminDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var requestingUser = _tokenService.GetUserName();
        var assetTypeIdsForSearch = await GetAssetTypeIdsForSearchAsync(request.SearchTerm, cancellationToken);
        var statusCounts = await _repository.GetStatusCountsAsync(null, requestingUser, cancellationToken);

        var paginatedAssets = await _repository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.StatusFilter,
            request.tokenCompanyId,
            request.SearchTerm,
            null,
            false,
            assetTypeIdsForSearch,
            requestingUser,
            cancellationToken);

        var assetDtos = await _assetListProjectionService.MapAsync(
            paginatedAssets.Items,
            includeOtherValuesForSectorAndSubSector: false,
            cancellationToken);

        return new InfrabaseAdminDashboardDto
        {
            StatusCards = new InfrabaseAdminStatusCardsDto
            {
                TotalAssets = statusCounts.Values.Sum(),
                Draft = statusCounts.GetValueOrDefault(AssetStatuses.Draft, 0),
                Submitted = statusCounts.GetValueOrDefault(AssetStatuses.Submitted, 0),
                AcceptedByPcAdmin = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByPcAdmin, 0),
                RejectedByPcAdmin = statusCounts.GetValueOrDefault(AssetStatuses.RejectedByPcAdmin, 0),
                AcceptedByInfrabase = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByInfrabase, 0),
                RejectedByInfrabase = statusCounts.GetValueOrDefault(AssetStatuses.RejectedByInfrabase, 0),
                ReturnForCorrection = statusCounts.GetValueOrDefault(AssetStatuses.RejectedByPcAdmin, 0) +
                                      statusCounts.GetValueOrDefault(AssetStatuses.RejectedByInfrabase, 0)
            },
            Assets = new PaginatedList<AssetListDto>(
                assetDtos,
                paginatedAssets.TotalCount,
                request.PageNumber,
                request.PageSize)
        };
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