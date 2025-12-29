using Microsoft.EntityFrameworkCore;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;


namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;

public class CompanyRepository(InnovationHubDbContext dbContext) : ICompanyRepository
{

    public async Task<IEnumerable<Company>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken)
    {
        return await dbContext.Companies.Where(sc => ids.Contains(sc.Id)).ToListAsync(cancellationToken); 
    }

}
