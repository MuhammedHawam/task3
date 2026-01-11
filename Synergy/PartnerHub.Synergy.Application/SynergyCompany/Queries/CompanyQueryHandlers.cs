using MediatR;
using PartnersHub.Synergy.Application.Common.Helpers;
using PartnersHub.Synergy.Application.Dashboard.DTOs;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.SynergyCompany.DTOs;
using PartnersHub.Synergy.Domain.Aggregates.Synergy.Lookups;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SynergyCompany.Queries;

public class GetRegisteredCompaniesQueryHandler : IRequestHandler<GetRegisteredCompaniesQuery, Result<PaginatedList<RegisteredCompanyCardDto>>>
{
    private readonly ISynergyCompanyRepository _companyRepository;
    private readonly IOpportunityRepository _opportunityRepository;

    public GetRegisteredCompaniesQueryHandler(
        ISynergyCompanyRepository companyRepository,
        IOpportunityRepository opportunityRepository)
    {
        _companyRepository = companyRepository;
        _opportunityRepository = opportunityRepository;
    }

    public async Task<Result<PaginatedList<RegisteredCompanyCardDto>>> Handle(GetRegisteredCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companiesPaginatedList = await _companyRepository.Search(request.PageSize, request.PageNumber, 
            searchTerm: request.SearchTerm, sectors: request.SectorIds,
            cities: request.Cities, countries: request.Countries);

        var companyDtos = new List<RegisteredCompanyCardDto>();
        foreach (var company in companiesPaginatedList.Items)
        {
            // Filter by IsActive status unless IncludeInactive is true (admin access)
            if (!request.IncludeInactive && !company.IsActive)
            {
                continue;
            }

            var collaborationsCount = await _opportunityRepository.GetDistinctCollaboratedCompaniesCountAsync(company.Id);
            
            companyDtos.Add(new RegisteredCompanyCardDto
            {
                Id = company.Id,
                Name = company.Name.Value,
                Logo = LogoHelper.ToBase64String(company.Logo),
                Sectors = company.Sectors.Select(s => new CompanySectorDto 
                { 
                    SectorId = s.SectorId, 
                    SectorName = s.SectorName 
                }).ToList(),
                HeadquarterCountry = company.HeadquarterCountry,
                HeadquarterCity = company.HeadquarterCity,
                TotalCollaborationNumber = collaborationsCount,
                Description = company.Description.Value,
                IsActive = company.IsActive
            });
        }

        var allCompaniesForFilters = await _companyRepository.GetAllAsync(asNoTracking: true);
        
        // Filter active companies for filter options unless admin
        if (!request.IncludeInactive)
        {
            allCompaniesForFilters = allCompaniesForFilters.Where(c => c.IsActive).ToList();
        }


        return Result<PaginatedList<RegisteredCompanyCardDto>>.Success(new PaginatedList<RegisteredCompanyCardDto>(companyDtos, companiesPaginatedList.TotalCount, request.PageNumber, request.PageSize));
    }


 
}

public class GetCompanyDetailsQueryHandler : IRequestHandler<GetCompanyDetailsQuery, Result<CompanyDetailsDto>?>
{
    private readonly ISynergyCompanyRepository _companyRepository;
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ISuccessStoryRepository _successStoryRepository;
    private readonly ICollaborationRequirementRepository _collaborationRequirementRepository;
    private readonly IExpectedOutcomesRepository _expectedOutcomesRepository;

    public GetCompanyDetailsQueryHandler(
        ISynergyCompanyRepository companyRepository,
        IOpportunityRepository opportunityRepository,
        ISuccessStoryRepository successStoryRepository,
        ICollaborationRequirementRepository collaborationRequirementRepository,
        IExpectedOutcomesRepository expectedOutcomesRepository)
    {
        _companyRepository = companyRepository;
        _opportunityRepository = opportunityRepository;
        _successStoryRepository = successStoryRepository;
        _collaborationRequirementRepository = collaborationRequirementRepository;
        _expectedOutcomesRepository = expectedOutcomesRepository;
    }

    public async Task<Result<CompanyDetailsDto>?> Handle(GetCompanyDetailsQuery request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId, asNoTracking: true, includes: c => c.Sectors);
        if (company == null)
            return null;

        var collaborationsCount = await _opportunityRepository.GetDistinctCollaboratedCompaniesCountAsync(company.Id);
        var opportunities = await _opportunityRepository.GetByPublishingCompanyId(company.Id);
        var collaborationRequirements = await _collaborationRequirementRepository.GetAllAsync();
        var expectedOutcomes = await _expectedOutcomesRepository.GetAllAsync();

