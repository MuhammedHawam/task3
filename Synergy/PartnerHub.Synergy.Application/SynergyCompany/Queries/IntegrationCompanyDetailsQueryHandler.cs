using MediatR;
using PartnersHub.Shared.Integration;
using PartnersHub.Shared.Integration.DTOs;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SynergyCompany.Queries;


public class IntegrationCompanyDetailsQueryHandler : IRequestHandler<IntegrationCompanyDetailsQuery, Result<ExternalCompanyDto>?>
{
    private readonly ICompanyIntegrationService _companyIntegrationService;


    public IntegrationCompanyDetailsQueryHandler(
        ICompanyIntegrationService companyIntegrationService)
    {
        _companyIntegrationService = companyIntegrationService;

    }

    public async Task<Result<ExternalCompanyDto>?> Handle(IntegrationCompanyDetailsQuery request, CancellationToken cancellationToken)
    {
      var companyObj = await _companyIntegrationService.GetCompanyByIdAsync(request.CompanyId); 
        return Result<ExternalCompanyDto>.Success(companyObj);
    }

  
}
