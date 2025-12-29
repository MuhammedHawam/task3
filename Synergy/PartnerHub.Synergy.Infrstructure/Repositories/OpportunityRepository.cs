using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PartnersHub.Synergy.Application.Common;
using PartnersHub.Synergy.Application.Common.Options;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate;
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace PartnersHub.Synergy.Infrastructure.Repositories;

public class OpportunityRepository : IOpportunityRepository
{
    private readonly SynergyDbContext _context;
    private readonly RequestCodeSettings _requestCodeSettings;

    public OpportunityRepository(SynergyDbContext context, IOptions<RequestCodeSettings> requestCodeSettings)
    {
        _context = context;
        _requestCodeSettings = requestCodeSettings.Value;
    }

    public async Task AddAsync(Opportunity opportunity)
    {
        await _context.Opportunities.AddAsync(opportunity);
    }

    public void Delete(Opportunity opportunity)
    {
        _context.Opportunities.Remove(opportunity);
    }

    public async Task<List<Opportunity>> GetAllAsync(bool asNoTracking = false, params Expression<Func<Opportunity, object>>[] includes)
    {
        IQueryable<Opportunity> query = _context.Opportunities.AsNoTracking();

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public async Task<bool> IsOpportunityWithTitleAndCompanyExistsAsync(string title, Guid companyId)
    {
        return await _context.Opportunities
            .AnyAsync(o => o.Title.Value == title && o.CompanyId == companyId);
    }

    public async Task<Opportunity?> GetByIdAsync(Guid id, bool asNoTracking = false, params Expression<Func<Opportunity, object>>[] includes)
    {
        IQueryable<Opportunity> query = _context.Opportunities.Where(o => o.Id == id);

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync();
    }

    public void Update(Opportunity opportunity)
    {
        _context.Opportunities.Update(opportunity);
    }


    public async Task<List<Opportunity>> GetOpportunitiesByCollaboratedCompanyAsync(Guid companyId)
    {
        return await _context.Opportunities
            .Include(o => o.OpportunityType)
            .Include(o => o.ThematicArea)
            .Include(o => o.CollaboratedCompanies)
            .Where(o => o.CollaboratedCompanies.Any(c => c.SynergyCompanyId == companyId))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }


    public async Task<List<Opportunity>> GetByPublishingCompanyId(Guid companyId, bool asNoTracking = false, params Expression<Func<Opportunity, object>>[] includes)
    {
        IQueryable<Opportunity> query = _context.Opportunities.Where(o => o.CompanyId == companyId);

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public async Task<List<Opportunity>> GetByIds(List<Guid> opportunityIds, bool asNoTracking = false, params Expression<Func<Opportunity, object>>[] includes)
    {
        IQueryable<Opportunity> query = _context.Opportunities.Where(o => opportunityIds.Contains(o.Id));

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        return await query.ToListAsync();
    }
    public async Task<Dictionary<Opportunity, List<SynergyCompany>>> GetOpportunitiesWithCompaniesByStatus(OpportunityStatus status,
       bool asNoTracking = false, params Expression<Func<Opportunity, object>>[] includes)
    {
        var query = _context.Opportunities.Where(o => o.Status == status);

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            query = includes.Aggregate(query, (q, i) => q.Include(i));
        }

        // Execute the query to get the fully-loaded entities first 
        var opportunities = await query.ToListAsync();

        // Now pass the list of fully loaded entities to the dictionary builder
        var opportunityDictionary = await ApplyOpportunityCompaniesDictionaryQuery(opportunities);
        return opportunityDictionary;
    }
    private async Task<Dictionary<Opportunity, List<SynergyCompany>>> ApplyOpportunityCompaniesDictionaryQuery(List<Opportunity> opportunities)
    {
        // Extract the IDs of the pre-loaded opportunities
        var opportunityIds = opportunities.Select(o => o.Id).ToList();

        // We filter the OpportunitySynergyCompanies by the IDs of the opportunities we just loaded.
        var companyAssociations = await _context.OpportunitySynergyCompanies
            .Where(occ => opportunityIds.Contains(occ.OpportunityId))
            .Join(_context.SynergyCompanies,
                occ => occ.SynergyCompanyId, // Join on SynergyCompanyId (the ID of the collaborating company)
                c => c.Id,           // Join on CompanyId (the actual ID in SynergyCompany)
                (occ, c) => new { occ.OpportunityId, Company = c }) // Project to anonymous type
            .ToListAsync();

        // Group the company associations by OpportunityId
        var companyDictionary = companyAssociations
            .GroupBy(x => x.OpportunityId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Company).ToList());

        // Merge the pre-loaded opportunities with the company lists
        var resultDictionary = new Dictionary<Opportunity, List<SynergyCompany>>();
        foreach (var opportunity in opportunities)
        {
            resultDictionary.Add(opportunity, companyDictionary.GetValueOrDefault(opportunity.Id, new List<SynergyCompany>()));
        }

        return resultDictionary;
    }


