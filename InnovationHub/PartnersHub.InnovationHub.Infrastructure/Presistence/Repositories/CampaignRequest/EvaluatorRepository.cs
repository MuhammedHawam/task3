using Microsoft.EntityFrameworkCore;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;

public class EvaluatorRepository(InnovationHubDbContext dbContext) : IEvaluatorRepository
{
    public async Task<IEnumerable<Evaluator>> GetAll(CancellationToken cancellationToken = default)
    {
        return await dbContext.Evaluators.AsQueryable().OrderByDescending(r => r.CreatedAt)
                                       .ToListAsync(cancellationToken);
    }


    public async Task<IEnumerable<Evaluator>> GetByIds(List<Guid> ids, CancellationToken cancellationToken)
    {
        return await dbContext.Evaluators.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
    }
}
