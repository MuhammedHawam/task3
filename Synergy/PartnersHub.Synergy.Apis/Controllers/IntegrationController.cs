using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.Synergy.Application.Interfaces.Integration;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IntegrationController : ControllerBase
{
    private readonly IUserProfileDataIntegrationService _userDataIntegrationService;
    private readonly IMiddlewareIntegrationService _middlewareIntegrationService;
    public IntegrationController(
        IUserProfileDataIntegrationService userDataIntegrationService,
        IMiddlewareIntegrationService middlewareIntegrationService)
    {
        _userDataIntegrationService = userDataIntegrationService;
        _middlewareIntegrationService = middlewareIntegrationService;
    }
    [HttpGet("get-user-profile")]
    public async Task<ActionResult<Result<UserProfileDataDto>>> GetSynergyCompanies()
    {
        var response = await _userDataIntegrationService.GetUserProfileData();
        return Result<UserProfileDataDto>.Success(response);
    }

    [HttpGet("attachments/download")]
    public async Task<ActionResult<DocumentInfo>> DownloadAttachment(
        [FromQuery] string sourceFilePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            return BadRequest("Document path is required");
        }

        var document = await _middlewareIntegrationService.DownloadDocumentAsync(sourceFilePath, cancellationToken);
        if (document == null)
        {
            return NotFound("Document not found");
        }

        return Ok(document);
    }
}