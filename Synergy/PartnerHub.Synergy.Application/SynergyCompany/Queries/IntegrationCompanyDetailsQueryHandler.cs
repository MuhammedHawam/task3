using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Integration;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.SynergyCompany.DTOs;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
