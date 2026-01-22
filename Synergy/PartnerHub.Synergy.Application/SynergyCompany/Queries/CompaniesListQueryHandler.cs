using MediatR;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Shared.Integration;
using PartnersHub.Shared.Integration.DTOs;
using PartnersHub.Synergy.Domain.Common;

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
        var requestDto = new CompanyIntegrationRequestDto
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchText = request.SearchText,
            SectorIds = request.SectorIds,
            CityIds = request.CityIds
        };

        var allCompaniesForFilters = await _companyService.GetCompaniesAsync(requestDto);
        if (allCompaniesForFilters == null)
        {
            return Result<PaginatedList<CompanyIntegrationDto>>.Failure("Companies list returned no data from middleware");
        }

        return Result<PaginatedList<CompanyIntegrationDto>>.Success(
            new PaginatedList<CompanyIntegrationDto>(
                allCompaniesForFilters.Companies,
                allCompaniesForFilters.TotalCount,
                request.PageNumber,
                request.PageSize));
    }



}
