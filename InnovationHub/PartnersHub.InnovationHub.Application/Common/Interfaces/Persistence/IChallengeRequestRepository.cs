using PartnersHub.InnovationHub.Application.Challenge.Queries.ChallengeRequest;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Paging;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence
{
    public interface IChallengeRequestRepository
    {
        Task AddAsync(ChallengeRequest challenge, CancellationToken cancellationToken);
        Task<ChallengeRequest?> GetById(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<ChallengeRequest>> GetByUserId(Guid userId, CancellationToken cancellationToken);
        Task Update(ChallengeRequest challenge, CancellationToken cancellationToken);
        Task Delete(ChallengeRequest challenge, CancellationToken cancellationToken);

        Task<(IEnumerable<ChallengeRequest>, int TotalCount)> ListAsync(string? Search,
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
                                                        CancellationToken cancellationToken);
        Task<IEnumerable<ChallengeRequest>> GetAll(CancellationToken cancellationToken);
        Task<bool> ExistsByNameAsync(string name, Guid? Id, CancellationToken cancellationToken);
        Task<(IEnumerable<ChallengeRequest> Items, int TotalCount, List<Guid> CampaignIds)> GetByCompanyId(List<Guid> companyIds,
                                                                    int pageNumber,
                                                                    int pageSize,
                                                                    CancellationToken cancellationToken);


        Task<List<ChallengeRequest>> GetByIDs(List<Guid> IDs);
    }
}