    public async Task<Dictionary<Opportunity, List<SynergyCompany>>> GetOpportunitiesWithCompaniesByCompanyId(
        Guid companyId,
        bool asNoTracking = false,
        params Expression<Func<Opportunity, object>>[] includes)
    {
        var query = _context.Opportunities.Where(o => o.CompanyId == companyId && o.Status == OpportunityStatus.Published);

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null)
        {
            query = includes.Aggregate(query, (q, i) => q.Include(i));
        }

        // Execute the query to get the fully-loaded entities first 
        var opportunities = await query.ToListAsync();

        // Now pass the list of fully loaded entities to the dictionary builder
        var opportunityDictionary = await ApplyOpportunityCompaniesDictionaryQuery(opportunities);
        return opportunityDictionary;
    }


    //ToDo : Move these methods to Dashboard Repository
    #region Dashboard Statistics

    public async Task<int> GetTotalCountByStatusAsync(OpportunityStatus status, DateTime? fromDate = null)
    {
        var query = _context.Opportunities.Where(o => o.Status == status);

        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedAt.Year == fromDate.Value.Year);

        return await query.CountAsync();
    }

    public async Task<int> GetCountByCompanyAndStatusAsync(Guid companyId, OpportunityStatus status, DateTime? fromDate = null)
    {
        var query = _context.Opportunities
            .Where(o => o.CompanyId == companyId && o.Status == status);

        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedAt.Year == fromDate.Value.Year);

        return await query.CountAsync();
    }

    public async Task<int> GetDistinctCollaboratedCompaniesCountAsync(Guid companyId, DateTime? fromDate = null)
    {
        IQueryable<Opportunity> query = _context.Opportunities
            .Where(o => o.CompanyId == companyId)
            .Include(o => o.CollaboratedCompanies);

        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedAt.Year >= fromDate.Value.Year);

        var opportunities = await query.ToListAsync();
        var collaboratedCompanyIds = opportunities
            .SelectMany(o => o.CollaboratedCompanies)
            .Select(c => c.SynergyCompanyId)
            .Distinct()
            .Count();

        return collaboratedCompanyIds;
    }
    public async Task<(int publishedCount, int totalCount)> GetTotalCount(DateTime? fromDate = null)
    {
        IQueryable<Opportunity> query = _context.Opportunities;
        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedAt.Year == fromDate.Value.Year);

        // Use the filtered query variable, not the full table

        int totalCount = await query.CountAsync();
        int publishedCount = await query.Where(s => s.Status == OpportunityStatus.Published).CountAsync();
        return (publishedCount, totalCount);
    }


    #endregion

    #region Search Opportunities

    public async Task<PaginatedList<Opportunity>> SearchOpportunitiesAsync(
        int pageNumber,
        int pageSize,
        List<Guid>? companyIds = null,
        string? searchTerm = null,
        List<Guid>? sectorIds = null,
        List<int>? opportunityTypeIds = null,
        List<int>? thematicAreaIds = null,
        List<int>? collaborationRequirementIds = null,
        List<int>? expectedOutcomeIds = null,
        List<CollaborationStatusFilter> collaborationStatuses = null,
        List<OpportunityStatus>? statuses = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        string? sortBy = null,
        bool sortDescending = true,
        bool asNoTracking = true)
    {
        // Remove duplicate includes
        IQueryable<Opportunity> query = _context.Opportunities
            .Include(o => o.OpportunityType)
            .Include(o => o.ThematicArea)
            .Include(o => o.CollaborationRequirements)
            .Include(o => o.ExpectedOutcomes)
            .Include(o => o.CollaboratedCompanies)
            .Include(o => o.RepresentativeInformation)
            .OrderByDescending(o => o.CreatedAt);

        if (asNoTracking)
            query = query.AsNoTracking();

        // Filter by companies (for user submissions or filtering by creator companies)
        if (companyIds != null && companyIds.Any())
        {
            query = query.Where(o => companyIds.Contains(o.CompanyId));
        }

        // Filter by statuses (default to Published and AssetManagerApproved if not specified AND no companyIds)
        if (statuses != null && statuses.Any())
        {
            query = query.Where(o => statuses.Contains(o.Status));
        }
        // Filter by collaboration status (default to Published and AssetManagerApproved if not specified AND no companyIds)
        if (collaborationStatuses != null && collaborationStatuses.Any())
        {

                query = query.Where(o =>
                (collaborationStatuses.Contains(CollaborationStatusFilter.Active) ? (o.StartDate <= DateOnly.FromDateTime(DateTime.Now) && o.EndDate >= DateOnly.FromDateTime(DateTime.Now)) : false) ||
                (collaborationStatuses.Contains(CollaborationStatusFilter.Closed) ? (o.EndDate < DateOnly.FromDateTime(DateTime.Now)) : false) ||
                (collaborationStatuses.Contains(CollaborationStatusFilter.Upcoming) ? (o.StartDate > DateOnly.FromDateTime(DateTime.Now)) : false));

     
        }
        else if (companyIds == null || !companyIds.Any())
        {
            //TODO: review the uncommented code
            // Default: only show published opportunities for public search
            //query = query.Where(o => 
            //    o.Status == OpportunityStatus.Published || 
            //    o.Status == OpportunityStatus.AssetManagerApproved);
        }
        // If companyIds is provided and no statuses, show all statuses for those companies

        // Apply search term
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(o =>
                o.Title.Value.ToLower().Contains(term) ||
                (o.Description != null && o.Description.Value != null && o.Description.Value.ToLower().Contains(term)));
        }

        // Apply sector filter
        if (sectorIds != null && sectorIds.Any())
        {
            query = query.Where(o => sectorIds.Contains(o.Sector.Id));
        }

        // Apply opportunity type filter
        if (opportunityTypeIds != null && opportunityTypeIds.Any())
        {
            query = query.Where(o => opportunityTypeIds.Contains(o.OpportunityTypeId));
        }

        // Apply thematic area filter
        if (thematicAreaIds != null && thematicAreaIds.Any())
        {
            query = query.Where(o => thematicAreaIds.Contains(o.ThematicAreaId));
        }

        // Apply collaboration requirement filter
        if (collaborationRequirementIds != null && collaborationRequirementIds.Any())
        {
            query = query.Where(o => o.CollaborationRequirements.Any(cr =>
                collaborationRequirementIds.Contains(cr.CollaborationRequirementId)));
        }

        // Apply expected outcome filter
        if (expectedOutcomeIds != null && expectedOutcomeIds.Any())
        {
            query = query.Where(o => o.ExpectedOutcomes.Any(eo =>
                expectedOutcomeIds.Contains(eo.ExpectedOutcomeId)));
        }
        if (startDate != null)
        {
            query = query.Where(o => o.StartDate >= startDate);
        }
        if (endDate != null)
        {
            query = query.Where(o => o.EndDate <= endDate);
        }
        // Get total count AFTER filtering but BEFORE pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = (sortBy?.ToLower(), sortDescending) switch
        {
            ("title", true) => query.OrderByDescending(o => o.Title.Value),
            ("title", false) => query.OrderBy(o => o.Title.Value),
            ("startdate", true) => query.OrderByDescending(o => o.StartDate),
            ("startdate", false) => query.OrderBy(o => o.StartDate),
            ("enddate", true) => query.OrderByDescending(o => o.EndDate),
            ("enddate", false) => query.OrderBy(o => o.EndDate),
            ("status", true) => query.OrderByDescending(o => o.Status),
            ("status", false) => query.OrderBy(o => o.Status),
            ("submissiondate", true) or ("createdat", true) or _ when sortDescending => query.OrderByDescending(o => o.CreatedAt),
            ("submissiondate", false) or ("createdat", false) or _ => query.OrderBy(o => o.CreatedAt)
        };

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<Opportunity>(items, totalCount, pageNumber, pageSize);
    }

    #endregion
    public async Task<int> GetNextRequestIdAsync(CancellationToken cancellationToken = default)
    {
        var expectedPrefix = $"{_requestCodeSettings.Prefix}{_requestCodeSettings.Separator}";

        var requestIds = await _context.Opportunities
            .Where(a => a.RequestId != null && a.RequestId.StartsWith(expectedPrefix))
            .Select(a => a.RequestId)
            .ToListAsync(cancellationToken);

        if (!requestIds.Any())
        {
            return 1;
        }

        var numbers = requestIds
            .Select(code => _requestCodeSettings.ParseCode(code))
            .Where(num => num.HasValue)
            .Select(num => num!.Value)
            .ToList();

        return numbers.Any() ? numbers.Max() + 1 : 1;
    }
}
