using PartnersHub.InfraBase.Application.Common.Models;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Asset?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedList<Asset>> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        AssetStatuses? status = null, 
        Guid? companyId = null, 
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);
    Task<PaginatedList<Asset>> GetPaginatedByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        AssetStatuses? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<PaginatedList<Asset>> GetTeamAssetsPaginatedAsync(
        Guid companyId,
        Guid excludeUserId,
        int pageNumber,
        int pageSize,
        AssetStatuses? status = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
    Task<Dictionary<AssetStatuses, int>> GetStatusCountsAsync(
        Guid? companyId = null,
        CancellationToken cancellationToken = default);
    Task<Dictionary<AssetStatuses, int>> GetStatusCountsByUserAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
    Task<Dictionary<AssetStatuses, int>> GetTeamAssetsStatusCountsAsync(
        Guid companyId,
        Guid excludeUserId,
        CancellationToken cancellationToken = default);
    Task<int> GetNextAssetNumberAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Asset asset, CancellationToken cancellationToken = default);
    void Delete(Asset asset);
}
