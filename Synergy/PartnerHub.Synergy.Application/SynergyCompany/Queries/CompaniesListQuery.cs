using MediatR;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Shared.Integration.DTOs;
using PartnersHub.Synergy.Domain.Common;


namespace PartnersHub.Synergy.Application.SynergyCompany.Queries;

public record class CompaniesListQuery : IRequest<Result<PaginatedList<CompanyIntegrationDto>>>
{

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchText { get; set; }
    public List<Guid>? SectorIds { get; set; }
    public List<Guid>? CityIds { get; set; }
}
