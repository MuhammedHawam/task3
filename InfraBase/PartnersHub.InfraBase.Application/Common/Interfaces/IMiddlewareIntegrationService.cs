using PartnersHub.InfraBase.Application.Common.DTOs;

namespace PartnersHub.InfraBase.Application.Common.Interfaces;

public interface IMiddlewareIntegrationService
{
    Task<MiddlewareCompany?> GetCompanyByIdAsync(Guid companyId);
}
