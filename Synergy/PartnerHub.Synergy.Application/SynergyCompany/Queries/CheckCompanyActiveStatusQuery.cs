using MediatR;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.SynergyCompany.Queries;

public record CheckCompanyActiveStatusQuery(Guid CompanyId) : IRequest<Result<bool>>;
