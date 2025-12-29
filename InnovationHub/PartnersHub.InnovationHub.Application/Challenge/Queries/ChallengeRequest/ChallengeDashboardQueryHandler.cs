using MediatR;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Application.Challenge.Queries;

public class ChallengeDashboardQueryHandler(IChallengeRequestRepository _challengeRequestReadRepo) : IRequestHandler<ChallengeDashboardQuery, ChallengeDashboardDto>
{


    public async Task<ChallengeDashboardDto> Handle(ChallengeDashboardQuery query, CancellationToken cancellationToken)
    {
       
            var challenges = await _challengeRequestReadRepo.GetAll(cancellationToken);
            var allStatus = Enum.GetValues(typeof(ChallengeStatus)).Cast<ChallengeStatus>();
            var challengeStatusCounts = challenges.GroupBy(c => c.ChallengeStatus)
                                                        .Select(g => new ChallengeStatusCount
                                                        {
                                                            ChallengeStatus = g.Key,
                                                            Count = g.Count()
                                                        }).ToList();

            var statusCounts = allStatus.Select(s =>
            {
                var count = challengeStatusCounts.FirstOrDefault(csc => csc.ChallengeStatus == s)?.Count ?? 0;
                return new ChallengeStatusCount { ChallengeStatus = s, Count = count };
            })
            .ToList();


            var allPriorityLevels = Enum.GetValues(typeof(PriorityLevel)).Cast<PriorityLevel>();
            var finalPriorityLevelCounts = allPriorityLevels.Select(pl =>
            {
                var count = challenges
                    .Where(c => c.PriorityLevelId == (int)pl)
                    .Count();
                return new PriorityLevelCount
                {
                    PriorityName = pl.ToString(),
                    Count = count
                };
            })
                .ToList();

            var sectorCounts = challenges.GroupBy(c => c.AssociatedSectorId)
                                          .Select(g => new SectorCount
                                          {
                                              AssociatedSectorId = g.Key,
                                              AssociatedSectorName = g.First().AssociatedSector.Name,
                                              Count = g.Count()
                                          }).ToList();
        return new ChallengeDashboardDto
        {
            PriorityCountList = finalPriorityLevelCounts,
            SectorCountList = sectorCounts ,
            StatusCountList = statusCounts ,
            TotalCount = challenges.Count() 
        };     

    }



}
