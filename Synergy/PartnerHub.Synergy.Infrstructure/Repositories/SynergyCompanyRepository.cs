using Microsoft.EntityFrameworkCore;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate;
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Infrastructure.Persistence;
using System.Globalization;
using System.Linq.Expressions;

public class SynergyCompanyRepository : ISynergyCompanyRepository
{
    private readonly SynergyDbContext _context;

    public SynergyCompanyRepository(SynergyDbContext context)
    {
        _context = context;
    }

    public async Task<SynergyCompany?> GetByIdAsync(Guid id, bool asNoTracking = false, params Expression<Func<SynergyCompany, object>>[] includes)
    {
        IQueryable<SynergyCompany> query = _context.SynergyCompanies.Where(c => c.Id == id);

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SynergyCompany>> GetAllAsync(bool asNoTracking = false, params Expression<Func<SynergyCompany, object>>[] includes)
    {
        var query = _context.SynergyCompanies.AsQueryable();
        if (includes != null)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }
        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync();
    }

    public async Task<List<SynergyCompany>> GetByIdsAsync(List<Guid> ids, bool asNoTracking = false)
    {
        var query = _context.SynergyCompanies.Where(sc => ids.Contains(sc.Id));

        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync();
    }
    
    public async Task AddAsync(SynergyCompany company)
    {
        await _context.SynergyCompanies.AddAsync(company);
    }
    
    public void Update(SynergyCompany company)
    {
        _context.SynergyCompanies.Update(company);
    }
    public async Task<PaginatedList<SynergyCompany>> Search(int pageSize,
        int pageNumber,
        string? searchTerm = null,
        List<Guid>? sectors = null,
        List<string>? cities = null,
        List<string>? countries = null,
        string? sortBy = null,
        bool sortDescending = true)
    {
        IQueryable<SynergyCompany> query = _context.SynergyCompanies;
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(sc => sc.Name.Value.ToLower().Contains(searchTerm.ToLower().Trim()) 
                || sc.Description.Value.ToLower().Contains(searchTerm.ToLower().Trim()));


        query = query.Include(sc => sc.Sectors);

        if (sectors != null && sectors.Count > 0)
            query = query.Where(sc => sc.Sectors.Any(s => sectors.Contains(s.SectorId)));

        if (cities != null && cities.Count > default(int))
            query = query.Where(sc => cities.Select(c => c.ToLower().Trim()).Contains(sc.HeadquarterCity.ToLower().Trim()));

        if (countries != null && countries.Count > default(int))
            query = query.Where(sc => countries.Select(c => c.ToLower().Trim()).Contains(sc.HeadquarterCountry.ToLower().Trim()));

        var totalCount = await query.CountAsync();
        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        query = (sortBy?.ToLower(), sortDescending) switch
        {
            ("sector", true) => query.OrderByDescending(sc => sc.Sectors.FirstOrDefault().SectorName),
            ("sector", false) => query.OrderBy(sc => sc.Sectors.FirstOrDefault().SectorName),
            ("country", true) => query.OrderByDescending(sc => sc.HeadquarterCountry),
            ("country", false) => query.OrderBy(sc => sc.HeadquarterCountry),
            ("city", true) => query.OrderByDescending(sc => sc.HeadquarterCity),
            ("city", false) => query.OrderBy(sc=> sc.HeadquarterCity),

            ("createdat", true) or _ when sortDescending => query.OrderByDescending(sc => sc.CreatedAt),
            ("createdat", false) or _ => query.OrderBy(sc => sc.CreatedAt)
        };
        return new PaginatedList<SynergyCompany>(items, totalCount, pageNumber, pageSize);
    }

    #region Dashboard Methods

    public async Task<int> GetTotalActiveCompaniesCountAsync()
    {
        IQueryable<SynergyCompany> query = _context.SynergyCompanies;


        return await query.Where(c => c.IsActive).CountAsync();
    }
    
    public async Task<int> GetTotalCompaniesCountAsync()
    {
        return await _context.SynergyCompanies.Where(e => e.IsActive).CountAsync();
    }

    public async Task<List<SynergyCompany>> GetRecentAsync(int count, bool asNoTracking = true, params Expression<Func<SynergyCompany, object>>[] includes)
    {
        var query = _context.SynergyCompanies
            .OrderByDescending(c => c.CreatedAt)
            .Take(count);
        if (includes != null)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }
        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync();
    }

    #endregion
}