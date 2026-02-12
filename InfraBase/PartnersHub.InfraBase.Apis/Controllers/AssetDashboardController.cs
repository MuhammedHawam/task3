using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Apis.Controllers;

/// <summary>
/// Dashboard endpoints for InfraBase - provides role-specific dashboard data
/// </summary>
[ApiController]
[Route("api/assets/dashboard")]
[Authorize]
public class AssetDashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITokenService _tokenService;

    public AssetDashboardController(IMediator mediator, ITokenService tokenService)
    {
        _mediator = mediator;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Get PC Contributor dashboard data
    /// User Story: "As PC contributor, I want to view home page – landing page"
    /// Automatically filters by company ID from token
    /// </summary>
    /// <param name="userId">The contributor's user ID</param>
    /// <param name="searchTerm">Search in asset name (max 500 characters)</param>
    /// <param name="statusFilter">Filter by specific status</param>
    /// <param name="pageNumber">Page number for asset list</param>
    /// <param name="pageSize">Page size for asset list</param>
    /// <returns>Dashboard with status cards and filtered/searched asset list</returns>
    [HttpGet("contributor")]
    [ProducesResponseType(typeof(ContributorDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContributorDashboardDto>> GetContributorDashboard(
        [FromQuery] Guid userId,
        [FromQuery] string? searchTerm = null,
        [FromQuery] AssetStatuses? statusFilter = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new { message = "User ID is required" });
        }

        if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 500)
        {
            return BadRequest(new { message = "Search term cannot exceed 500 characters" });
        }

        // Apply company ID filter from token for contributors
        var tokenCompanyId = _tokenService.GetCompanyId();
        if (!tokenCompanyId.HasValue)
        {
            return BadRequest(new { message = "Company ID not found in token. Contributors must be associated with a company." });
        }

        var query = new GetContributorDashboardQuery(
            userId, 
            pageNumber, 
            pageSize, 
            searchTerm, 
            statusFilter);
            
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get PC Admin dashboard data for their own assets
    /// User Story: "As PC admin, I want to view home page – landing page"
    /// Automatically filters by company ID from token if available
    /// </summary>
    /// <param name="userId">The PC admin's user ID</param>
    /// <param name="searchTerm">Search in asset name (max 500 characters)</param>
    /// <param name="statusFilter">Filter by specific status</param>
    /// <param name="pageNumber">Page number for asset list</param>
    /// <param name="pageSize">Page size for asset list</param>
    /// <returns>Dashboard with status cards and asset list for PC Admin's own assets</returns>
    [HttpGet("pc-admin")]
    [ProducesResponseType(typeof(PcAdminDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PcAdminDashboardDto>> GetPcAdminDashboard(
        [FromQuery] Guid userId,
        [FromQuery] string? searchTerm = null,
        [FromQuery] AssetStatuses? statusFilter = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new { message = "User ID is required" });
        }

        if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 500)
        {
            return BadRequest(new { message = "Search term cannot exceed 500 characters" });
        }

        // Apply company ID filter from token for PC Admins if available
        var tokenCompanyId = _tokenService.GetCompanyId();
        if (!tokenCompanyId.HasValue)
        {
            return BadRequest(new { message = "Company ID not found in token. PC Admins must be associated with a company." });
        }

        var query = new GetPcAdminDashboardQuery(
            userId, 
            pageNumber, 
            pageSize, 
            searchTerm, 
            statusFilter);
            
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get PC Admin team assets dashboard data
    /// User Story: "As PC admin, I want to view team assets"
    /// Shows assets submitted by contributors in the same company
    /// Automatically uses company ID from token
    /// </summary>
    /// <param name="userId">PC Admin user ID (to exclude their own assets)</param>
    /// <param name="searchTerm">Search in asset name (max 500 characters)</param>
    /// <param name="statusFilter">Filter by specific status</param>
    /// <param name="pageNumber">Page number for asset list</param>
    /// <param name="pageSize">Page size for asset list</param>
    /// <returns>Dashboard with status cards and team asset list</returns>
    [HttpGet("pc-admin/team-assets")]
    [ProducesResponseType(typeof(TeamAssetsDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamAssetsDashboardDto>> GetTeamAssetsDashboard(
        [FromQuery] Guid userId,
        [FromQuery] string? searchTerm = null,
        [FromQuery] AssetStatuses? statusFilter = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new { message = "User ID is required" });
        }

        if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 500)
        {
            return BadRequest(new { message = "Search term cannot exceed 500 characters" });
        }

        // Apply company ID from token - required for team assets
        var tokenCompanyId = _tokenService.GetCompanyId();
        if (!tokenCompanyId.HasValue)
        {
            return BadRequest(new { message = "Company ID not found in token. Cannot retrieve team assets without company context." });
        }

        var query = new GetTeamAssetsDashboardQuery(
            tokenCompanyId.Value,  // Use company ID from token
            userId, 
            pageNumber, 
            pageSize, 
            searchTerm, 
            statusFilter);
            
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get Infrabase Admin dashboard data
    /// User Story: "As infrabase admin, I want to view home page – landing page"
    /// Shows all assets across all companies (no company filtering)
    /// </summary>
    /// <param name="searchTerm">Search in asset name (max 500 characters)</param>
    /// <param name="statusFilter">Filter by specific status</param>
    /// <param name="pageNumber">Page number for asset list</param>
    /// <param name="pageSize">Page size for asset list</param>
    /// <returns>Dashboard with status cards and all assets</returns>
    [HttpGet("infrabase-admin")]
    [ProducesResponseType(typeof(InfrabaseAdminDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InfrabaseAdminDashboardDto>> GetInfrabaseAdminDashboard(
        [FromQuery] string? searchTerm = null,
        [FromQuery] AssetStatuses? statusFilter = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 500)
        {
            return BadRequest(new { message = "Search term cannot exceed 500 characters" });
        }

        // Apply company ID from token
        var tokenCompanyId = _tokenService.GetCompanyId();

        // NO company filtering for Infrabase Admin - they see all assets
        var query = new GetInfrabaseAdminDashboardQuery(
            pageNumber, 
            pageSize, 
            searchTerm, 
            statusFilter, tokenCompanyId);
            
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
