using MediatR;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.InnovationHub.Apis.Controllers.Base;
using PartnersHub.InnovationHub.Application.Campaign.Commands;
using PartnersHub.InnovationHub.Application.Campaign.Queries;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest;
using PartnersHub.InnovationHub.Application.Challenge.Queries.ChallengeRequest;
using PartnersHub.InnovationHub.Application.Common.Models;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System.Globalization;


namespace PartnersHub.InnovationHub.Apis.Controllers.CampaignRequest;

[ApiController]
[Route("api/v1/[controller]")]
public class CampaignRequestController : ApiBaseController<CampaignRequestController>
{
    private readonly IMediator _mediator;
    public CampaignRequestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("Create")]
    public Task<ActionResult<ApiResponse>> Create([FromBody] CreateCampaignRequestCommand command, CancellationToken cancellationToken)
         => Execute(command, cancellationToken);

    [HttpGet("ActiveCampaignList")]
    [ProducesResponseType(typeof(Result<PaginatedList<ActiveCampaignCardDTO>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<PaginatedList<ActiveCampaignCardDTO>>>> ActiveCampaignList(
                                      [FromQuery] List<CampaignType>? CampaignType,
                                      [FromQuery] List<CampaignStatus>? CampaignStatus,
                                      [FromQuery] DateTime? LaunchDate,
                                      [FromQuery] Guid? UserId,
                                      [FromQuery] string? SearchTerm,
                                      [FromQuery] bool? IsMyCampaign,
                                      [FromQuery] bool? IsAdmin,
                                      [FromQuery] bool? IsPending,
                                      [FromQuery] List<RequestState>? StatusList,
                                      [FromQuery] int PageSize = 8,
                                      [FromQuery] int PageNumber = 1)

    {
        var query = new ActiveCampaignListQuery
        {
            PageNumber = PageNumber,
            PageSize = PageSize,
            SearchTerm = SearchTerm,
            CampaignType = CampaignType,
            CampaignStatus = CampaignStatus,
            LaunchDate = LaunchDate,
            UserId = UserId,
            IsMyCampaign = IsMyCampaign,
            IsAdmin = IsAdmin,
            IsPending = IsPending,
            StatusList = StatusList
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{requestId}/details")]
    public Task<ActionResult<ApiResponse>> Details(Guid requestId, CancellationToken cancellationToken)
               => Execute(new CampaignDetailsQuery { CampaignId = requestId }, cancellationToken);

    [HttpPost("CreateCampaign")]
    public Task<ActionResult<ApiResponse>> CreateCampaign([FromBody] CreateCampaignCommand command, CancellationToken cancellationToken)
         => Execute(command, cancellationToken);

    [HttpPut("ConvertToDraft")]
    public Task<ActionResult<ApiResponse>> ConvertToDraft([FromBody] ConvertRequestToCampaignDraftCommand command, CancellationToken cancellationToken)
     => Execute(command, cancellationToken);

    [HttpGet("SponsorList")]
    public Task<ActionResult<ApiResponse>> SponsorList(CancellationToken cancellationToken)
               => Execute(new SponsorListQuery { }, cancellationToken);

    [HttpGet("EvaluatorList")]
    public Task<ActionResult<ApiResponse>> EvaluatorList(CancellationToken cancellationToken)
              => Execute(new EvaluatorListQuery { }, cancellationToken);



[HttpPost("{id}/attachments")]
[ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
public async Task<ActionResult<Result<Guid>>> AddAttachment(
        Guid id,
        [FromBody] AddCampaignAttachmentCommand command)
{
    if (id != command.CampaignId)
    {
        return BadRequest(Result<Guid>.Failure("Campaign ID mismatch"));
    }

    var result = await _mediator.Send(command);

    if (result.IsFailure)
    {
        return result.Error!.Contains("not found")
            ? NotFound(result)
            : BadRequest(result);
    }

    return Ok(result);
}

    /// <summary>
    /// Get all attachments for a Campaign
    /// </summary>
    [HttpGet("{id}/attachments")]
[ProducesResponseType(typeof(Result<List<CampaignAttachmentDto>>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(Result<List<CampaignAttachmentDto>>), StatusCodes.Status404NotFound)]
public async Task<ActionResult<Result<List<CampaignAttachmentDto>>>> GetAttachments(Guid id)
{
    var query = new GetCampaignAttachmentsQuery { CampaignId = id };
    var result = await _mediator.Send(query);

    if (result.IsFailure && result.Error!.Contains("not found"))
    {
        return NotFound(result);
    }

    return Ok(result);
}

    /// <summary>
    /// Remove an attachment from a Campaign
    /// </summary>
    [HttpDelete("{id}/attachments/{attachmentId}")]
[ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(Result<bool>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(Result<bool>), StatusCodes.Status404NotFound)]
public async Task<ActionResult<Result<bool>>> RemoveAttachment(
    Guid id,
    Guid attachmentId)
{
    var command = new RemoveCampaignAttachmentCommand
    {
        CampaignId = id,
        AttachmentId = attachmentId
    };

    var result = await _mediator.Send(command);

    if (result.IsFailure)
    {
        return result.Error!.Contains("not found")
            ? NotFound(result)
            : BadRequest(result);
    }

    return Ok(result);
}
}
