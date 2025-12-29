using MediatR;
using PartnersHub.Synergy.Application.Common;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Lookups.DTOs;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Common;

public class GetCollaborationRequirementsQueryHandler : IRequestHandler<GetCollaborationRequirementsQuery, Result<List<KeyValueDto>>>
{
    private readonly ICollaborationRequirementRepository _repository;

    public GetCollaborationRequirementsQueryHandler(ICollaborationRequirementRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<KeyValueDto>>> Handle(GetCollaborationRequirementsQuery request, CancellationToken cancellationToken)
    {
        var collaborationRequirements = await _repository.GetAllAsync();
        var collaborationRequirementsDto = collaborationRequirements.Select(cr => new KeyValueDto(cr.Id, cr.Name)).ToList();
        return Result<List<KeyValueDto>>.Success(collaborationRequirementsDto);

    }
}

public class GetExpectedOutcomesQueryHandler : IRequestHandler<GetExpectedOutcomesQuery, Result<List<KeyValueDto>>>
{
    private readonly IExpectedOutcomesRepository _repository;

    public GetExpectedOutcomesQueryHandler(IExpectedOutcomesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<KeyValueDto>>> Handle(GetExpectedOutcomesQuery request, CancellationToken cancellationToken)
    {
        var expectedOutcomes = await _repository.GetAllAsync();
        var expectedOutcomesDto = expectedOutcomes.Select(eo => new KeyValueDto(eo.Id, eo.Name)).ToList();
        return Result<List<KeyValueDto>>.Success(expectedOutcomesDto);
    }
}

public class GetThematicAreasQueryHandler : IRequestHandler<GetThematicAreasQuery, Result<List<KeyValueDto>>>
{
    private readonly IThematicAreaRepository _repository;

    public GetThematicAreasQueryHandler(IThematicAreaRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<KeyValueDto>>> Handle(GetThematicAreasQuery request, CancellationToken cancellationToken)
    {
        var thematicAreas = await _repository.GetAllAsync();
        var thematicAreasDto = thematicAreas.Select(ta => new KeyValueDto(ta.Id, ta.Name)).ToList();
        return Result<List<KeyValueDto>>.Success(thematicAreasDto);
    }
}
public class GetOpportunityTypesQueryHandler : IRequestHandler<GetOpportunityTypesQuery, Result<List<KeyValueDto>>>
{
    private readonly IOpportunityTypeRepository _repository;

    public GetOpportunityTypesQueryHandler(IOpportunityTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<KeyValueDto>>> Handle(GetOpportunityTypesQuery request, CancellationToken cancellationToken)
    {
        var opportunityTypes = await _repository.GetAllAsync();
        var opportunityTypesDto = opportunityTypes.Select(ta => new KeyValueDto(ta.Id, ta.Name)).ToList();
        return Result<List<KeyValueDto>>.Success(opportunityTypesDto);
    }
}
public class GetSuccessStoryStatusesQueryHandler : IRequestHandler<GetSuccessStoryStatusesQuery, Result<List<KeyValueDto>>>
{
    public GetSuccessStoryStatusesQueryHandler()
    {
    }

    public async Task<Result<List<KeyValueDto>>> Handle(GetSuccessStoryStatusesQuery request, CancellationToken cancellationToken)
    {
        var successStoryStatuses = new List<KeyValueDto>
            {
                new KeyValueDto((byte)SuccessStoryStatus.PendingReview, "Pending Review" ),
                new KeyValueDto ((byte) SuccessStoryStatus.AssetManagerApproved, "Approved"),
                new KeyValueDto ((byte) SuccessStoryStatus.AdminRejected, "Rejected By Admin"),
                new KeyValueDto ((byte) SuccessStoryStatus.Published, "Published"),
            };
        return Result<List<KeyValueDto>>.Success(successStoryStatuses);
    }
}

public class SuccessStoryCollaborationStatusesQueryHandler : IRequestHandler<SuccessStoryCollaborationStatusesQuery, Result<List<KeyValueDto>>>
{
    public SuccessStoryCollaborationStatusesQueryHandler()
    {
    }

