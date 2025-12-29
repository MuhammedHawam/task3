using PartnersHub.Synergy.Application.SynergyCompany.DTOs;
using PartnersHub.Synergy.Application.SynergyCompany.Queries;

namespace PartnersHub.Synergy.Application.Interfaces.Integration;

/// <summary>
/// Service for integrating with external middleware/company service
/// </summary>
public interface ICompanyIntegrationService
{
    /// <summary>
    /// Fetches company details from external middleware service
    /// </summary>
    /// <param name="companyId">The company ID in the external system</param>
    /// <returns>Company data from external system</returns>
    Task<ExternalCompanyDto?> GetCompanyByIdAsync(Guid companyId);

    Task<CompanyIntegrationResponseDto?> GetCompaniesList(CompaniesListQuery request);
}
