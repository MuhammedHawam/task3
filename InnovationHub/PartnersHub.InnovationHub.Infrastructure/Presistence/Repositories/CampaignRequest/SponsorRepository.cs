using Microsoft.EntityFrameworkCore;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;


namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;

public class SponsorRepository(InnovationHubDbContext dbContext) : ISponsorRepository
{


    public async Task<IEnumerable<Sponsor>> GetAll( CancellationToken cancellationToken = default)
    {
        return await dbContext.Sponsors.AsQueryable().OrderByDescending(r => r.CreatedAt)
                                       .ToListAsync(cancellationToken); 
    }



}
