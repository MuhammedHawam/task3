using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.Synergy.Application.Dashboard.DTOs;
using PartnersHub.Synergy.Application.Dashboard.Queries;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Apis.Controllers;

/// <summary>
/// Dashboard API for Synergy PC Representatives
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get complete dashboard home page (KPIs + recent opportunities/stories/companies)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<DashboardHomeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<DashboardHomeDto>>> GetDashboardHome(
        [FromQuery] Guid companyId,
        [FromQuery] int? year = null)
    {
        // TODO: Get companyId from authenticated user claims
        //compare it against the current company id
        if (companyId == Guid.Empty)
            return BadRequest(Result<DashboardHomeDto>.Failure("Company ID is required"));

        var query = new GetDashboardHomeQuery
        {
            CompanyId = companyId,
            Year = year
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpGet("admin")]
    [ProducesResponseType(typeof(Result<AdminDashboardKPIsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<AdminDashboardKPIsDto>>> GetAdminDashboard(
    [FromQuery] int? year = null)
    {

        var query = new GetAdminDashboardQuery
        {
            Year = year
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
    /// <summary>
    /// Get user's opportunity submissions with filtering, search, and pagination
    /// Supports multiple statuses (comma-separated): pending, published, returned/rejected, draft
    /// </summary>
    [HttpGet("my-opportunities")]
    [ProducesResponseType(typeof(Result<PaginatedList<UserOpportunitySubmissionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<PaginatedList<UserOpportunitySubmissionDto>>>> GetMyOpportunities(
        [FromQuery] Guid companyId,
        [FromQuery] string? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // TODO: Get companyId from authenticated user claims
        if (companyId == Guid.Empty)
            return BadRequest(Result<PaginatedList<UserOpportunitySubmissionDto>>.Failure("Company ID is required"));

        var query = new GetUserSubmissionsQuery
        {
            CompanyId = companyId,
            Status = status,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get user's success story submissions with filtering, search, and pagination
    /// Supports multiple statuses (comma-separated): pending, published, returned/rejected, draft
    /// </summary>
    [HttpGet("my-success-stories")]
    [ProducesResponseType(typeof(Result<PaginatedList<UserSuccessStorySubmissionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<PaginatedList<UserSuccessStorySubmissionDto>>>> GetMySuccessStories(
        [FromQuery] Guid companyId,
        [FromQuery] string? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // TODO: Get companyId from authenticated user claims
        if (companyId == Guid.Empty)
            return BadRequest(Result<PaginatedList<UserSuccessStorySubmissionDto>>.Failure("Company ID is required"));

        var query = new GetUserSuccessStoriesQuery
        {
            CompanyId = companyId,
            Status = status,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
