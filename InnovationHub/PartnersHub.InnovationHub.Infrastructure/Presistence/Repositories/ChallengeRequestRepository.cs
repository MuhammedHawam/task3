using Microsoft.EntityFrameworkCore;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Enums;




namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;

public class ChallengeRequestRepository(InnovationHubDbContext dbContext) : IChallengeRequestRepository
{

    public async Task AddAsync(ChallengeRequest challenge, CancellationToken cancellationToken)
    {
        await dbContext.challengeRequests.AddAsync(challenge, cancellationToken);
    }

    public async Task<ChallengeRequest?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.challengeRequests.Include(c=>c.SourceCompany).Include(c => c.AssociatedSector).Include(s => s.Technologies).ThenInclude(d => d.LinkedTechnology).Include(w => w.Attachments.Where(a => !a.IsDeleted)).Include(e => e.RevisionComments).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ChallengeRequest>> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        return dbContext.challengeRequests.Where(c => c.UserId == userId);
    }

    public async Task Update(ChallengeRequest challenge, CancellationToken cancellationToken)
    {
        dbContext.challengeRequests.Update(challenge);
    }

    public async Task Delete(ChallengeRequest challenge, CancellationToken cancellationToken)
    {
            dbContext.challengeRequests.Remove(challenge);
    }



    public async Task<IEnumerable<ChallengeRequest>> GetAll(CancellationToken cancellationToken)
    {
        return await dbContext.challengeRequests.Include(c => c.SourceCompany).Include(c => c.AssociatedSector).AsQueryable().OrderByDescending(r => r.CreatedAt).ToListAsync(cancellationToken);
    }




    public async Task<(IEnumerable<ChallengeRequest>, int TotalCount)> ListAsync(string? Search,
                                                            List<Guid>? DevCoId,
                                                            List<Guid>? SectorId,
                                                            List<string>? PriorityLevel,
                                                            bool? IsMyChallenge,
                                                            Guid? UserId,
                                                            bool? IsAdmin,
                                                            bool? IsCounts,
                                                            List<string>? StatusList,
                                                            bool? IsPending,
                                                            int PageSize,
                                                            int PageNumber,
                                                            CancellationToken cancellationToken)
    {

        var challenges = dbContext.challengeRequests
            .Include(c => c.SourceCompany)
            .Include(c => c.AssociatedSector).AsQueryable();

        if (!string.IsNullOrEmpty(Search))
        {
            challenges = challenges
                .Where(c => c.Name.Contains(Search) ||
                           c.SubmitterName.Contains(Search) ||
                          (c.SourceCompany != null && c.SourceCompany.Name.Contains(Search)) ||
                          (c.AssociatedSector != null && c.AssociatedSector.Name.Contains(Search)));
        }

        if (DevCoId != null && DevCoId.Count > 0)
        {
            challenges = challenges
                .Where(c => DevCoId.Contains(c.SourceCompanyId));
        }

        if (SectorId != null && SectorId.Count > 0)
        {
            challenges = challenges
                .Where(c => SectorId.Contains(c.AssociatedSectorId));
        }

        if (PriorityLevel != null && PriorityLevel.Count > 0)
        {
            var ids = PriorityLevel
                            .Select(pl => (PriorityLevel)Enum.Parse(typeof(PriorityLevel), pl))
                            .Select(pl => (int)pl)
                            .ToArray();

            challenges = challenges
                .Where(c => ids.Contains(c.PriorityLevelId));
        }
        if (StatusList != null && StatusList.Count > 0)
        {

            var ids = StatusList.Where(e => !e.Contains(ChallengeStatus.Archived.ToString()))
                            .Select(pl => (ChallengeStatus)Enum.Parse(typeof(ChallengeStatus), pl))
                            .Select(pl => (int)pl)
                            .ToArray();
            if (ids != null && ids.Length > 0 && StatusList.Contains(ChallengeStatus.Archived.ToString()))
            {
                challenges = challenges.Where(c => c.IsArchived == true || ids.Contains((int)c.ChallengeStatus));
            }
            else if (ids != null && ids.Length > 0 && !StatusList.Contains(ChallengeStatus.Archived.ToString()))
            {
                challenges = challenges.Where(c =>  ids.Contains((int)c.ChallengeStatus));
            }
            else
            {
                challenges = challenges.Where(e => e.IsArchived == true);
            }
           
        }
        if (IsMyChallenge == true)
        {
            challenges = challenges.Where(c => UserId == null || c.UserId == UserId);

        }
        else if (IsAdmin == true)
        {
            challenges = challenges.Where(c => c.ChallengeStatus == ChallengeStatus.Approved ); 
        }
        else if(IsPending == true)
        {
            challenges = challenges.Where(c => (c.ChallengeStatus == ChallengeStatus.Pending || c.ChallengeStatus == ChallengeStatus.RevisionsRequest));
        }
        else
        {
            challenges = challenges.Where(c => c.ChallengeStatus == ChallengeStatus.Approved && c.IsArchived != true);

        }

        var orderedChallenges = challenges.OrderByDescending(c => c.CreatedAt);
        var totalCount = await orderedChallenges.CountAsync(cancellationToken);
       


        var items = await orderedChallenges
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);

    }
    public async Task<bool> ExistsByNameAsync(string name, Guid? Id, CancellationToken cancellationToken)
    {
        return await dbContext.challengeRequests.AnyAsync(c => c.Id != Id && EF.Functions.Like(c.Name, $"{name}", "\\"), cancellationToken);
    }

    public async Task<(IEnumerable<ChallengeRequest> Items, int TotalCount,List<Guid> CampaignIds)> GetByCompanyId(List<Guid> companyIds,
                                                                    int pageNumber,
                                                                    int pageSize, 
                                                                    CancellationToken cancellationToken)
    {

        var query = dbContext.challengeRequests.Include(c => c.CampaignRequests)
                                               .Include(c => c.SourceCompany)
                                               .Include(c => c.AssociatedSector)
                                               .Where(c => companyIds.Contains(c.SourceCompanyId) && c.ChallengeStatus == ChallengeStatus.Approved).AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        var campaignIds = query.SelectMany(cr => cr.CampaignRequests.Select(crLinked => crLinked.CampaignRequestId)).Distinct().ToList();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount, campaignIds);
    }

    public async Task<List<ChallengeRequest>> GetByIDs(List<Guid> IDs)
    {
        return dbContext.challengeRequests.Include(c => c.SourceCompany)
                                          .Include(c => c.AssociatedSector).Where(s => IDs.Contains(s.Id)).ToList();

    }
}