        var collaborationDtos = opportunities.Select(o => new OpportunityCollaborationDto
        {
            OpportunityId = o.Id,
            Title = o.Title.Value,
            CollaborationType = o.OpportunityType?.Name ?? "N/A",
            Sector = o.Sector.Value,
            StartDate = o.StartDate,
            EndDate = o.EndDate,
            PostedByCompany = company.Name.Value,
            Description = o.Description.Value,
            Id = o.Id,    
            RequestId = o.RequestId,
            Status = o.Status,  
            StatusDescription = MapStatusToDisplay(o.Status),
            //CompanyId = o.CompanyId,
            CompanyName = company?.Name.Value ?? "Unknown Company",
            //CompanyLogo = LogoHelper.ToBase64String(company?.Logo),
            OpportunityTypeId = o.OpportunityTypeId,
            OpportunityTypeName = o.OpportunityType?.Name ?? "N/A",
            ThematicAreaId = o.ThematicAreaId,
            ThematicAreaName = o.ThematicArea?.Name ?? "N/A",
            //SectorId = o.Sector.Id,
            //SectorName = o.Sector?.Value,
            CollaborationRequirements = collaborationRequirements.
                  Where(cr => o.CollaborationRequirements.Select(c => c.CollaborationRequirementId).Contains(cr.Id)).Select(cr => cr.Name).ToList(),
            ExpectedOutcomes = expectedOutcomes.
                  Where(eo => o.ExpectedOutcomes.Select(e => e.ExpectedOutcomeId).Contains(eo.Id)).Select(eo => eo.Name).ToList(),
            CollaboratedCompaniesCount = o.CollaboratedCompanies?.Count??0,
            CreatedAt = o.CreatedAt
        }).ToList();

        var successStories = await _successStoryRepository.GetByCompanyIdAsync(company.Id);

        var companyIds = successStories.SelectMany(s  =>  s.CollaboratedProfiles.Select(cp => cp.SynergyCompanyId))
                                .Distinct()
                                .ToList();
        var companies = await _companyRepository.GetByIdsAsync(companyIds);
        var companyDict = companies.ToDictionary(c => c.Id);

        var storyDtos = successStories.Select(s => new SuccessStoryPreviewDto
        {
            StoryId = s.Id,
            Title = s.Title.Value,
            Type = "Collaboration",
            PartnerCompanies = s.CollaboratedProfiles?
                                    .Select(p => new CompanyNameLogoDto
                                    {
                                        Name = companyDict.GetValueOrDefault(p.SynergyCompanyId)?.Name.Value ?? "Unknown",
                                    })
                                    .ToList() ?? new List<CompanyNameLogoDto>(),
            PostedBy = company.Name.Value,
            PostedDate = s.CreatedAt,
            SectorName = s.SectorName,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Description = s.Description?.Value,
            RequestId = s.RequestId,
            Id = s.Id,
            CompanyId = s.CompanyId,
            CompanyName = company?.Name.Value ?? "Unknown Company",
            SuccessStoryType = s.SuccessStoryType?.ToString(),
            SuccessStoryStatus = s.Status,
            SuccessStoryStatusDescription = s.Status.ToString(),    
            SubmissionDate = s.CreatedAt
        }).ToList();

        var responseDto = new CompanyDetailsDto
        {
            Id = company.Id,
            Name = company.Name.Value,
            Logo = LogoHelper.ToBase64String(company.Logo),
            Sector = company.Sectors.Any() 
                ? new CompanySectorDto(company.Sectors.First().SectorId, company.Sectors.First().SectorName) 
                : new CompanySectorDto(),
            HeadquarterCountry = company.HeadquarterCountry,
            HeadquarterCity = company.HeadquarterCity,
            TotalCollaborationNumber = collaborationsCount,
            Description = company.Description.Value,
            IsActive = company.IsActive,
            Services = new List<string>(),
            CollaborationFocus = new List<string>(),
            Representative = new RepresentativeInfoDto
            {
                Name = company?.RepresentativeInformation.Name,
                Position = company?.RepresentativeInformation.Position,
                Email = company?.RepresentativeInformation.Email,
                Phone = company?.RepresentativeInformation.Phone
            },
            Collaborations = collaborationDtos,
            SuccessStories = storyDtos
        };
        return Result<CompanyDetailsDto>.Success(responseDto);
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
