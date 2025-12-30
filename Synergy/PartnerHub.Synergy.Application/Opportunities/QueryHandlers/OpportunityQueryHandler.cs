using MediatR;
using PartnersHub.Synergy.Application.Common.Helpers;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.Opportunities.Commands;
using PartnersHub.Synergy.Application.Opportunities.DTOs;
using PartnersHub.Synergy.Application.Opportunity.Queries;
using PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate;
using PartnersHub.Synergy.Domain.Aggregates.Synergy.Lookups;
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Domain.Common;

public class GetOpportunityDetailsQueryHandler : IRequestHandler<GetOpportunityDetailsQuery, Result<OpportunityResponseDto>>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ISynergyCompanyRepository _synergyCompanyRepository;
    private readonly IExpectedOutcomesRepository _expectedOutcomesRepository;
    private readonly ICollaborationRequirementRepository _collaborationRequirementRepository;
    public GetOpportunityDetailsQueryHandler(IOpportunityRepository opportunityRepository, ISynergyCompanyRepository synergyCompanyRepository, IExpectedOutcomesRepository expectedOutcomesRepository,
        ICollaborationRequirementRepository collaborationRequirementRepository)
    {
        _opportunityRepository = opportunityRepository;
        _synergyCompanyRepository = synergyCompanyRepository;
        _collaborationRequirementRepository = collaborationRequirementRepository;
        _expectedOutcomesRepository = expectedOutcomesRepository;
    }

    public async Task<Result<OpportunityResponseDto>> Handle(GetOpportunityDetailsQuery request, CancellationToken cancellationToken)
    {
        Opportunity opportunity = await _opportunityRepository.GetByIdAsync(request.Id, true,
        o => o.OpportunityType,
        o => o.ThematicArea,
        o => o.Sector,
        o => o.ExpectedOutcomes,
        o => o.CollaborationRequirements,
        o => o.OpportunityType,
        o => o.CollaboratedCompanies,
        o => o.RepresentativeInformation,
        o => o.Attachments);


        List<SynergyCompany> associatedSynergyCompanies = await _synergyCompanyRepository.GetByIdsAsync(opportunity.CollaboratedCompanies.Select(cc => cc.SynergyCompanyId).ToList());
        List<ExpectedOutcome> expectedOutcomes = await _expectedOutcomesRepository.GetByIdsAsync(opportunity.ExpectedOutcomes.Select(e => e.ExpectedOutcomeId).ToList());
        List<CollaborationRequirement> collaborationRequirements = await _collaborationRequirementRepository.GetByIdsAsync(opportunity.CollaborationRequirements.Select(c => c.CollaborationRequirementId).ToList());
        if (opportunity == null)
        {
            return Result<OpportunityResponseDto>.Failure("Opportunity doesn't exist");
        }
        SynergyCompany creatorCompany = await _synergyCompanyRepository.GetByIdAsync(opportunity.CompanyId);
        return Result<OpportunityResponseDto>.Success(Helpers.CreateResponseObject(opportunity, associatedSynergyCompanies, creatorCompany, expectedOutcomes, collaborationRequirements));
    }


}

