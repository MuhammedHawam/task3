using Microsoft.AspNetCore.Mvc;
using PartnersHub.Synergy.Apis.Controllers.Base;
using PartnersHub.Synergy.Application.SynergyCompany.Queries;
using PartnersHub.Synergy.Application.SynergyCompany.Commands;
using PartnersHub.Synergy.Domain.Common;
using MediatR;
using PartnersHub.Shared.Integration.DTOs;
using PartnersHub.Synergy.Application.Models;

namespace PartnersHub.Synergy.Apis.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SynergyCompaniesController : ApiBaseController<SynergyCompaniesController>
{
    private readonly IMediator _mediator;

    public SynergyCompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get registered synergy companies with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="searchTerm">Search term for company name/description</param>
    /// <param name="sectorIds">Comma-separated sector IDs to filter by</param>
    /// <param name="countries">Comma-separated countries to filter by</param>
    /// <param name="cities">Comma-separated cities to filter by</param>
    /// <param name="includeInactive">Include inactive companies (Admin only, default: false)</param>
    /// <returns>Paginated list of companies</returns>
    [HttpGet]
    public async Task<ActionResult<Result<PaginatedList<RegisteredCompanyCardDto>>>> GetRegisteredCompanies(
        [FromQuery] string? searchTerm = null,
        [FromQuery] List<Guid>? sectorIds = null,
        [FromQuery] List<string>? countries = null,
        [FromQuery] List<string>? cities = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 12)
    {
        var query = new GetRegisteredCompaniesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SectorIds = sectorIds,
            Countries = countries,
            Cities = cities,
            IncludeInactive = includeInactive
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<CompanyDetailsDto>>> GetCompanyDetails(Guid id)
    {
        var query = new GetCompanyDetailsQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Company with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Adds a company to Synergy by fetching it from external middleware service
    /// </summary>
    /// <param name="companyId">The company ID from the external system</param>
    /// <returns>The ID of the added company</returns>
    [HttpPost("add/{companyId}")]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<Guid>>> AddCompanyToSynergy(Guid companyId)
    {
        var command = new AddCompanyToSynergyCommand { CompanyId = companyId };
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found") == true)
                return NotFound(result);
            
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id}/activate")]
    public async Task<ActionResult<Result>> ActivateCompany(Guid id)
    {
        var command = new ActivateCompanyCommand { CompanyId = id };
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}/deactivate")]
    public async Task<ActionResult<Result>> DeactivateCompany(Guid id)
    {
        var command = new DeactivateCompanyCommand { CompanyId = id };
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return BadRequest(result);

        return Ok(result);
    }

    private List<string>? ParseCommaSeparated(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
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


  
    [HttpGet("integration-companies")]
    public async Task<ActionResult<Result<PaginatedList<CompanyIntegrationDto>>>> CompaniesList([FromQuery]CompaniesListQuery req)
    {
        var result = await _mediator.Send(req);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found") == true)
                return NotFound(result);

            return BadRequest(result);
        }

        return Ok(result);
    }


    [HttpGet("GetIntegrationCompany/{id}")]
    public async Task<ActionResult<Result<ExternalCompanyDto>>> GetIntegrationCompany(Guid id)
    {
        var query = new IntegrationCompanyDetailsQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Company with ID {id} not found" });

        return Ok(result);
    }


    [HttpGet("{companyId}/is-active")]
    public async Task<ActionResult<Result<bool>>> IsCompanyActive(Guid companyId)
    {
        var result = await _mediator.Send(
            new CheckCompanyActiveStatusQuery(companyId)
        );

        if (result.IsFailure)
            return NotFound(new { message = result.Error });

        return Ok(result);
    }

}
