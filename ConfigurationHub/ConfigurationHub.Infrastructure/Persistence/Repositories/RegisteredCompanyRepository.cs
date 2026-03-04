using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Repositories
{
    public class RegisteredCompanyRepository(ConfigurationHubDbContext _context,ICurrentUserService _currentUser) : IRegisteredCompanyRepository
    {
        public async Task<PaginatedList<RegisteredCompanyListDto>> GetAllAsync(int pageSize, int pageIndex, string? searchparam, string? sortBy = null)
        {
            var query = _context.RegisteredCompanies
                .AsNoTracking()
                .AsQueryable();


            query = sortBy?.ToLower() switch
            {
                "name:asc" => query.OrderBy(x => x.Name),
                "name:desc" => query.OrderByDescending(x => x.Name),

                "sectorname:asc" => query.OrderBy(x => x.SectorName),
                "sectorname:desc" => query.OrderByDescending(x => x.SectorName),

                "productname:asc" => query.OrderBy(x => x.Module.Name),
                "productname:desc" => query.OrderByDescending(x => x.Module.Name),

                "createdat:asc" => query.OrderBy(x => x.CreatedAt),
                "createdat:desc" => query.OrderByDescending(x => x.CreatedAt),

                _ => query.OrderByDescending(x => x.CreatedAt)
            };
            if (!string.IsNullOrEmpty(searchparam))
            {
                var searchPattern = $"%{searchparam.Trim()}%";
                query = query.Where(r =>
                    EF.Functions.Like(r.Name, searchPattern) ||
                    EF.Functions.Like(r.Module.Name, searchPattern));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RegisteredCompanyListDto
                {
                   Id = r.Id,
                   Name = r.Name,
                   Description = r.Description,
                   SectorName = r.SectorName,
                   ModuleName = r.Module.Name,
                   OnboardedBy= r.CreatedBy,
                   OnboardingDate=r.CreatedAt
                })
                .ToListAsync(); 

            return new PaginatedList<RegisteredCompanyListDto>(items, totalCount, pageIndex, pageSize);
        }


        public async Task<bool> AddAsync(Guid ModuleId,string? sectorId,string? sectorName,string description,List<RegisteredCompanyDto> companyDtos,CancellationToken cancellationToken)
        {
            var normalizedRequestedNames = companyDtos
                .Select(company => company.Name?.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .ToList();

            var distinctRequestedNames = normalizedRequestedNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (distinctRequestedNames.Count != normalizedRequestedNames.Count)
            {
                throw new ArgumentException("Company Already Registered");
            }

            var normalizedNameSet = distinctRequestedNames
                .Select(name => name.ToLower())
                .ToList();

            var companyAlreadyExists = await _context.RegisteredCompanies
                .AsNoTracking()
                .AnyAsync(company => normalizedNameSet.Contains(company.Name.ToLower()), cancellationToken);

            if (companyAlreadyExists)
            {
                throw new ArgumentException("Company Already Registered");
            }

            var companies = companyDtos.Select(companyDto => new RegisteredCompany
                {
                    ModuleId = ModuleId,
                    Name = companyDto.Name,
                    CompanyId = companyDto.CompanyId,
                    SectorId = sectorId,
                    SectorName = sectorName,
                    Description = description,
                    CreatedBy = _currentUser.UserId ?? "N/A",
                    CreatedAt = DateTime.Now,
                })
                .ToList();

            await _context.RegisteredCompanies.AddRangeAsync(companies, cancellationToken);

            return await _context.SaveChangesAsync(cancellationToken) >0 ;
        }


        public async Task<bool> DeleteAsync(Guid companyId,CancellationToken cancellationToken)
        {
            var company = await _context.RegisteredCompanies.FirstOrDefaultAsync(a=>a.Id == companyId);
            if (company == null)
            {
                return false;
            }

            _context.RegisteredCompanies.Remove(company);

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
