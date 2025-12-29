using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;


namespace PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;

public interface ICampaignRequestRepository
{
    Task AddAsync(CampaignRequest campaign, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, Guid? Id, CancellationToken cancellationToken);
    Task<(IEnumerable<CampaignRequest> Items, int TotalCount)> GetActiveCampaignPaginatedAsync(
        List<int> typeList,
        List<int> statusList,
        List<int> requestStatusList,
        DateTime? lunchdate,
        string? Search,
        bool? IsMyCampaign,
        Guid? userId,
        bool? IsAdmin,
        bool? IsPending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<CampaignRequest> Items, int TotalCount)> GetByIdsAsync(
       List<Guid> Ids,
       int pageNumber,
       int pageSize,
       CancellationToken cancellationToken = default);

    Task<CampaignRequest?> GetById(Guid id, CancellationToken cancellationToken);
    Task Update(CampaignRequest campaign, CancellationToken cancellationToken);
}
