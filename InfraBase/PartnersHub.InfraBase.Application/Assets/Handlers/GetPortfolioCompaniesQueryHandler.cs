using MediatR;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.DTOs;
using PartnersHub.InfraBase.Application.Common.Interfaces;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetPortfolioCompaniesQueryHandler : IRequestHandler<GetPortfolioCompaniesQuery, List<PortfolioCompanyDto>>
{
    private readonly IMiddlewareIntegrationService _middlewareService;

    public GetPortfolioCompaniesQueryHandler(IMiddlewareIntegrationService middlewareService)
    {
        _middlewareService = middlewareService;
    }

    public async Task<List<PortfolioCompanyDto>> Handle(GetPortfolioCompaniesQuery query, 
        CancellationToken cancellationToken)
    {
        return await _middlewareService.SearchPortfolioCompaniesAsync(query.SearchTerm, cancellationToken);
    }
}
