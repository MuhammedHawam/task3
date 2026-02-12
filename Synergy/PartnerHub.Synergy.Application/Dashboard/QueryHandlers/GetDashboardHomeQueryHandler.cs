using MediatR;
using PartnersHub.Synergy.Application.Common.Helpers;
using PartnersHub.Synergy.Application.Dashboard.DTOs;
using PartnersHub.Synergy.Application.SynergyCompany.Queries;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Application.Dashboard.Queries;
using PartnersHub.Synergy.Application.SynergyCompany.DTOs;
using PartnersHub.Synergy.Application.Interfaces.Common;

namespace PartnersHub.Synergy.Application.Dashboard.QueryHandlers;

/// <summary>
/// Single handler for complete dashboard home page - fetches all data in one go
/// </summary>
public class GetDashboardHomeQueryHandler : IRequestHandler<GetDashboardHomeQuery, Result<DashboardHomeDto>>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ISuccessStoryRepository _successStoryRepository;
    private readonly ISynergyCompanyRepository _companyRepository;
    public GetDashboardHomeQueryHandler(
        IOpportunityRepository opportunityRepository,
        ISuccessStoryRepository successStoryRepository,
        ISynergyCompanyRepository companyRepository)
    {
        _opportunityRepository = opportunityRepository;
        _successStoryRepository = successStoryRepository;
        _companyRepository = companyRepository;
    }

    public async Task<Result<DashboardHomeDto>> Handle(GetDashboardHomeQuery request, CancellationToken cancellationToken)
    {
        var year = request.Year ?? DateTime.UtcNow.Year;
        var startOfYear = new DateTime(year, 1, 1);

        // Fetch data sequentially to avoid DbContext concurrency issues
        var kpis = await FetchKPIsAsync(request.CompanyId, startOfYear);
        var opportunities = await FetchRecentOpportunitiesAsync();
        var stories = await FetchRecentSuccessStoriesAsync();
        var companies = await FetchRecentCompaniesAsync(opportunities);

        var dashboard = new DashboardHomeDto
        {
            KPIs = kpis,
            RecentOpportunities = opportunities,
            RecentSuccessStories = stories,
            RecentCompanies = companies
        };

        return Result<DashboardHomeDto>.Success(dashboard);
    }

    private async Task<DashboardKPIsDto> FetchKPIsAsync(Guid companyId, DateTime? fromDate)
    {
        var totalCompanies = await _companyRepository.GetTotalCompaniesCountAsync();
        var collaborationsCount = await _opportunityRepository.GetDistinctCollaboratedCompaniesCountAsync(companyId, fromDate);

        var totalPublishedOpp = await _opportunityRepository.GetTotalCountByStatusAsync(OpportunityStatus.Published, fromDate);

        var companyPublishedOpp = await _opportunityRepository.GetCountByCompanyAndStatusAsync(companyId, OpportunityStatus.Published, fromDate);

        var totalStories = await _successStoryRepository.GetTotalCountByStatusAsync(SuccessStoryStatus.Published, fromDate);
        var companyStories = await _successStoryRepository.GetCountByCompanyAndStatusAsync(companyId, SuccessStoryStatus.Published, fromDate);

        return new DashboardKPIsDto
        {
            PortfolioCompanies = new PortfolioCompaniesKPI
            {
                TotalRegistered = totalCompanies,
                YourCollaborations = collaborationsCount
            },
            ActiveOpportunities = new ActiveOpportunitiesKPI
            {
                TotalAcrossSynergy = totalPublishedOpp,
                YourCompanyOpportunities = companyPublishedOpp,
            },
            SuccessStories = new SuccessStoriesKPI
            {
                TotalPublished = totalStories,
                YourCompanyStories = companyStories
            }
        };
    }

    private async Task<List<RecentOpportunityCardDto>> FetchRecentOpportunitiesAsync()
    {
        var opportunities = await _opportunityRepository.SearchOpportunitiesAsync(
            pageNumber: 1,
            pageSize: 4,
            statuses: new List<OpportunityStatus> { OpportunityStatus.Published },
            sortBy: "CreatedAt",
            IncludeIsHide:false,
            asNoTracking: true);

        var companies = await _companyRepository.GetByIdsAsync(opportunities.Items.Select(o => o.CompanyId).Distinct().ToList());
        var companyDict = companies.ToDictionary(c => c.Id);
        
        return opportunities.Items.Select(o =>
        {
            var company = companyDict.GetValueOrDefault(o.CompanyId);
            return new RecentOpportunityCardDto
            {
                Id = o.Id,
                Title = o.Title.Value,
                RequestId = o.RequestId,
                PostedByCompany = new CompanyInfoDto
                {
                    Id = o.CompanyId,
                    Name = company?.Name.Value ?? "Unknown",
                    Logo = LogoHelper.ToBase64String(company?.Logo)
                },
                Description = o.Description.Value,
                CollaborationType = o.OpportunityType?.Name ?? "N/A",
                Sector = o.Sector?.Value ?? "N/A",
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                CompanyName = company?.Name.Value ?? "Unknown",
                IsHide = o.IsHide
            };
        }).ToList();
    }

    private async Task<List<RecentSuccessStoryCardDto>> FetchRecentSuccessStoriesAsync()
    {
        var stories = await _successStoryRepository.GetPaginatedAsync(
            pageNumber: 1,
            pageSize: 4,
            status: SuccessStoryStatus.Published,
            sortBy: "CreatedAt",
            sortDescending: true,
            includeIsHide : false,
            asNoTracking: true);

        var companyIds = stories.Items
                                .SelectMany(s => new[] { s.CompanyId }
                                .Concat(s.CollaboratedProfiles.Select(cp => cp.SynergyCompanyId)))
                                .Distinct()
                                .ToList();

        var companies = await _companyRepository.GetByIdsAsync(companyIds);
        var companyDict = companies.ToDictionary(c => c.Id);

        return stories.Items.Select(s =>
        {
            var company = companyDict.GetValueOrDefault(s.CompanyId);
            return new RecentSuccessStoryCardDto
            {
                Id = s.Id,
                RequestId = s.RequestId,
                Title = s.Title.Value,
                SourceCompany = new CompanyInfoDto
                {
                    Id = s.CompanyId,
                    Name = company?.Name.Value ?? "Unknown",
                    Logo = LogoHelper.ToBase64String(company?.Logo)
                },
                PartnerCompanies = s.CollaboratedProfiles?
                                    .Select(p => new CompanyInfoDto
                                                      {
                                                         Id = p.SynergyCompanyId,
                                                         Name = companyDict.GetValueOrDefault(p.SynergyCompanyId)?.Name.Value ?? "Unknown",  
                                                         Logo = LogoHelper.ToBase64String(companyDict.GetValueOrDefault(p.SynergyCompanyId)?.Logo) 
                                    })
                                    .ToList() ?? new List<CompanyInfoDto>(),
                Description = s.Description.Value,
                Type = MapSuccessStoryType(s.SuccessStoryTypeId),
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                CompanyName = company?.Name.Value ?? "Unknown",
                IsHide = s.IsHide
            };
        }).ToList();
    }

    private async Task<List<RecentCompanyCardDto>> FetchRecentCompaniesAsync(List<RecentOpportunityCardDto> Opportunities)
    {
        var companies = await _companyRepository.GetRecentAsync(4, includes: c => c.Sectors);

        return companies.Select(c => new RecentCompanyCardDto
        {
            Id = c.Id,
            Name = c.Name.Value,
            Logo = LogoHelper.ToBase64String(c.Logo),
            Description = c.Description?.Value ?? string.Empty,
            Sectors = c.Sectors.Select(s => new CompanySectorDto
            {
                SectorId = s.SectorId,
                SectorName = s.SectorName
            }).ToList(),
            HeadquarterCountry = c.HeadquarterCountry,
            HeadquarterCity = c.HeadquarterCity,
            RegisteredDate = c.CreatedAt,
            TotalCollaborationNumber = Opportunities.Where(s => s.PostedByCompany.Id == c.Id).Count()
        }).ToList();
    }

    private static string MapSuccessStoryType(int typeId)
    {
        // TODO: Load from lookup table
        return typeId switch
        {
            1 => "Partnership",
            2 => "Collaboration",
            3 => "Joint Venture",
            _ => "Unknown"
        };
    }
}
