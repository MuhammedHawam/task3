using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Application.Middleware.DTOs;

namespace PartnersHub.ConfigurationHub.Application.Middleware.Interfaces;

public interface IMiddlewareCompanyService
{
    Task<PaginatedList<MiddlewareCompanyDto>> GetCompaniesAsync(MiddlewareCompanyRequestDto request);
    Task<MiddlewareCompanyDto?> GetCompanyByIdAsync(Guid companyId);
    Task<List<MiddlewareSectorDto>> GetSectorsAsync();
}
