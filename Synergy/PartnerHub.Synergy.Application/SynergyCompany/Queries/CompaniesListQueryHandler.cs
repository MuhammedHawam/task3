using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Integration;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.SynergyCompany.DTOs;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.SynergyCompany.Queries;


public class CompaniesListQueryHandler : IRequestHandler<CompaniesListQuery, Result<PaginatedList<CompanyIntegrationDto>>>
{
    private readonly ICompanyIntegrationService _companyService;


    public CompaniesListQueryHandler(
        ICompanyIntegrationService companyService)
    {
        _companyService = companyService;

    }

    public async Task<Result<PaginatedList<CompanyIntegrationDto>>> Handle(CompaniesListQuery request, CancellationToken cancellationToken)
    {




        var allCompaniesForFilters = await _companyService.GetCompaniesList(request);


        return Result<PaginatedList<CompanyIntegrationDto>>.Success(new PaginatedList<CompanyIntegrationDto>(allCompaniesForFilters.Companies, allCompaniesForFilters.TotalCount, request.PageNumber, request.PageSize));
    }



}
