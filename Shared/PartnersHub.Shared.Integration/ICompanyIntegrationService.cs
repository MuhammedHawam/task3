using PartnersHub.Shared.Integration.DTOs;

namespace PartnersHub.Shared.Integration;

public interface ICompanyIntegrationService
{
    Task<ExternalCompanyDto?> GetCompanyByIdAsync(Guid companyId);
    Task<CompanyIntegrationResponseDto?> GetCompaniesAsync(CompanyIntegrationRequestDto request);
}