public class GetOpportunitiesByCompanyIdQueryHandler : IRequestHandler<GetOpportunitiesByCompanyIdQuery, Result<List<OpportunityResponseDto>>>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ISynergyCompanyRepository _synergyCompanyRepository;
    private readonly IExpectedOutcomesRepository _expectedOutcomesRepository;
    private readonly ICollaborationRequirementRepository _collaborationRequirementRepository;
    public GetOpportunitiesByCompanyIdQueryHandler(IOpportunityRepository opportunityRepository, ISynergyCompanyRepository synergyCompanyRepository,
        IExpectedOutcomesRepository expectedOutcomesRepository, ICollaborationRequirementRepository collaborationRequirementRepository)
    {
        _opportunityRepository = opportunityRepository;
        _synergyCompanyRepository = synergyCompanyRepository;
        _collaborationRequirementRepository = collaborationRequirementRepository;
        _expectedOutcomesRepository = expectedOutcomesRepository;
    }

    public async Task<Result<List<OpportunityResponseDto>>> Handle(GetOpportunitiesByCompanyIdQuery request, CancellationToken cancellationToken)
    {
        Dictionary<Opportunity, List<SynergyCompany>> opportunitiesCompaniesDictionary = await _opportunityRepository.GetOpportunitiesWithCompaniesByCompanyId(request.CompanyId, true,
                                o => o.OpportunityType,
                                o => o.ThematicArea,
                                o => o.Sector,
                                o => o.CollaboratedCompanies,
                                o => o.ExpectedOutcomes,
                                o => o.CollaborationRequirements,
                                o => o.ThematicArea,
                                o => o.OpportunityType,
                                o => o.Description,
                                o => o.Title,
                                o => o.RepresentativeInformation,
                                o => o.Attachments);


        //To be refactored to Dapper 
        if (opportunitiesCompaniesDictionary == null || opportunitiesCompaniesDictionary.Count == default(int))
        {
            return Result<List<OpportunityResponseDto>>.Success(new List<OpportunityResponseDto>(), "No Opportunities Exist");
        }
        List<OpportunityResponseDto> opportunityDetailsList = new List<OpportunityResponseDto>();
        List<ExpectedOutcome> expectedOutcomes = await _expectedOutcomesRepository.GetAllAsync();
        List<CollaborationRequirement> collaborationRequirements = await _collaborationRequirementRepository.GetAllAsync();
        List<SynergyCompany> creatorCompanies = await _synergyCompanyRepository.GetByIdsAsync(opportunitiesCompaniesDictionary.Select(o => o.Key).Select(o => o.CompanyId).ToList());
        foreach (var entry in opportunitiesCompaniesDictionary)
        {
            opportunityDetailsList.Add(Helpers.CreateResponseObject(entry.Key, entry.Value, creatorCompanies?.FirstOrDefault(cc => cc.Id == entry.Key.CompanyId),
                expectedOutcomes.Where(e => entry.Key.ExpectedOutcomes.Select(ex => ex.ExpectedOutcomeId).Contains(e.Id)).ToList(),
                collaborationRequirements.Where(c => entry.Key.CollaborationRequirements.Select(cr => cr.CollaborationRequirementId).Contains(c.Id)).ToList()));
        }
        return Result<List<OpportunityResponseDto>>.Success(opportunityDetailsList);

    }



}
public class GetOpportunitiesByStatusHandler : IRequestHandler<GetOpportunitiesByStatusQuery, Result<List<OpportunityResponseDto>>>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ISynergyCompanyRepository _synergyCompanyRepository;
    private readonly IExpectedOutcomesRepository _expectedOutcomesRepository;
    private readonly ICollaborationRequirementRepository _collaborationRequirementRepository;
    public GetOpportunitiesByStatusHandler(IOpportunityRepository opportunityRepository, ISynergyCompanyRepository synergyCompanyRepository,
        IExpectedOutcomesRepository expectedOutcomesRepository, ICollaborationRequirementRepository collaborationRequirementRepository)
    {
        _opportunityRepository = opportunityRepository;
        _synergyCompanyRepository = synergyCompanyRepository;
        _collaborationRequirementRepository = collaborationRequirementRepository;
        _expectedOutcomesRepository = expectedOutcomesRepository;
    }

    public async Task<Result<List<OpportunityResponseDto>>> Handle(GetOpportunitiesByStatusQuery request, CancellationToken cancellationToken)
    {
        Dictionary<Opportunity, List<SynergyCompany>> opportunitiesCompaniesDictionary = await _opportunityRepository.GetOpportunitiesWithCompaniesByStatus(request.Status, true,
                                o => o.OpportunityType,
                                o => o.ThematicArea,
                                o => o.Sector,
                                o => o.CollaboratedCompanies,
                                o => o.ExpectedOutcomes,
                                o => o.CollaborationRequirements,
                                o => o.ThematicArea,
                                o => o.OpportunityType,
                                o => o.Description,
                                o => o.Title,
                                o => o.RepresentativeInformation,
                                o => o.Attachments);


        //To be refactored to Dapper 
        if (opportunitiesCompaniesDictionary == null || opportunitiesCompaniesDictionary.Count == default(int))
        {
            return Result<List<OpportunityResponseDto>>.Success(new List<OpportunityResponseDto>(), "No Opportunities Exist");
        }
        List<OpportunityResponseDto> opportunityDetailsList = new List<OpportunityResponseDto>();
        List<ExpectedOutcome> expectedOutcomes = await _expectedOutcomesRepository.GetAllAsync();
        List<CollaborationRequirement> collaborationRequirements = await _collaborationRequirementRepository.GetAllAsync();
        List<SynergyCompany> creatorCompanies = await _synergyCompanyRepository.GetByIdsAsync(opportunitiesCompaniesDictionary.Select(o => o.Key).Select(o => o.CompanyId).ToList());
        foreach (var entry in opportunitiesCompaniesDictionary)
        {
            opportunityDetailsList.Add(Helpers.CreateResponseObject(entry.Key, entry.Value, creatorCompanies?.FirstOrDefault(cc => cc.Id == entry.Key.CompanyId),
                expectedOutcomes.Where(e => entry.Key.ExpectedOutcomes.Select(ex => ex.ExpectedOutcomeId).Contains(e.Id)).ToList(),
                collaborationRequirements.Where(c => entry.Key.CollaborationRequirements.Select(cr => cr.CollaborationRequirementId).Contains(c.Id)).ToList()));
        }
        return Result<List<OpportunityResponseDto>>.Success(opportunityDetailsList);

    }




}
public class GetAllOpportunitiesQueryHandler : IRequestHandler<GetAllOpportunitiesQuery, Result<List<GuidKeyValueDto>>>
{
    private readonly IOpportunityRepository _opportunityRepository;

