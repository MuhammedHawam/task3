using MediatR;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Application.Middleware.DTOs;

namespace PartnersHub.ConfigurationHub.Application.Middleware.Queries;

public record GetMiddlewareCompaniesQuery : IRequest<Result<PaginatedList<MiddlewareCompanyDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchText { get; init; }
    public List<Guid>? SectorIds { get; init; }
    public List<Guid>? CityIds { get; init; }
}

public record GetAllMiddlewareCompaniesQuery : IRequest<Result<List<MiddlewareCompanyDto>>>;

public record GetMiddlewareCompanyByIdQuery : IRequest<Result<MiddlewareCompanyDto>>
{
    public Guid CompanyId { get; init; }

    public GetMiddlewareCompanyByIdQuery(Guid companyId)
    {
        CompanyId = companyId;
    }
}

public record GetMiddlewareSectorsQuery : IRequest<Result<List<MiddlewareSectorDto>>>;
