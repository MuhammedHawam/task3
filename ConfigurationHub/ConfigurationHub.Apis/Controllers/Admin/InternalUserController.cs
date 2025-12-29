using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Application.Common.Models;

namespace PartnersHub.ConfigurationHub.Apis.Controllers.Admin;

[ApiController]
[Route("api/admin/internal-users")]
[Authorize]
public class InternalUserController : ControllerBase
{
    private readonly ILdapUserService _ldapUserService;

    public InternalUserController(ILdapUserService ldapUserService)
    {
        _ldapUserService = ldapUserService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<LdapUser>>> SearchInternalUsers(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            return BadRequest(new { message = "Search term must be at least 2 characters" });

        var users = await _ldapUserService.SearchUsersAsync(searchTerm, pageNumber, pageSize);
        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<LdapUser>> GetInternalUserByUsername(string username)
    {
        var user = await _ldapUserService.GetUserByUsernameAsync(username);

        if (user == null)
            return NotFound(new { message = $"User '{username}' not found in Active Directory" });

        return Ok(user);
    }


    [HttpGet]
    public async Task<ActionResult<List<LdapUser>>> GetInternalUsersByUsername(string? username = null)
    {
        var user = await _ldapUserService.GetUsersByUsernameAsync(username);

        if (user == null)
            return NotFound(new { message = $"User '{username}' not found in Active Directory" });

        return Ok(user);
    }
}
