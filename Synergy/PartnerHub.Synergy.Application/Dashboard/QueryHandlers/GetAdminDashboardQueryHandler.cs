using MediatR;
using PartnersHub.Synergy.Application.Dashboard.DTOs;
using PartnersHub.Synergy.Application.Dashboard.Queries;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Interfaces.Repository.Dapper;
using PartnersHub.Synergy.Domain.Common;

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardKPIsDto>>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ISuccessStoryRepository _successStoryRepository;
    private readonly ISynergyCompanyRepository _companyRepository;
    private readonly IDapperRepository _dapperRepository;
    public GetAdminDashboardQueryHandler(
        IOpportunityRepository opportunityRepository,
        ISuccessStoryRepository successStoryRepository,
        ISynergyCompanyRepository companyRepository,
        IDapperRepository dapperRepository)
    {
        _opportunityRepository = opportunityRepository;
        _successStoryRepository = successStoryRepository;
        _companyRepository = companyRepository;
        _dapperRepository = dapperRepository;
    }

    public async Task<Result<AdminDashboardKPIsDto>> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        var year = request.Year ?? DateTime.UtcNow.Year;
        var startOfYear = new DateTime(year, 1, 1);

        int totalActiveCompaniesCount = await _companyRepository.GetTotalActiveCompaniesCountAsync();

        var (PublishedSuccessStoriesCount, totalSuccessStoriesCount) = await _successStoryRepository.GetTotalCount();
        int totalPendingApprovalSuccessStories = await _successStoryRepository.GetTotalCountByStatusAsync(SuccessStoryStatus.PendingReview);

        var (PublishedOpportunityCount,totalOpportunitiesCount) = await _opportunityRepository.GetTotalCount();
        int totalPendingReviewOpportunitiesCount = await _opportunityRepository.GetTotalCountByStatusAsync(OpportunityStatus.PendingReview);

        var sectorsKPIs = await _dapperRepository.FetchSectorsKPIs();
        var engagementTrends = await _dapperRepository.FetchYTDMonthlyEngagementTrends();
        var topPerformingCompanies = await _dapperRepository.FetchCompanyKPIsAsync(null,5);
        var collaborationTypeKPIs = await _dapperRepository.FetchCollaborationTypeKPIs();



        AdminDashboardKPIsDto AdminDashboardKPIs = new AdminDashboardKPIsDto()
        {
            TotalActiveCompaniesCount = totalActiveCompaniesCount,
            TotalOpportunitiesCount = totalOpportunitiesCount,
            TotalPendingApprovalSuccessStories = totalPendingApprovalSuccessStories,
            PublishedOpportunitiesCount = PublishedOpportunityCount,
            TotalPendingReviewOpportunitiesCount = totalPendingReviewOpportunitiesCount,
            TotalSuccessStoriesCount = totalSuccessStoriesCount,
            PublishedSuccessStoriesCount = PublishedSuccessStoriesCount,
            SectorsKPIs = sectorsKPIs,
            CollaborationTypeKPIs = collaborationTypeKPIs,
            EngagementTrends = engagementTrends,
            TopPerformingCompanies = topPerformingCompanies,

        };
        return Result<AdminDashboardKPIsDto>.Success(AdminDashboardKPIs);
    }
    public async Task<List<SectorKPI>> FetchSectorKPIs(DateTime? startDate = null)
    {

        return null;
    }
}