    public GetAllOpportunitiesQueryHandler(IOpportunityRepository opportunityRepository)
    {
        _opportunityRepository = opportunityRepository;

    }

    public async Task<Result<List<GuidKeyValueDto>>> Handle(GetAllOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        List<Opportunity> opportunities = await _opportunityRepository.GetAllAsync();

        if (opportunities == null)
        {
            return Result<List<GuidKeyValueDto>>.Success(new List<GuidKeyValueDto>());
        }
        return Result<List<GuidKeyValueDto>>.Success(opportunities.Select(o => new GuidKeyValueDto(o.Id, o.Title.Value)).ToList());
    }


}

internal static class Helpers
{
    internal static OpportunityResponseDto CreateResponseObject(Opportunity opportunity, List<SynergyCompany> associatedSynergyCompanies, SynergyCompany? creatorCompany,
        List<ExpectedOutcome> expectedOutcomes, List<CollaborationRequirement> collaborationRequirements)
    {


        // The method returns an IEnumerable<int> and uses yield return.
        static IEnumerable<int> GetIncrementedNumbers(int count, int start = 1)
        {
            for (int i = 0; i < count; i++)
            {
                // 'yield return' pauses the execution and returns the current value.
                // When the next value is requested, execution resumes here.
                yield return start + i;
            }
        }



        var opportunityDetailsDto = new OpportunityResponseDto
        {
            Id = opportunity.Id,
            RequestId = opportunity.RequestId,
            CompanyId = opportunity.CompanyId,
            CompanyName = creatorCompany?.Name?.Value,
            CompanyLogo = LogoHelper.ToBase64String(creatorCompany?.Logo),
            Title = opportunity.Title.Value,
            Description = opportunity.Description.Value,
            TypeId = opportunity.OpportunityTypeId,
            TypeName = string.Concat(opportunity.OpportunityType.Name.ToString().Select(c => char.IsUpper(c) ?
                " " + c.ToString() : c.ToString())),
            Status = opportunity.Status,
            StatusDescription = MapStatusToDisplay(opportunity.Status),
            ThematicAreaId = opportunity.ThematicAreaId,
            ThematicAreaName = opportunity.ThematicArea.Name,
            SectorName = opportunity.Sector.Value,
            SectorId = opportunity.Sector.Id,
            CollaborationRationale = opportunity.CollaborationRationale,
            CollaborationRequirements = collaborationRequirements.Select(cr => new KeyValueDto(cr.Id, cr.Name)).ToList(),
            CollaborationRequirementOther = opportunity.CollaborationRequirementOther,
            ExpectedOutcomes = expectedOutcomes.Select(eo => new KeyValueDto(eo.Id, eo.Name)).ToList(),
            ExpectedOutcomeOther = opportunity.ExpectedOutcomeOther,
            StartDate = opportunity.StartDate,
            EndDate = opportunity.EndDate,
            RepresentaitveTitle = opportunity.RepresentativeInformation.Position,
            RepresentativeEmail = opportunity.RepresentativeInformation.Email,
            RepresentativeName = opportunity.RepresentativeInformation.Name,
            RepresentativePhone = opportunity.RepresentativeInformation.Phone,
            TermsAndConditionId = opportunity.TermsAndConditionId,
            CreatedBy = opportunity.CreatedBy,
            RejectionReason = opportunity.RejectionReason,
            CollaboratedProfiles = associatedSynergyCompanies
                    .Select(c => new GuidKeyValueDto(c.Id, c.Name.Value.ToString()))
                    .ToList(),
            Attachments = opportunity.Attachments
                    .Select(a => new OpportunityAttachmentDto
                    {
                        Id = a.Id,
                        OpportunityId = a.OpportunityId,
                        FileName = a.FileName,
                        SharePointUrl = a.SharePointUrl,
                        FileExtension = a.FileExtension,
                        FileSizeInBytes = a.FileSizeInBytes,
                        UploadedAt = a.UploadedAt,
                        UploadedBy = a.UploadedBy
                    })
                    .OrderByDescending(a => a.UploadedAt)
                    .ToList()
        };
        return opportunityDetailsDto;

    }
    private static string MapStatusToDisplay(OpportunityStatus status)
    {
        return status switch
        {
            OpportunityStatus.PendingReview => "Pending",
            OpportunityStatus.Pending => "Pending",
            OpportunityStatus.Published => "Published",
            OpportunityStatus.AssetManagerRejected => "Rejected",
            OpportunityStatus.AdminRejected => "Rejected",
            _ => "Draft"
        };
    }
}

