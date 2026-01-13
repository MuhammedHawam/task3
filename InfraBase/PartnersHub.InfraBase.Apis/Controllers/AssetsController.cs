using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Models;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITokenService _tokenService;

    public AssetsController(IMediator mediator, ITokenService tokenService)
    {
        _mediator = mediator;
        _tokenService = tokenService;
    }

    // ========== CRUD Operations ==========

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateAsset([FromBody] CreateAssetCommand command)
    {
        var assetId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAssetById), new { id = assetId }, assetId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssetDto>> GetAssetById(Guid id)
    {
        var asset = await _mediator.Send(new GetAssetByIdQuery(id));
        if (asset == null)
        {
            return NotFound();
        }

        return Ok(asset);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<bool>> UpdateAsset(Guid id, 
        [FromBody] UpdateAssetCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Asset ID mismatch");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsset(Guid id)
    {
        var result = await _mediator.Send(new DeleteAssetCommand(id));
        return Ok(result);
    }

    // ========== List & Search ==========

    [HttpGet]
    public async Task<ActionResult<PaginatedList<AssetListDto>>> GetAssets(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] AssetStatuses? status = null, 
        [FromQuery] Guid? companyId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        // Non-admins should always be scoped to their token company.
        // Infrabase admins can view all companies unless a companyId filter is explicitly provided.
        Guid? effectiveCompanyId;
        if (_tokenService.IsInfrabaseAdmin())
        {
            effectiveCompanyId = companyId;
        }
        else
        {
            var tokenCompanyId = _tokenService.GetCompanyId();
            effectiveCompanyId = companyId ?? tokenCompanyId;
        }
        
        var query = new GetAssetListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Status = status,
            CompanyId = effectiveCompanyId,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<List<AssetListDto>>> GetByStatus(
        AssetStatuses status,
        [FromQuery] Guid? companyId = null)
    {
        // Apply company ID filter from token if not provided
        var tokenCompanyId = _tokenService.GetCompanyId();
        var effectiveCompanyId = companyId ?? tokenCompanyId;
        
        var assets = await _mediator.Send(new GetAssetsByStatusQuery(status, effectiveCompanyId));
        return Ok(assets);
    }

    // ========== Asset Workflow Actions ==========

    /// <summary>
    /// Save asset as draft
    /// User Story: "Save as draft - The asset is saved with status 'draft'"
    /// </summary>
    [HttpPost("{id}/draft")]
    public async Task<ActionResult<bool>> SaveAsDraft(Guid id, [FromBody] SaveAssetAsDraftCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Asset ID mismatch");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Submit asset for approval
    /// User Story: "Submit - Give the asset a code (prefix +Max code +1)"
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<ActionResult<bool>> Submit(Guid id, [FromBody] SubmitAssetCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Asset ID mismatch");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ========== PC Admin Actions ==========

    /// <summary>
    /// PC Admin accepts a submitted asset
    /// User Story: "As PC admin, I want to accept \ Reject submitted asset from contributor"
    /// </summary>
    [HttpPost("{id}/pc-admin/accept")]
    public async Task<ActionResult<bool>> AcceptByPcAdmin(Guid id, 
        [FromBody] AcceptAssetByPcAdminCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Asset ID mismatch");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// PC Admin rejects a submitted asset
    /// User Story: "As PC admin, I want to accept \ Reject submitted asset from contributor"
    /// </summary>
    [HttpPost("{id}/pc-admin/reject")]
    public async Task<ActionResult<bool>> RejectByPcAdmin(Guid id, 
        [FromBody] RejectAssetByPcAdminCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Asset ID mismatch");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ========== Infrabase Admin Actions ==========

    /// <summary>
    /// Infrabase Admin accepts/checks a PC admin approved asset
    /// User Story: "As an Infrabase admin, I want to accept \ Reject asset approved by PC admin"
    /// </summary>
    [HttpPost("{id}/infrabase-admin/accept")]
    public async Task<ActionResult<bool>> AcceptByInfrabaseAdmin(Guid id, 
        [FromBody] CheckAssetByInfrabaseAdminCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Asset ID mismatch");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Infrabase Admin rejects/returns for correction a PC admin approved asset
    /// User Story: "As an Infrabase admin, I want to accept \ Reject asset approved by PC admin"
    /// </summary>
    [HttpPost("{id}/infrabase-admin/reject")]
    public async Task<ActionResult<bool>> RejectByInfrabaseAdmin(Guid id, 
        [FromBody] ReturnAssetForCorrectionCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Asset ID mismatch");
        }

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // ========== Asset Attachments ==========

    [HttpPost("{id}/attachments")]
    public async Task<ActionResult<Guid>> AddAttachment(Guid id, 
        [FromBody] AddAssetAttachmentCommand command)
    {
        if (id != command.AssetId)
        {
            return BadRequest("Asset ID mismatch");
        }

        var attachmentId = await _mediator.Send(command);
        return Ok(attachmentId);
    }

    /// <summary>
    /// Get all attachments for an asset
    /// User Story: "As PC Contributor, PC admin and Infrabase admin, I want to upload asset attachments"
    /// </summary>
    [HttpGet("{id}/attachments")]
    public async Task<ActionResult<List<AssetAttachmentDto>>> GetAttachments(Guid id)
    {
        var query = new GetAssetAttachmentsQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpDelete("{id}/attachments/{attachmentId}")]
    public async Task<ActionResult<bool>> RemoveAttachment(Guid id, Guid attachmentId, 
        [FromQuery] Guid userId)
    {
        var result = await _mediator.Send(new RemoveAssetAttachmentCommand(id, attachmentId));
        return Ok(result);
    }

    // ========== Asset History ==========

    [HttpGet("{id}/history")]
    public async Task<ActionResult<List<AssetHistoryDto>>> GetAssetHistory(Guid id)
    {
        var query = new GetAssetHistoryQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ========== Utility Endpoints ==========

    [HttpGet("next-code")]
    public async Task<ActionResult<string>> GetNextAssetCode()
    {
        var query = new GetNextAssetCodeQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
