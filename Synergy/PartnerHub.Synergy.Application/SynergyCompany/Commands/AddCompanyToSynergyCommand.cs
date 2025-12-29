using MediatR;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SynergyCompany.Commands;

/// <summary>
/// Command to add a company to Synergy from external middleware service
/// </summary>
public class AddCompanyToSynergyCommand : IRequest<Result<Guid>>
{
    /// <summary>
    /// The company ID from the middleware/external system
    /// </summary>
    public Guid CompanyId { get; set; }
}
