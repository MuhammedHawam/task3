using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces
{
    public interface IRegisteredCompanyRepository
    {
        Task<PaginatedList<RegisteredCompanyListDto>> GetAllAsync(int pageSize, int pageIndex, string? searchparam);
        Task<bool> AddAsync(Guid ModuleId, string sectorId, string sectorName, string description, List<RegisteredCompanyDto> companyDtos, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid companyId, CancellationToken cancellationToken = default);
    }
}
