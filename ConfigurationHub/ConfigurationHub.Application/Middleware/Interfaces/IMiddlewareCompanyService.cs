using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Application.Middleware.DTOs;

namespace PartnersHub.ConfigurationHub.Application.Middleware.Interfaces;

public interface IMiddlewareCompanyService
{
    Task<PaginatedList<MiddlewareCompanyDto>> GetCompaniesAsync(MiddlewareCompanyRequestDto request);
    Task<List<MiddlewareCompanyDto>> GetAllCompaniesAsync();
    Task<MiddlewareCompanyDto?> GetCompanyByIdAsync(Guid companyId);
    Task<MiddlewareCompanyDto?> GetCompanyBySectorIdAsync(Guid sectorId);
    Task<List<MiddlewareSectorDto>> GetSectorsAsync();
}
