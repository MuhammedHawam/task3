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
                .Include(r=>r.Module)
                .OrderBy(r => r.CreatedAt)
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
                query = query.Where(r =>
                    r.Name.Contains(searchparam) ||
                    r.Module.Name.Contains(searchparam));
            }

            var totalCount =  query.Count();

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


        public async Task<bool> AddAsync(Guid ModuleId,string sectorId,string sectorName,string description,List<RegisteredCompanyDto> companyDtos,CancellationToken cancellationToken)
        {
            if(_context.RegisteredCompanies.Any(a=> companyDtos.Select(c=>c.Name).Any(co=>co.ToLower() == a.Name.ToLower())))
            {
                throw new ArgumentException("Company Already Registered");
            }
            foreach (var companyDto in companyDtos)
            {
               await _context.RegisteredCompanies.AddAsync(new RegisteredCompany
                {
                    ModuleId = ModuleId,
                    Name = companyDto.Name,
                    CompanyId = companyDto.CompanyId,
                    SectorId = sectorId,
                    SectorName = sectorName,
                    Description = description,
                    CreatedBy = _currentUser.UserId ?? "N/A",
                    CreatedAt = DateTime.Now,
                }, cancellationToken);
            }

            return await _context.SaveChangesAsync(cancellationToken) >0 ;
        }


        public async Task<bool> DeleteAsync(Guid companyId,CancellationToken cancellationToken)
        {
            var company = await _context.RegisteredCompanies.FirstOrDefaultAsync(a=>a.Id == companyId);

            _context.RegisteredCompanies.Remove(company);

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
