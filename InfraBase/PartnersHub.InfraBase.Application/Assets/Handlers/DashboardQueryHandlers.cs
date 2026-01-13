using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Application.Common.Models;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetContributorDashboardQueryHandler 
    : IRequestHandler<GetContributorDashboardQuery, ContributorDashboardDto>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;
    private readonly IMiddlewareIntegrationService _middlewareService;

    public GetContributorDashboardQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService,
        IMiddlewareIntegrationService middlewareService)
    {
        _repository = repository;
        _lookupService = lookupService;
        _middlewareService = middlewareService;
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

        var assetDtos = new List<AssetListDto>();
        
        var companyNamesById = await LoadCompanyNamesAsync(
            paginatedAssets.Items.Select(a => a.CompanyId).Distinct(),
            cancellationToken);

        foreach (var asset in paginatedAssets.Items)
        {
            var sectorName = asset.SectorId.HasValue && asset.SectorId.Value != Guid.Empty
                ? await _lookupService.GetSectorNameAsync(asset.SectorId.Value, cancellationToken)
                : null;
            sectorName = string.IsNullOrWhiteSpace(sectorName) ? "N/A" : sectorName;

            var subSectorName = asset.SubSectorId.HasValue && asset.SubSectorId.Value != Guid.Empty
                ? await _lookupService.GetSubSectorNameAsync(asset.SubSectorId.Value, cancellationToken)
                : null;
            subSectorName = string.IsNullOrWhiteSpace(subSectorName) ? "N/A" : subSectorName;

            var assetTypeName = asset.AssetTypeId.HasValue && asset.AssetTypeId.Value != Guid.Empty
                ? await _lookupService.GetAssetTypeNameAsync(asset.AssetTypeId.Value, cancellationToken)
                : null;
            assetTypeName = string.IsNullOrWhiteSpace(assetTypeName)
                ? (asset.AssetTypeOther ?? "N/A")
                : assetTypeName;

            var companyName = companyNamesById.TryGetValue(asset.CompanyId, out var resolvedCompanyName)
                ? resolvedCompanyName
                : asset.CompanyName;
            
            assetDtos.Add(new AssetListDto
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName.Value,
                SectorName = sectorName,
                SubSectorName = subSectorName,
                AssetTypeName = assetTypeName,
                Status = asset.Status,
                SubmittedAt = asset.SubmittedAt,
                TotalCapex = asset.TotalCapex,
                TotalOpex = asset.TotalOpex,
                CompanyName = companyName,
                CreatedAt = asset.CreatedAt
            });
        }

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

    private async Task<IReadOnlyDictionary<Guid, string>> LoadCompanyNamesAsync(
        IEnumerable<Guid> companyIds,
        CancellationToken cancellationToken)
    {
        const int maxConcurrency = 8;
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var dict = new System.Collections.Concurrent.ConcurrentDictionary<Guid, string>();

        var tasks = companyIds.Select(async companyId =>
        {
            if (companyId == Guid.Empty)
                return;

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

public class GetPcAdminDashboardQueryHandler 
    : IRequestHandler<GetPcAdminDashboardQuery, PcAdminDashboardDto>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;
    private readonly IMiddlewareIntegrationService _middlewareService;

    public GetPcAdminDashboardQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService,
        IMiddlewareIntegrationService middlewareService)
    {
        _repository = repository;
        _lookupService = lookupService;
        _middlewareService = middlewareService;
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

        var assetDtos = new List<AssetListDto>();
        
        var companyNamesById = await LoadCompanyNamesAsync(
            paginatedAssets.Items.Select(a => a.CompanyId).Distinct(),
            cancellationToken);

        foreach (var asset in paginatedAssets.Items)
        {
            var sectorName = asset.SectorId.HasValue && asset.SectorId.Value != Guid.Empty
                ? await _lookupService.GetSectorNameAsync(asset.SectorId.Value, cancellationToken)
                : null;
            sectorName = string.IsNullOrWhiteSpace(sectorName) ? "N/A" : sectorName;

            var subSectorName = asset.SubSectorId.HasValue && asset.SubSectorId.Value != Guid.Empty
                ? await _lookupService.GetSubSectorNameAsync(asset.SubSectorId.Value, cancellationToken)
                : null;
            subSectorName = string.IsNullOrWhiteSpace(subSectorName) ? "N/A" : subSectorName;

            var assetTypeName = asset.AssetTypeId.HasValue && asset.AssetTypeId.Value != Guid.Empty
                ? await _lookupService.GetAssetTypeNameAsync(asset.AssetTypeId.Value, cancellationToken)
                : null;
            assetTypeName = string.IsNullOrWhiteSpace(assetTypeName)
                ? (asset.AssetTypeOther ?? "N/A")
                : assetTypeName;

            var companyName = companyNamesById.TryGetValue(asset.CompanyId, out var resolvedCompanyName)
                ? resolvedCompanyName
                : asset.CompanyName;
            
            assetDtos.Add(new AssetListDto
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName.Value,
                SectorName = sectorName,
                SubSectorName = subSectorName,
                AssetTypeName = assetTypeName,
                Status = asset.Status,
                SubmittedAt = asset.SubmittedAt,
                TotalCapex = asset.TotalCapex,
                TotalOpex = asset.TotalOpex,
                CompanyName = companyName,
                CreatedAt = asset.CreatedAt
            });
        }

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

    private async Task<IReadOnlyDictionary<Guid, string>> LoadCompanyNamesAsync(
        IEnumerable<Guid> companyIds,
        CancellationToken cancellationToken)
    {
        const int maxConcurrency = 8;
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var dict = new System.Collections.Concurrent.ConcurrentDictionary<Guid, string>();

        var tasks = companyIds.Select(async companyId =>
        {
            if (companyId == Guid.Empty)
                return;

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

public class GetTeamAssetsDashboardQueryHandler 
    : IRequestHandler<GetTeamAssetsDashboardQuery, TeamAssetsDashboardDto>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;
    private readonly IMiddlewareIntegrationService _middlewareService;

    public GetTeamAssetsDashboardQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService,
        IMiddlewareIntegrationService middlewareService)
    {
        _repository = repository;
        _lookupService = lookupService;
        _middlewareService = middlewareService;
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

        var assetDtos = new List<AssetListDto>();
        
        var companyNamesById = await LoadCompanyNamesAsync(
            paginatedAssets.Items.Select(a => a.CompanyId).Distinct(),
            cancellationToken);

        foreach (var asset in paginatedAssets.Items)
        {
            var sectorName = asset.SectorId.HasValue && asset.SectorId.Value != Guid.Empty
                ? await _lookupService.GetSectorNameAsync(asset.SectorId.Value, cancellationToken)
                : null;
            sectorName = string.IsNullOrWhiteSpace(sectorName) ? "N/A" : sectorName;

            var subSectorName = asset.SubSectorId.HasValue && asset.SubSectorId.Value != Guid.Empty
                ? await _lookupService.GetSubSectorNameAsync(asset.SubSectorId.Value, cancellationToken)
                : null;
            subSectorName = string.IsNullOrWhiteSpace(subSectorName) ? "N/A" : subSectorName;

            var assetTypeName = asset.AssetTypeId.HasValue && asset.AssetTypeId.Value != Guid.Empty
                ? await _lookupService.GetAssetTypeNameAsync(asset.AssetTypeId.Value, cancellationToken)
                : null;
            assetTypeName = string.IsNullOrWhiteSpace(assetTypeName)
                ? (asset.AssetTypeOther ?? "N/A")
                : assetTypeName;

            var companyName = companyNamesById.TryGetValue(asset.CompanyId, out var resolvedCompanyName)
                ? resolvedCompanyName
                : asset.CompanyName;
            
            assetDtos.Add(new AssetListDto
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName.Value,
                SectorName = sectorName,
                SubSectorName = subSectorName,
                AssetTypeName = assetTypeName,
                Status = asset.Status,
                SubmittedAt = asset.SubmittedAt,
                TotalCapex = asset.TotalCapex,
                TotalOpex = asset.TotalOpex,
                CompanyName = companyName,
                CreatedAt = asset.CreatedAt
            });
        }

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

    private async Task<IReadOnlyDictionary<Guid, string>> LoadCompanyNamesAsync(
        IEnumerable<Guid> companyIds,
        CancellationToken cancellationToken)
    {
        const int maxConcurrency = 8;
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var dict = new System.Collections.Concurrent.ConcurrentDictionary<Guid, string>();

        var tasks = companyIds.Select(async companyId =>
        {
            if (companyId == Guid.Empty)
                return;

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

public class GetInfrabaseAdminDashboardQueryHandler 
    : IRequestHandler<GetInfrabaseAdminDashboardQuery, InfrabaseAdminDashboardDto>
{
    private readonly IAssetRepository _repository;
    private readonly IConfigurationLookupService _lookupService;
    private readonly IMiddlewareIntegrationService _middlewareService;

    public GetInfrabaseAdminDashboardQueryHandler(
        IAssetRepository repository,
        IConfigurationLookupService lookupService,
        IMiddlewareIntegrationService middlewareService)
    {
        _repository = repository;
        _lookupService = lookupService;
        _middlewareService = middlewareService;
    }

    public async Task<InfrabaseAdminDashboardDto> Handle(
        GetInfrabaseAdminDashboardQuery request, 
        CancellationToken cancellationToken)
    {
        var statusCounts = await _repository.GetStatusCountsAsync(null, cancellationToken);

        var paginatedAssets = await _repository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.StatusFilter,
            null,
            request.SearchTerm,
            null,
            false,
            cancellationToken);

        var assetDtos = new List<AssetListDto>();
        
        var companyNamesById = await LoadCompanyNamesAsync(
            paginatedAssets.Items.Select(a => a.CompanyId).Distinct(),
            cancellationToken);

        foreach (var asset in paginatedAssets.Items)
        {
            var sectorName = asset.SectorId.HasValue && asset.SectorId.Value != Guid.Empty
                ? await _lookupService.GetSectorNameAsync(asset.SectorId.Value, cancellationToken)
                : null;
            sectorName = string.IsNullOrWhiteSpace(sectorName) ? "N/A" : sectorName;

            var subSectorName = asset.SubSectorId.HasValue && asset.SubSectorId.Value != Guid.Empty
                ? await _lookupService.GetSubSectorNameAsync(asset.SubSectorId.Value, cancellationToken)
                : null;
            subSectorName = string.IsNullOrWhiteSpace(subSectorName) ? "N/A" : subSectorName;

            var assetTypeName = asset.AssetTypeId.HasValue && asset.AssetTypeId.Value != Guid.Empty
                ? await _lookupService.GetAssetTypeNameAsync(asset.AssetTypeId.Value, cancellationToken)
                : null;
            assetTypeName = string.IsNullOrWhiteSpace(assetTypeName)
                ? (asset.AssetTypeOther ?? "N/A")
                : assetTypeName;

            var companyName = companyNamesById.TryGetValue(asset.CompanyId, out var resolvedCompanyName)
                ? resolvedCompanyName
                : asset.CompanyName;
            
            assetDtos.Add(new AssetListDto
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName.Value,
                SectorName = sectorName,
                SubSectorName = subSectorName,
                AssetTypeName = assetTypeName,
                Status = asset.Status,
                SubmittedAt = asset.SubmittedAt,
                TotalCapex = asset.TotalCapex,
                TotalOpex = asset.TotalOpex,
                CompanyName = companyName,
                CreatedAt = asset.CreatedAt
            });
        }

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
                RejectedByInfrabase = statusCounts.GetValueOrDefault(AssetStatuses.RejectedByInfrabase, 0)
            },
            Assets = new PaginatedList<AssetListDto>(
                assetDtos, 
                paginatedAssets.TotalCount, 
                request.PageNumber, 
                request.PageSize)
        };
    }

    private async Task<IReadOnlyDictionary<Guid, string>> LoadCompanyNamesAsync(
        IEnumerable<Guid> companyIds,
        CancellationToken cancellationToken)
    {
        const int maxConcurrency = 8;
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var dict = new System.Collections.Concurrent.ConcurrentDictionary<Guid, string>();

        var tasks = companyIds.Select(async companyId =>
        {
            if (companyId == Guid.Empty)
                return;

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
