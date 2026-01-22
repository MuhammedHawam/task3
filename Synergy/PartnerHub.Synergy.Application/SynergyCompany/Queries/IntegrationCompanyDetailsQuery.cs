using MediatR;
using PartnersHub.Shared.Integration.DTOs;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SynergyCompany.Queries;

public record IntegrationCompanyDetailsQuery(Guid CompanyId) : IRequest<Result<ExternalCompanyDto>?>;

