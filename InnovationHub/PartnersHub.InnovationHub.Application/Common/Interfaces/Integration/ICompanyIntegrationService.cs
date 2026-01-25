using PartnersHub.InnovationHub.Application.Company.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Common.Interfaces.Integration;

public interface ICompanyIntegrationService
{

    /// <summary>
    /// Fetches company details from external middleware service
    /// </summary>
    /// <param name="companyId">The company ID in the external system</param>
    /// <returns>Company data from external system</returns>
    Task<ExternalCompanyDto?> GetCompanyByIdAsync(Guid companyId);
}
