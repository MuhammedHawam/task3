using MediatR;
using PartnersHub.Synergy.Application.Lookups.DTOs;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Common;

public class GetCollaborationRequirementsQuery : IRequest<Result<List<KeyValueDto>>>
{
}

public class GetExpectedOutcomesQuery : IRequest<Result<List<KeyValueDto>>>
{
}



public class GetOpportunityTypesQuery : IRequest<Result<List<KeyValueDto>>>
{
}

public class GetThematicAreasQuery : IRequest<Result<List<KeyValueDto>>>
{
}
public class GetSuccessStoryStatusesQuery : IRequest<Result<List<KeyValueDto>>>
{

}
public class SuccessStoryCollaborationStatusesQuery : IRequest<Result<List<KeyValueDto>>>
{

}
public class GetSuccessStoryStoryTypesQuery : IRequest<Result<List<KeyValueDto>>>
{

}
public class GetSuccessStoryTypesQuery : IRequest<Result<List<KeyValueDto>>>
{

}
public class GetSynergyCompaniesQuery : IRequest<Result<List<GuidKeyValueDto>>>
{

}
public class GetSectorsQuery : IRequest<Result<List<GuidKeyValueDto>>>
{

}
public class GetCountriesCitiesQuery : IRequest<Result<List<CountryCityDto>>>
{

}
public class GetCollaborationStatusFilterQuery : IRequest<Result<List<KeyValueDto>>>
{

}