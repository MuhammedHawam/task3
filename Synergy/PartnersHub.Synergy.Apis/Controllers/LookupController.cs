using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.Synergy.Apis.Controllers.Base;
using PartnersHub.Synergy.Application.Lookups.DTOs;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Apis.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LookupController : ApiBaseController<LookupController>
{
    [HttpGet("collaboration-requirements")]
    public async Task<ActionResult<Result<List<KeyValueDto>>>> GetCollaborationRequirements()
    {
        var query = new GetCollaborationRequirementsQuery();
        var result= await _mediator.Send(query);
        return Ok(result);

    }

    [HttpGet("expected-outcomes")]
    public async Task<ActionResult<Result<List<KeyValueDto>>>> GetExpectedOutcomes()
    {
        var query = new GetExpectedOutcomesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }


    [HttpGet("opportunity-types")]
    public async Task<ActionResult<Result<List<KeyValueDto>>>> GetOpportunityTypes()
    {
        var query = new GetOpportunityTypesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("thematic-areas")]
    public async Task<ActionResult<Result<List<KeyValueDto>>>> GetThematicAreas()
    {
        var query = new GetThematicAreasQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpGet("successs-story-statuses")]
    public async Task<ActionResult<Result<List<KeyValueDto>>>> GetSuccessStoryStatuses()
    {
        var query = new GetSuccessStoryStatusesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpGet("successs-story-collaboration-statuses")]
    public async Task<ActionResult<Result<List<KeyValueDto>>>> GetSuccessStoryCollaborationStatuses()
    {
        var query = new SuccessStoryCollaborationStatusesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpGet("successs-story-types")]
    public async Task<ActionResult<Result<List<KeyValueDto>>>> GetSuccessStoryTypes()
    {
        var query = new GetSuccessStoryTypesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpGet("synergy-companies")]
    public async Task<ActionResult<Result<List<GuidKeyValueDto>>>> GetSynergyCompanies()
    {
        var query = new GetSynergyCompaniesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpGet("sectors")]
    public async Task<ActionResult<Result<List<GuidKeyValueDto>>>> GetSectors()
    {
        var query = new GetSectorsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpGet("countries-and-cities")]
    public async Task<ActionResult<Result<List<CountryCityDto>>>> GetCountriesAndCities()
    {
        var query = new GetCountriesCitiesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpGet("collaboration-status-filters")]
    public async Task<ActionResult<Result<List<KeyValueDto>>>> GetCollaborationStatusFilters()
    {
        var query = new GetCollaborationStatusFilterQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

