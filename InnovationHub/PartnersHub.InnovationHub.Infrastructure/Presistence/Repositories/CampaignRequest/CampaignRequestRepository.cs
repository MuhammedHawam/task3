using Microsoft.EntityFrameworkCore;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Enums;



namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;

public class CampaignRequestRepository(InnovationHubDbContext dbContext) : ICampaignRequestRepository
{

    public async Task AddAsync(CampaignRequest campaign, CancellationToken cancellationToken)
    {
        await dbContext.campaignRequests.AddAsync(campaign, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? Id, CancellationToken cancellationToken)
    {
        return await dbContext.campaignRequests.AnyAsync(c => c.Id != Id && EF.Functions.Like(c.Name, $"{name}", "\\"), cancellationToken);
    }


    public async Task<(IEnumerable<CampaignRequest> Items, int TotalCount)> GetActiveCampaignPaginatedAsync(
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
        CancellationToken cancellationToken = default)
    {

        var query = dbContext.campaignRequests.AsQueryable();

        if (!string.IsNullOrEmpty(Search))
        {
            query = query
                         .Where(c => c.Name.Contains(Search) ||
                                     c.OwnerName.Contains(Search) ||
                                     c.Description.Contains(Search));
        }

        if(typeList != null && typeList.Count > 0)
            query = query.Where(c => typeList.Contains((int)c.Type));

        if (requestStatusList != null && requestStatusList.Count > 0)
            query = query.Where(c => requestStatusList.Contains((int)c.CampaignRequestStatus));


        if (lunchdate != null)
            query = query.Where(c => c.LaunchDate.Value.Date == lunchdate.Value.Date);

        if (statusList != null && statusList.Any())
        {
            query = query.Where(c =>
                  (statusList.Contains((int)CampaignStatus.Open) ? c.LaunchDate <= DateTime.Now && c.SubmissionDeadLine >= DateTime.Now : false) ||
                  (statusList.Contains((int)CampaignStatus.Upcoming) ? (c.LaunchDate > DateTime.Now || c.LaunchDate == null) : false) ||
                  (statusList.Contains((int)CampaignStatus.Closed) ? c.SubmissionDeadLine < DateTime.Now : false));
        }

        if (IsMyCampaign == true)
        {
            query = query.Where(c => c.OwnerId == userId).OrderByDescending(r => r.CreatedAt);
        }
        else if (IsAdmin == true)
        {
            query = query.Where(c => c.CampaignRequestStatus == CampaignRequestStatus.Published ).OrderByDescending(r => r.CreatedAt);
        }
        else if (IsPending == true)
        {
            query = query.Where(c => c.CampaignRequestStatus == CampaignRequestStatus.PendingReview || c.CampaignRequestStatus == CampaignRequestStatus.Requested).OrderByDescending(r => r.CreatedAt);

        }
        else
        {
            query = query.Where(c => c.CampaignRequestStatus == CampaignRequestStatus.Published).OrderByDescending(r => r.CreatedAt);

        }



        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<CampaignRequest> Items, int TotalCount)> GetByIdsAsync(
       List<Guid> Ids,
       int pageNumber,
       int pageSize,
       CancellationToken cancellationToken = default)
    {

        var query = dbContext.campaignRequests.Include(c => c.LinkedChallenges).Where(e => Ids.Contains(e.Id) && e.CampaignRequestStatus == CampaignRequestStatus.Published).AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<CampaignRequest?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.campaignRequests.Include(c => c.EvaluationCriterias).Include(c => c.Sponsors).Include(c => c.LinkedChallenges).Include(s => s.TermsAndCondition.Where(f => !f.IsDeleted)).Include(e => e.Evaluators).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task Update(CampaignRequest campaign, CancellationToken cancellationToken)
    {
        dbContext.campaignRequests.Update(campaign);
    }

}