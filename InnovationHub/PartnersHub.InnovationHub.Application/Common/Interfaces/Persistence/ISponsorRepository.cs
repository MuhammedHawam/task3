using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;

public interface ISponsorRepository
{
    Task<IEnumerable<Sponsor>> GetAll(CancellationToken cancellationToken);
}
