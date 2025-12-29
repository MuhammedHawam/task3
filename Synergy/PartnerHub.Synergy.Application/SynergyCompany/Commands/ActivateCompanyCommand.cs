using MediatR;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SynergyCompany.Commands;

public class ActivateCompanyCommand : IRequest<Result>
{
    public Guid CompanyId { get; set; }
}
