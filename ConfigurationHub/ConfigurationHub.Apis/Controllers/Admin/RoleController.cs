using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
namespace PartnersHub.ConfigurationHub.Apis.Controllers.Admin;

[ApiController]
[Route("api/admin/roles")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<Role>>> GetAllRoles(int pageNumber = 1, int pageSize = 10)
    {
        var roles = await _roleService.GetAllRolesAsync(pageSize, pageNumber);
        return Ok(roles);
    }

    [HttpGet("{moduleId}/lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetAllRolesLookup(Guid moduleId)
    {
        var roles = await _roleService.GetAllRolesLookupByModuleAsync(moduleId);
        return Ok(roles);
    }

    [HttpGet("{roleId}")]
    public async Task<ActionResult<Role>> GetRoleById(Guid roleId)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId);
        return role == null ? NotFound(new { message = "Role not found" }) : Ok(role);
    }

    [HttpGet("module/{moduleId}")]
    public async Task<ActionResult<IEnumerable<Role>>> GetRolesByModule(Guid moduleId)
    {
        var roles = await _roleService.GetRolesByModuleAsync(moduleId);
        return Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<Role>> CreateRole([FromBody] CreateRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Role name is required" });

        var role = await _roleService.CreateRoleAsync(request.Name, request.Description, request.ModuleId);
        return CreatedAtAction(nameof(GetRoleById), new { roleId = role.Id }, role);
    }

    [HttpPut("{roleId}")]
    public async Task<IActionResult> UpdateRole(Guid roleId, [FromBody] UpdateRoleRequest request)
    {
        var success = await _roleService.UpdateRoleAsync(roleId, request.Name, request.Description);
        return success ? Ok(new { message = "Role updated successfully" }) : NotFound(new { message = "Role not found" });
    }

    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRole(Guid roleId)
    {
        var success = await _roleService.DeleteRoleAsync(roleId);
        return success ? Ok(new { message = "Role deleted successfully" }) : NotFound(new { message = "Role not found" });
    }

    [HttpPost("{roleId}/permissions")]
    public async Task<IActionResult> AssignPermissionToRole(Guid roleId, [FromBody] AddRolePermissionsRequest request)
    {
        var success = await _roleService.AssignPermissionToRoleAsync(roleId, request.PermissionsIds);
        return success ? Ok(new { message = "Permission assigned successfully" }) : BadRequest(new { message = "Permission already assigned" });
    }


    [HttpDelete("{roleId}/permissions/{permissionId}")]
    public async Task<IActionResult> RemovePermissionFromRole(Guid roleId, Guid permissionId)
    {
        var success = await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
        return success ? Ok(new { message = "Permission removed successfully" }) : NotFound(new { message = "Permission not found" });
    }

    [HttpGet("{roleId}/permissions")]
    public async Task<ActionResult<IEnumerable<Permission>>> GetRolePermissions(Guid roleId)
    {
        var permissions = await _roleService.GetRolePermissionsAsync(roleId);
        return Ok(permissions);
    }

    [HttpGet("{roleId}/permissions/lookup")]
    public async Task<ActionResult<IEnumerable<LookupDto>>> GetRolePermissionslookup(Guid roleId)
    {
        var permissions = await _roleService.GetRolePermissionsLookupAsync(roleId);
        return Ok(permissions);
    }

    [HttpPost("users/{userId}")]
    public async Task<IActionResult> AssignRoleToUser(string userId, [FromBody] AssignRoleRequest request)
    {
        var assignedBy = User.Identity?.Name ?? "System";
        var success = await _roleService.AssignRoleToUserAsync(userId,request.userName,request.useremail, request.RoleId, request.ModuleId, assignedBy);

        return success ? Ok(new { message = "Role assigned successfully" }) : BadRequest(new { message = "User already has this role" });
    }

    [HttpDelete("users/{userId}/{roleId}/modules/{moduleId}")]
    public async Task<IActionResult> RemoveRoleFromUser(string userId, Guid roleId, Guid moduleId)
    {
        var success = await _roleService.RemoveRoleFromUserAsync(userId, roleId, moduleId);
        return success ? Ok(new { message = "Role removed successfully" }) : NotFound(new { message = "Role assignment not found" });
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<IEnumerable<UserRoleResponse>>> GetUserRoles(string userId)
    {
        var cleanUserId = userId.Contains("@") ? userId.Split('@')[0] : userId;
        var userRoles = await _roleService.GetUserRoleDetailsAsync(cleanUserId);

        var response = userRoles.Select(ur => new UserRoleResponse
        {
            UserId = ur.UserId,
            Role = ur.Role,
            Module = ur.Module,
            AssignedBy = ur.AssignedBy,
            AssignedAt = ur.AssignedAt
        });

        return Ok(response);
    }

    [HttpGet("users/{userId}/permissions")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserPermissions(string userId)
    {
        var cleanUserId = userId.Contains("@") ? userId.Split('@')[0] : userId;
        cleanUserId = cleanUserId.Contains("\\") ? cleanUserId.Split('\\')[0] : cleanUserId;
        var permissions = await _roleService.GetUserPermissionsAsync(cleanUserId);
        return Ok(permissions);
    }

    [HttpPut("{roleId}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(Guid roleId, [FromBody] UpdateRolePermissionsRequest request)
    {
        var success = await _roleService.UpdateRolePermissionsAsync(roleId, request.PermissionsIds);
        return success ? Ok(new { message = "Role updated successfully" }) : NotFound(new { message = "Role not found" });
    }

    [HttpGet("Moduleadmins")]
    public async Task<ActionResult<IEnumerable<string>>> GetPaginatedAdmin(string? searchTerm = null, string? sortBy = null, int pageNumber = 1, int pageSize = 10)
    {
        var adminUsers = await _roleService.GetPaginatedAdminAsync(pageNumber, pageSize, searchTerm, sortBy);
        return Ok(adminUsers);
    }
}

public record CreateRoleRequest(string Name, string Description, Guid? ModuleId);
public record UpdateRoleRequest(string Name, string Description);
public record AddRolePermissionsRequest(List<Guid> PermissionsIds);
public record UpdateRolePermissionsRequest(List<Guid> PermissionsIds);
public record AssignRoleRequest(Guid RoleId, Guid ModuleId,string useremail,string userName);
public record UserRoleResponse
{
    public string UserId { get; set; } = string.Empty;
    public Role Role { get; set; } = default!;
    public Module Module { get; set; } = default!;
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
