using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using PartnersHub.Synergy.Application.Common.Options;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.SuccessStories.DTOs;
using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Infrastructure.Persistence;
using System.ComponentModel.Design;
using System.Data;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace PartnersHub.Synergy.Infrastructure.Repositories;

public class SuccessStoryRepository : ISuccessStoryRepository
{
    private readonly SynergyDbContext _context;
    private readonly RequestCodeSettings _requestCodeSettings;

    public SuccessStoryRepository(SynergyDbContext context, IOptions<RequestCodeSettings> requestCodeSettings)
    {
        _context = context;
        _requestCodeSettings = requestCodeSettings.Value;
    }

    public async Task<SuccessStory> GetByIdAsync(Guid id, bool asNoTracking = false, params Expression<Func<SuccessStory, object>>[] includes)
    {
        IQueryable<SuccessStory> query = _context.SuccessStories;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(ss => ss.Id == id);
    }

    public async Task AddAsync(SuccessStory successStory)
    {
        await _context.SuccessStories.AddAsync(successStory);
    }

    public void Update(SuccessStory successStory)
    {
        _context.SuccessStories.Update(successStory);
    }

    public void Delete(SuccessStory successStory)
    {
        _context.SuccessStories.Remove(successStory);
    }

    public async Task<IEnumerable<SuccessStory>> GetAllAsync(bool asNoTracking = false, params Expression<Func<SuccessStory, object>>[] includes)
    {
        IQueryable<SuccessStory> query = _context.SuccessStories;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        return await query.ToListAsync();
    }

    public async Task<PaginatedList<SuccessStory>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        Guid? companyId = null,
        SuccessStoryStatus? status = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = true,
        bool includeIsHide = true,
        bool asNoTracking = false)
    {
        IQueryable<SuccessStory> query = _context.SuccessStories;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (!includeIsHide)
            query = query.Where(e => e.IsHide != true);

        // Apply filters
        if (companyId.HasValue)
            query = query.Where(s => s.CompanyId == companyId.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s => s.Title.Value.ToLower().Contains(term)||
                                     (s.Description != null && s.Description.Value != null && s.Description.Value.ToLower().Contains(term))||
                                     (s.SectorName != null && s.SectorName.ToLower().Contains(term))||
                                     (s.SuccessStoryType != null && s.SuccessStoryType.Name.ToLower().Contains(term))
                                     
                                     );
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = (sortBy?.ToLower(), sortDescending) switch
        {
            ("title", true) => query.OrderByDescending(s => s.Title.Value),
            ("title", false) => query.OrderBy(s => s.Title.Value),
            ("createdat", true) => query.OrderByDescending(s => s.CreatedAt),
            ("createdat", false) => query.OrderBy(s => s.CreatedAt),
            ("submissiondate", true) => query.OrderByDescending(s => s.CreatedAt),
            ("submissiondate", false) => query.OrderBy(s => s.CreatedAt),
            ("status", true) => query.OrderByDescending(s => s.Status),
            ("status", false) => query.OrderBy(s => s.Status),
            _ => query.OrderByDescending(s => s.CreatedAt)
        };

        // Apply pagination
        var items = await query.Include(e => e.CollaboratedProfiles)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedList<SuccessStory>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<List<SuccessStory>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.SuccessStories.Include(e => e.CollaboratedProfiles).Include(f=>f.SuccessStoryType)
            .Where(s => s.CompanyId == companyId && s.Status == SuccessStoryStatus.Published)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    //ToDo : Move these methods to Dashboard Repository

    #region Dashboard Statistics

    public async Task<int> GetTotalCountByStatusAsync(SuccessStoryStatus status, DateTime? fromDate = null)
    {
        var query = _context.SuccessStories.Where(s => s.Status == status);

        if (fromDate.HasValue)
            query = query.Where(s => s.CreatedAt.Year == fromDate.Value.Year);

        return await query.CountAsync();
    }

    public async Task<int> GetCountByCompanyAndStatusAsync(Guid companyId, SuccessStoryStatus status, DateTime? fromDate = null)
    {
        var query = _context.SuccessStories
            .Where(s => s.CompanyId == companyId && s.Status == status);

        if (fromDate.HasValue)
            query = query.Where(s => s.CreatedAt.Year == fromDate.Value.Year);

        return await query.CountAsync();
    }
    
    public async Task<(int PublishedCount, int TotalCount)> GetTotalCount(DateTime? fromDate = null)
    {
        IQueryable<SuccessStory> query = _context.SuccessStories;

        if (fromDate.HasValue)
            query = query.Where(s => s.CreatedAt.Year == fromDate.Value.Year);

        int totalCount = await query.CountAsync();
        int publishedCount = await query.Where(s => s.Status == SuccessStoryStatus.Published).CountAsync();
        return (publishedCount, totalCount);
    }

    #endregion
    public async Task<int> GetNextRequestIdAsync(CancellationToken cancellationToken = default)
    {
        var expectedPrefix = $"{_requestCodeSettings.Prefix}{_requestCodeSettings.Separator}";

        var requestIds = await _context.SuccessStories
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

    public async Task<bool> CheckTitleUniqueness(string Title,Guid? Id)
    {
        if (Id != null)
        {
            return await _context.SuccessStories.AnyAsync(x => x.Title.Value == Title && x.Id != Id);
        }
        else
        {
            return await _context.SuccessStories.AnyAsync(x => x.Title.Value == Title);
        }
    }
}
