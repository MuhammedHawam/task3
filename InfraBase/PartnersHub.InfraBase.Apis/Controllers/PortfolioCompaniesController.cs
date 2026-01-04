using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.InfraBase.Application.Common.DTOs;
using PartnersHub.InfraBase.Application.Common.Interfaces;

namespace PartnersHub.InfraBase.Apis.Controllers;

/// <summary>
/// Portfolio companies lookup used by InfraBase Admin flows (create assets on behalf of a PC company).
/// </summary>
[ApiController]
[Route("api/portfolio-companies")]
[Authorize]
public class PortfolioCompaniesController : ControllerBase
{
    private readonly IMiddlewareIntegrationService _middlewareIntegrationService;
    private readonly ITokenService _tokenService;

    public PortfolioCompaniesController(
        IMiddlewareIntegrationService middlewareIntegrationService,
        ITokenService tokenService)
    {
        _middlewareIntegrationService = middlewareIntegrationService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Get portfolio company details (name, sector, representative) for the "Infrabase Owner" section.
    /// </summary>
    [HttpGet("{companyId:guid}")]
    public async Task<ActionResult<MiddlewareCompany>> GetById(Guid companyId, CancellationToken cancellationToken)
    {
        if (!_tokenService.IsInfrabaseAdmin())
        {
            return Forbid();
        }

        var company = await _middlewareIntegrationService.GetCompanyByIdAsync(companyId);
        if (company == null)
        {
            return NotFound();
        }

        return Ok(company);
    }
}

