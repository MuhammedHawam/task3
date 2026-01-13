using MediatR;
using PartnersHub.Synergy.Application.Common;
using PartnersHub.Synergy.Application.Common.Helpers;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.Opportunities.DTOs;
using PartnersHub.Synergy.Application.Opportunities.Queries;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.Opportunities.QueryHandlers;

public class SearchOpportunitiesQueryHandler : IRequestHandler<SearchOpportunitiesQuery, Result<PaginatedList<OpportunitySearchCardDto>>>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ISynergyCompanyRepository _companyRepository;
    private readonly IOpportunityTypeRepository _opportunityTypeRepository;
    private readonly IThematicAreaRepository _thematicAreaRepository;
    private readonly ICollaborationRequirementRepository _collaborationRequirementRepository;
    private readonly IExpectedOutcomesRepository _expectedOutcomesRepository;

    public SearchOpportunitiesQueryHandler(
        IOpportunityRepository opportunityRepository,
        ISynergyCompanyRepository companyRepository,
        IOpportunityTypeRepository opportunityTypeRepository,
        IThematicAreaRepository thematicAreaRepository,
        ICollaborationRequirementRepository collaborationRequirementRepository,
        IExpectedOutcomesRepository expectedOutcomesRepository)
    {
        _opportunityRepository = opportunityRepository;
        _companyRepository = companyRepository;
        _opportunityTypeRepository = opportunityTypeRepository;
        _thematicAreaRepository = thematicAreaRepository;
        _collaborationRequirementRepository = collaborationRequirementRepository;
        _expectedOutcomesRepository = expectedOutcomesRepository;
    }

    public async Task<Result<PaginatedList<OpportunitySearchCardDto>>> Handle(SearchOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        // Parse status if provided
        List<OpportunityStatus>? statuses = new List<OpportunityStatus>();
        if (request.Statuses != null && request.Statuses.Count > default(int))
        {
            foreach(int enumInt in request.Statuses)
            {
                OpportunityStatus opportunityStatus = (OpportunityStatus)enumInt;
                statuses.Add(opportunityStatus);
            }
        }
        // Parse collaboration statuses if provided
        List<CollaborationStatusFilter>? collaborationStatuses = new List<CollaborationStatusFilter>();
        if (request.CollaborationStatuses != null && request.CollaborationStatuses.Count > default(int))
        {
            foreach (int enumInt in request.CollaborationStatuses)
            {
                CollaborationStatusFilter collaborationStatus = (CollaborationStatusFilter)enumInt;
                collaborationStatuses.Add(collaborationStatus);
            }
            collaborationStatuses = collaborationStatuses.Distinct().ToList();
        }

        // Call repository with all filters - filtering happens at database level
        var paginatedResult = await _opportunityRepository.SearchOpportunitiesAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            companyIds: request.CompanyIds,
            searchTerm: request.SearchTerm,
            sectorIds: request.SectorIds,
            opportunityTypeIds: request.OpportunityTypeIds,
            thematicAreaIds: request.ThematicAreaIds,
            collaborationRequirementIds: request.CollaborationRequirementIds,
            expectedOutcomeIds: request.ExpectedOutcomeIds,
            statuses: statuses,
            collaborationStatuses : collaborationStatuses,
            startDate:request.StartDate, 
            endDate:request.EndDate,
            sortBy: request.SortBy,
            asNoTracking: true);

        // Get company information for the opportunities
        var companyIds = paginatedResult.Items.Select(o => o.CompanyId).Distinct().ToList();
        var companies = await _companyRepository.GetByIdsAsync(companyIds, asNoTracking: true);
        var companyDict = companies.ToDictionary(c => c.Id);
        var collaborationRequirements = await _collaborationRequirementRepository.GetAllAsync();
        var expectedOutcomes = await _expectedOutcomesRepository.GetAllAsync();
        
        // Map to DTOs
        var opportunityDtos = paginatedResult.Items.Select(o =>
        {
            var company = companyDict.GetValueOrDefault(o.CompanyId);

            return new OpportunitySearchCardDto
            {
                Id = o.Id,
                RequestId = o.RequestId,
                Title = o.Title.Value,
                Description = o.Description?.Value ?? string.Empty,
                Status =o.Status,
                State = (o.StartDate > DateOnly.FromDateTime(DateTime.Now)) ? CollaborationStatusFilter.Upcoming.ToString() :
                                                                   ((o.StartDate <= DateOnly.FromDateTime(DateTime.Now) && (o.EndDate == null || o.EndDate >= DateOnly.FromDateTime(DateTime.Now))) ?
                                                                   CollaborationStatusFilter.Active.ToString() : (o.StartDate == null ? CollaborationStatusFilter.Upcoming.ToString() : CollaborationStatusFilter.Closed.ToString())),
                StatusDescription = MapStatusToDisplay(o.Status),
                CompanyId = o.CompanyId,
                CompanyName = company?.Name.Value ?? "Unknown Company",
                CompanyLogo = LogoHelper.ToBase64String(company?.Logo),
                OpportunityTypeId = o.OpportunityTypeId,
                OpportunityTypeName = o.OpportunityType?.Name ?? "N/A",
                ThematicAreaId = o.ThematicAreaId,
                ThematicAreaName = o.ThematicArea?.Name ?? "N/A",
                SectorId = o.Sector.Id,
                SectorName = o.Sector.Value,
                CollaborationRequirements = collaborationRequirements.
                  Where(cr => o.CollaborationRequirements.Select(c => c.CollaborationRequirementId).Contains(cr.Id)).Select(cr => cr.Name).ToList(),
                ExpectedOutcomes = expectedOutcomes.
                  Where(eo => o.ExpectedOutcomes.Select(e => e.ExpectedOutcomeId).Contains(eo.Id)).Select(eo => eo.Name).ToList(),
                CollaboratedCompaniesCount = o.CollaboratedCompanies.Count,
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                CreatedAt = o.CreatedAt,
                IsHide = o.IsHide
            };
        }).ToList();

    // Build available filters (these need separate queries to get all available options)
    var result = new PaginatedList<OpportunitySearchCardDto>
    {
            Items = opportunityDtos,
            TotalCount = paginatedResult.TotalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,

        };

        return Result<PaginatedList<OpportunitySearchCardDto>>.Success(result);
    }
    private string MapStatusToDisplay(OpportunityStatus status)
    {
        return status switch
        {
            OpportunityStatus.PendingReview => "Pending",
            OpportunityStatus.Pending => "Approved",
            OpportunityStatus.Published => "Published",
            OpportunityStatus.AssetManagerRejected => "Rejected",
            OpportunityStatus.AdminRejected => "Rejected",
            _ => "Draft"
        };
    }

}