    public async Task<Result<List<KeyValueDto>>> Handle(SuccessStoryCollaborationStatusesQuery request, CancellationToken cancellationToken)
    {
        var successStoryCollaborationStatuses = new List<KeyValueDto>
            {
                new KeyValueDto((byte)SuccessStroyCollaborationStatus.Ongoing, SuccessStroyCollaborationStatus.Ongoing.ToString()),
                new KeyValueDto((byte)SuccessStroyCollaborationStatus.Successful, SuccessStroyCollaborationStatus.Successful.ToString() ),
            };
        return Result<List<KeyValueDto>>.Success(successStoryCollaborationStatuses);
    }
}

public class GetSuccessStoryTypeQueryHandler : IRequestHandler<GetSuccessStoryTypesQuery, Result<List<KeyValueDto>>>
{
    private readonly ISuccessStroyTypeRepository _repository;

    public GetSuccessStoryTypeQueryHandler(ISuccessStroyTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<KeyValueDto>>> Handle(GetSuccessStoryTypesQuery request, CancellationToken cancellationToken)
    {
        var successStoryTypes = await _repository.GetAllAsync();
        var successStoryTypesDto = successStoryTypes.Select(ta => new KeyValueDto(ta.Id, ta.Name)).ToList();
        return Result<List<KeyValueDto>>.Success(successStoryTypesDto);
    }
}

public class GetSynergyCompanyQueryHandler : IRequestHandler<GetSynergyCompaniesQuery, Result<List<GuidKeyValueDto>>>
{
    private readonly ISynergyCompanyRepository _repository;

    public GetSynergyCompanyQueryHandler(ISynergyCompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<GuidKeyValueDto>>> Handle(GetSynergyCompaniesQuery request, CancellationToken cancellationToken)
    {
        var synergyCompanies = await _repository.GetAllAsync();
        var synergyCompaniesDto = synergyCompanies
            .Where(c => c.IsActive)
            .Select(sc => new GuidKeyValueDto(sc.Id, sc.Name.Value))
            .ToList();

        return Result<List<GuidKeyValueDto>>.Success(synergyCompaniesDto);
    }
}

public class GetSectorsQueryHandler : IRequestHandler<GetSectorsQuery, Result<List<GuidKeyValueDto>>>
{
    private readonly ISynergyCompanyRepository _repository;

    public GetSectorsQueryHandler(ISynergyCompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<GuidKeyValueDto>>> Handle(GetSectorsQuery request, CancellationToken cancellationToken)
    {
        var synergyCompanies = await _repository.GetAllAsync(includes: c => c.Sectors);
        var sectors = synergyCompanies
                .SelectMany(sc => sc.Sectors)
                .DistinctBy(s => s.SectorId)
                .Select(cs => new GuidKeyValueDto(cs.SectorId, cs.SectorName))
                .ToList();
        return Result<List<GuidKeyValueDto>>.Success(sectors);
    }
}

public class GetCountriesCitiesQueryHandler : IRequestHandler<GetCountriesCitiesQuery, Result<List<CountryCityDto>>>
{
    private readonly ISynergyCompanyRepository _repository;

    public GetCountriesCitiesQueryHandler(ISynergyCompanyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<CountryCityDto>>> Handle(GetCountriesCitiesQuery request, CancellationToken cancellationToken)
    {
        var synergyCompanies = await _repository.GetAllAsync();
        var countriesCities = synergyCompanies
                .DistinctBy(sc => sc.HeadquarterCity)
                .Select(sc => new CountryCityDto(sc.HeadquarterCountry, sc.HeadquarterCity))
                .ToList();
        return Result<List<CountryCityDto>>.Success(countriesCities);
    }
}

public class GetCollaborationStatusFilterQueryHandler : IRequestHandler<GetCollaborationStatusFilterQuery, Result<List<KeyValueDto>>>
{
    public GetCollaborationStatusFilterQueryHandler()
    {
    }

    public async Task<Result<List<KeyValueDto>>> Handle(GetCollaborationStatusFilterQuery request, CancellationToken cancellationToken)
    {
        var successStoryStatuses = new List<KeyValueDto>
            {
                new KeyValueDto((byte)CollaborationStatusFilter.Active, CollaborationStatusFilter.Active.ToString() ),
                new KeyValueDto ((byte) CollaborationStatusFilter.Closed, CollaborationStatusFilter.Closed.ToString()),
                new KeyValueDto ((byte) CollaborationStatusFilter.Upcoming, CollaborationStatusFilter.Upcoming.ToString()),
            };
        return Result<List<KeyValueDto>>.Success(successStoryStatuses);
    }
}
