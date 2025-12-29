using Microsoft.EntityFrameworkCore;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;
public class ReviewChallengeRepository(InnovationHubDbContext dbContext) : IReviewChallengeRepository
{
    public async Task AddAsync(ChallengeRequestRevisionComment comment, CancellationToken cancellationToken)
    {
        await dbContext.challengeRequestRevisionComments.AddAsync(comment, cancellationToken);
    }

    public async Task<ChallengeRequestRevisionComment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.challengeRequestRevisionComments.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
    public void Update(ChallengeRequestRevisionComment comment, CancellationToken cancellationToken)
    {
        dbContext.challengeRequestRevisionComments.Update(comment);
    }
}
