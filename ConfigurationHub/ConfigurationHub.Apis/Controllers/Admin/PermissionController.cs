using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Apis.Controllers.Admin;

[ApiController]
[Route("api/admin/permissions")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Get all permissions
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Permission>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Permission>>> GetAllPermissions()
    {
        var permissions = await _permissionService.GetAllPermissionsAsync();
        return Ok(permissions);
    }


    [HttpGet("lookup")]
    [ProducesResponseType(typeof(IEnumerable<LookupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LookupDto>>> GetAllPermissionslookup()
    {
        var permissions = await _permissionService.GetAllPermissionsLookupAsync();
        return Ok(permissions);
    }

    /// <summary>
    /// Get permission by ID
    /// </summary>
    [HttpGet("{permissionId}")]
    [ProducesResponseType(typeof(Permission), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Permission>> GetPermissionById(Guid permissionId)
    {
        var permission = await _permissionService.GetPermissionByIdAsync(permissionId);
        return permission == null ? NotFound() : Ok(permission);
    }



    [HttpGet("ModuleAssignedPermission")]
    [ProducesResponseType(typeof(PaginatedList<ModulePermissionsRolesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedList<ModulePermissionsRolesDto>>> GetAllAssignedPermissionsRole(int pageSize =10, int pageIndex= 1, string searchparam =null, string? sortBy = null)
    {
        var permission = await _permissionService.GetAllAssignedPermissionsRole(pageSize, pageIndex, searchparam, sortBy);
        return permission == null ? NotFound() : Ok(permission);
    }

    /// <summary>
    /// Get permissions by module
    /// </summary>
    [HttpGet("module/{moduleId}")]
    [ProducesResponseType(typeof(IEnumerable<Permission>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Permission>>> GetPermissionsByModule(Guid moduleId)
    {
        var permissions = await _permissionService.GetPermissionsByModuleAsync(moduleId);
        return Ok(permissions);
    }
}
