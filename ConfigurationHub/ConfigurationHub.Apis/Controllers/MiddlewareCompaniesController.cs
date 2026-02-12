using MediatR;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Application.Middleware.DTOs;
using PartnersHub.ConfigurationHub.Application.Middleware.Queries;

namespace PartnersHub.ConfigurationHub.Apis.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MiddlewareCompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MiddlewareCompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("companies/search")]
    [ProducesResponseType(typeof(Result<PaginatedList<MiddlewareCompanyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<PaginatedList<MiddlewareCompanyDto>>>> SearchCompanies(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] string? sectorIds = null,
        [FromQuery] string? cityIds = null)
    {
        var query = new GetMiddlewareCompaniesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchText = searchText,
            SectorIds = ParseGuids(sectorIds),
            CityIds = ParseGuids(cityIds)
        };

        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("companies/{companyId}")]
    [ProducesResponseType(typeof(Result<MiddlewareCompanyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<MiddlewareCompanyDto>>> GetCompanyById(Guid companyId)
    {
        if (companyId == Guid.Empty)
        {
            return BadRequest(Result<MiddlewareCompanyDto>.Failure("Company ID is required"));
        }

        var query = new GetMiddlewareCompanyByIdQuery(companyId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("not found"))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("companiesBySector/{sectorId}")]
    [ProducesResponseType(typeof(Result<MiddlewareCompanyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<MiddlewareCompanyDto>>> GetCompanyBySectorId(Guid sectorId)
    {
        if (sectorId == Guid.Empty)
        {
            return BadRequest(Result<MiddlewareCompanyDto>.Failure("Sector ID is required"));
        }

        var query = new GetMiddlewareCompanyBySectorIdQuery(sectorId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            if (result.Error!.Contains("not found"))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("sectors")]
    [ProducesResponseType(typeof(Result<List<MiddlewareSectorDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<List<MiddlewareSectorDto>>>> GetSectors()
    {
        var query = new GetMiddlewareSectorsQuery();
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
    
    [HttpGet("companies")]
    [ProducesResponseType(typeof(Result<List<MiddlewareCompanyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Result<List<MiddlewareCompanyDto>>>> GetAllCompanies()
    {
        var query = new GetAllMiddlewareCompaniesQuery();
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
    private List<Guid>? ParseGuids(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Guid.TryParse(s, out var guid) ? guid : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();
    }
}
