using MediatR;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.SynergyCompany.DTOs;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SynergyCompany.Queries;

public record GetRegisteredCompaniesQuery : IRequest<Result<PaginatedList<RegisteredCompanyCardDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public string? SearchTerm { get; init; }
    public List<Guid>? SectorIds { get; init; }
    public List<string>? Countries { get; init; }
    public List<string>? Cities { get; init; }
    
    /// <summary>
    /// Include inactive companies in search results (Admin only)
    /// Default: false (only active companies)
    /// </summary>
    public bool IncludeInactive { get; init; } = false;
}

//public class RegisteredCompaniesResultDto
//{
//    public List<RegisteredCompanyCardDto> Companies { get; set; } = new();
//    public int TotalCount { get; set; }
//    public int PageNumber { get; set; }
//    public int PageSize { get; set; }
//    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
//    public List<string> AvailableCountries { get; set; } = new();
//    public List<string> AvailableCities { get; set; } = new();
//}

public record GetCompanyDetailsQuery(Guid CompanyId) : IRequest<Result<CompanyDetailsDto>?>;
