using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;

public interface IEvaluatorRepository
{
    Task<IEnumerable<Evaluator>> GetAll(CancellationToken cancellationToken = default);

    Task<IEnumerable<Evaluator>> GetByIds(List<Guid> ids, CancellationToken cancellationToken);
}
