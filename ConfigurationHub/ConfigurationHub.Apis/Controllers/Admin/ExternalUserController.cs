using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Application.Common.Models;

namespace PartnersHub.ConfigurationHub.Apis.Controllers.Admin;

[ApiController]
[Route("api/admin/external-users")]
[Authorize]
public class ExternalUserController : ControllerBase
{
    private readonly IScimUserService _scimUserService;
    private readonly ILogger<ExternalUserController> _logger;

    public ExternalUserController(IScimUserService scimUserService, ILogger<ExternalUserController> logger)
    {
        _scimUserService = scimUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get external users from CIAM (for partner companies)
    /// </summary>
    /// <returns>List of external CIAM users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SimpleUser>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<SimpleUser>>> GetExternalUsers()
    {
        try
        {
            var users = await _scimUserService.GetUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving external users from CIAM");
            return StatusCode(500, new { message = "An error occurred while retrieving external users" });
        }
    }
}
