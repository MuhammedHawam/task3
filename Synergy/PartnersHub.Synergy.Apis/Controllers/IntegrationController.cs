using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public IntegrationController(IUserProfileDataIntegrationService userDataIntegrationService) 
    {
        _userDataIntegrationService = userDataIntegrationService;
    } 
    [HttpGet("get-user-profile")]
    public async Task<ActionResult<Result<UserProfileDataDto>>> GetSynergyCompanies()
    {
        var response = await _userDataIntegrationService.GetUserProfileData();
        return Result<UserProfileDataDto>.Success(response);
    }
}