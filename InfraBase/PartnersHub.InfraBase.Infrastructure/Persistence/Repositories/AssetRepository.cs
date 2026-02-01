using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Application.Common.Models;
using PartnersHub.InfraBase.Application.Common.Options;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Infrastructure.Persistence.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly InfrabaseDbContext _context;
    private readonly AssetCodeSettings _assetCodeSettings;

    public AssetRepository(InfrabaseDbContext context, IOptions<AssetCodeSettings> assetCodeSettings)
    {
        _context = context;
        _assetCodeSettings = assetCodeSettings.Value;
    }

    public async Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Asset?> GetByIdWithDetailsAsync(Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .Include(a => a.CapexDetails)
            .Include(a => a.OpexDetails)
            .Include(a => a.History)
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Asset?> GetByIdWithFinancialsAsync(Guid id,
       CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .Include(a => a.CapexDetails)
            .Include(a => a.OpexDetails)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Asset?> GetByIdWithAttachmentsAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
    public async Task<PaginatedList<Asset>> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        AssetStatuses? status = null, 
        Guid? companyId = null, 
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = false,
        IReadOnlyCollection<Guid>? assetTypeIds = null,
        string? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildAssetQuery(status, companyId, searchTerm, assetTypeIds, requestingUser);

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Asset>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PaginatedList<Asset>> GetPaginatedByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        AssetStatuses? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Assets
            .Where(a => a.CreatedBy == userId.ToString())
            .Include(a => a.CapexDetails)
            .Include(a => a.OpexDetails)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchPattern = $"%{searchTerm}%";
            query = query.Where(a => 
                EF.Functions.Like(a.AssetName.Value, searchPattern) ||
                (a.AssetCode != null && EF.Functions.Like(a.AssetCode, searchPattern)) ||
                (a.AssetTypeOther != null && EF.Functions.Like(a.AssetTypeOther, searchPattern)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Asset>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PaginatedList<Asset>> GetTeamAssetsPaginatedAsync(
        Guid companyId,
        Guid excludeUserId,
        int pageNumber,
        int pageSize,
        AssetStatuses? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Assets
            .Where(a => a.CompanyId == companyId && a.CreatedBy != excludeUserId.ToString())
            .Where(a => a.Status != AssetStatuses.Draft)
            .Include(a => a.CapexDetails)
            .Include(a => a.OpexDetails)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchPattern = $"%{searchTerm}%";
            query = query.Where(a => 
                EF.Functions.Like(a.AssetName.Value, searchPattern) ||
                (a.AssetCode != null && EF.Functions.Like(a.AssetCode, searchPattern)) ||
                (a.AssetTypeOther != null && EF.Functions.Like(a.AssetTypeOther, searchPattern)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<Asset>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Dictionary<AssetStatuses, int>> GetStatusCountsAsync(
        Guid? companyId = null, 
        string? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Assets.AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(a => a.CompanyId == companyId.Value);
        }

        query = ApplyDraftVisibilityFilter(query, requestingUser);

        return await query
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    public async Task<Dictionary<AssetStatuses, int>> GetStatusCountsByUserAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .Where(a => a.CreatedBy == userId.ToString())
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    public async Task<Dictionary<AssetStatuses, int>> GetTeamAssetsStatusCountsAsync(
        Guid companyId,
        Guid excludeUserId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .Where(a => a.CompanyId == companyId && a.CreatedBy != excludeUserId.ToString())
            .Where(a => a.Status != AssetStatuses.Draft)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    public async Task<int> GetNextAssetNumberAsync(CancellationToken cancellationToken = default)
    {
        var expectedPrefix = $"{_assetCodeSettings.Prefix}{_assetCodeSettings.Separator}";
        
        var assetCodes = await _context.Assets
            .Where(a => a.AssetCode != null && a.AssetCode.StartsWith(expectedPrefix))
            .Select(a => a.AssetCode)
            .ToListAsync(cancellationToken);

        if (!assetCodes.Any())
        {
            return 1;
        }

        var numbers = assetCodes
            .Select(code => _assetCodeSettings.ParseCode(code))
            .Where(num => num.HasValue)
            .Select(num => num!.Value)
            .ToList();

        return numbers.Any() ? numbers.Max() + 1 : 1;
    }

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        await _context.Assets.AddAsync(asset, cancellationToken);
    }

    public void Delete(Asset asset)
    {
        _context.Assets.Remove(asset);
    }

    private IQueryable<Asset> BuildAssetQuery(
        AssetStatuses? status = null,
        Guid? companyId = null,
        string? searchTerm = null,
        IReadOnlyCollection<Guid>? assetTypeIds = null,
        string? requestingUser = null)
    {
        var query = _context.Assets
            .Include(a => a.CapexDetails)
            .Include(a => a.OpexDetails)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (companyId.HasValue)
        {
            query = query.Where(a => a.CompanyId == companyId.Value);
        }

        query = ApplyDraftVisibilityFilter(query, requestingUser);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchPattern = $"%{searchTerm}%";
            var hasAssetTypeMatches = assetTypeIds != null && assetTypeIds.Count > 0;
            query = query.Where(a => 
                EF.Functions.Like(a.AssetName.Value, searchPattern) ||       // Search by Asset Name
                (a.AssetCode != null && EF.Functions.Like(a.AssetCode, searchPattern)) ||  // Search by Asset Code
                (a.AssetTypeOther != null && EF.Functions.Like(a.AssetTypeOther, searchPattern)) || // Search by Asset Type Other
                (hasAssetTypeMatches && a.AssetTypeId.HasValue && assetTypeIds!.Contains(a.AssetTypeId.Value)) || // Search by Asset Type name
                (a.CompanyName != null && EF.Functions.Like(a.CompanyName, searchPattern)));  // Search by Company Name
        }

        return query;
    }

    private static IQueryable<Asset> ApplyDraftVisibilityFilter(
        IQueryable<Asset> query,
        string? requestingUser)
    {
        if (string.IsNullOrWhiteSpace(requestingUser))
        {
            return query;
        }

        return query.Where(a => a.Status != AssetStatuses.Draft || a.CreatedBy == requestingUser);
    }

    private IQueryable<Asset> ApplySorting(IQueryable<Asset> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query.OrderByDescending(a => a.CreatedAt);
        }

        query = sortBy.ToLower() switch
        {
            "assetcode" => sortDescending 
                ? query.OrderByDescending(a => a.AssetCode) 
                : query.OrderBy(a => a.AssetCode),
            "assetname" => sortDescending 
                ? query.OrderByDescending(a => a.AssetName.Value) 
                : query.OrderBy(a => a.AssetName.Value),
            "sectorname" or "sectorid" => sortDescending 
                ? query.OrderByDescending(a => a.SectorId.HasValue ? a.SectorId.Value : Guid.Empty) 
                : query.OrderBy(a => a.SectorId.HasValue ? a.SectorId.Value : Guid.Empty),
            "subsectorname" or "subsectorid" => sortDescending 
                ? query.OrderByDescending(a => a.SubSectorId.HasValue ? a.SubSectorId.Value : Guid.Empty) 
                : query.OrderBy(a => a.SubSectorId.HasValue ? a.SubSectorId.Value : Guid.Empty),
            "assettypename" or "assettypeid" => sortDescending 
                ? query.OrderByDescending(a => a.AssetTypeId) 
                : query.OrderBy(a => a.AssetTypeId),
            "status" => sortDescending 
                ? query.OrderByDescending(a => a.Status) 
                : query.OrderBy(a => a.Status),
            "submittedat" => sortDescending 
                ? query.OrderByDescending(a => a.SubmittedAt) 
                : query.OrderBy(a => a.SubmittedAt),
            "submittedby" => sortDescending 
                ? query.OrderByDescending(a => a.SubmittedBy) 
                : query.OrderBy(a => a.SubmittedBy),
            "createdat" => sortDescending 
                ? query.OrderByDescending(a => a.CreatedAt) 
                : query.OrderBy(a => a.CreatedAt),
            "companyname" => sortDescending 
                ? query.OrderByDescending(a => a.CompanyName) 
                : query.OrderBy(a => a.CompanyName),
            "totalcapex" => sortDescending 
                ? query.OrderByDescending(a => a.TotalCapex) 
                : query.OrderBy(a => a.TotalCapex),
            "totalopex" => sortDescending 
                ? query.OrderByDescending(a => a.TotalOpex) 
                : query.OrderBy(a => a.TotalOpex),
            _ => query.OrderByDescending(a => a.CreatedAt)
        };

        return query;
    }
}
