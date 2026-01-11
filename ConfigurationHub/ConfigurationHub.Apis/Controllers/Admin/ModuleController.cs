using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Apis.Controllers.Admin;

[ApiController]
[Route("api/admin/modules")]
[Authorize]
public class ModuleController : ControllerBase
{
    private readonly IModuleService _moduleService;

    public ModuleController(IModuleService moduleService)
    {
        _moduleService = moduleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Module>>> GetAllModules()
    {
        var modules = await _moduleService.GetAllModulesAsync();
        return Ok(modules);
    }

    [HttpGet("Lookup")]
    public async Task<ActionResult<IEnumerable<LookupDto>>> GetLookup()
    {
        var modules = await _moduleService.GetLookupAsync();
        return Ok(modules);
    }

    [HttpGet("{moduleId}")]
    public async Task<ActionResult<Module>> GetModuleById(Guid moduleId)
    {
        var module = await _moduleService.GetModuleByIdAsync(moduleId);
        return module == null ? NotFound() : Ok(module);
    }
}
