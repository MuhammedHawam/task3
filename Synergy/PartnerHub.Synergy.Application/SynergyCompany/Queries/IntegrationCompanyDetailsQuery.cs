using MediatR;
using PartnersHub.Synergy.Application.SynergyCompany.DTOs;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.SynergyCompany.Queries;

public record IntegrationCompanyDetailsQuery(Guid CompanyId) : IRequest<Result<ExternalCompanyDto>?>;

