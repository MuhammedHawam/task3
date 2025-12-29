using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.InnovationHub.Apis.Common;
using PartnersHub.InnovationHub.Apis.Controllers.Base;
using PartnersHub.InnovationHub.Application.Campaign.Queries;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest;
using PartnersHub.InnovationHub.Application.Challenge.Commands.LinkTechnologyToChallenge;
using PartnersHub.InnovationHub.Application.Challenge.Queries;
using PartnersHub.InnovationHub.Application.Challenge.Queries.ChallengeRequest;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Models;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;

namespace PartnersHub.InnovationHub.Apis.Controllers.ChallengeRequest
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ChallengeRequestController : ApiBaseController<ChallengeRequestController>
    {
        private readonly IMediator _mediator;
        public ChallengeRequestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public Task<ActionResult<ApiResponse>> Create([FromBody] CreateChallengeRequestCommand command, CancellationToken cancellationToken)
             => Execute(command, cancellationToken);

        [HttpPost("link-to-technology")]
        public Task<ActionResult<ApiResponse>> LinkToTechnology(LinkTechnologyToChallengeCommand command, CancellationToken cancellationToken)
         => Execute(command, cancellationToken);

        [HttpPost("link-additional-technology")]
        public Task<ActionResult<ApiResponse>> LinkAdditionalTechnology(LinkAdditionalTechnologyToChallengeCommand command, CancellationToken cancellationToken)
            => Execute(command, cancellationToken);

        [HttpPost("ArchiveAndUnarchive")]
        public Task<ActionResult<ApiResponse>> ArchiveAndUnarchive(UnarchiveChallengeRequestCommand request, CancellationToken cancellationToken)
             => Execute(request, cancellationToken);


        [HttpDelete("{requestId}/draft")]
        public Task<ActionResult<ApiResponse>> DeleteDraft(Guid requestId, CancellationToken cancellationToken)
              => Execute(new DeleteChallengeRequestDraftCommand { RequestId = requestId }, cancellationToken);


        [HttpGet("List")]
        [ProducesResponseType(typeof(Result<PaginatedList<ChallengeCardDTO>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Result<PaginatedList<ChallengeCardDTO>>>> List(
                                                                                      [FromQuery] string? SearchTerm,
                                                                                      [FromQuery] List<Guid>? DevCoId,
                                                                                      [FromQuery] List<Guid>? SectorId,
                                                                                      [FromQuery] List<string>? PriorityLevel,
                                                                                      [FromQuery] bool? IsMyChallenge,
                                                                                      [FromQuery] Guid? UserId,
                                                                                      [FromQuery] bool? IsAdmin,
                                                                                      [FromQuery] bool? IsCounts,
                                                                                      [FromQuery] List<string>? StatusList,
                                                                                      [FromQuery] bool? IsPending,
                                                                                      [FromQuery] int PageSize = 8,
                                                                                      [FromQuery] int PageNumber = 1)
        {
            var query = new ChallengeRequestListQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                Search = SearchTerm,
                DevCoId = DevCoId,  
                SectorId = SectorId,
                PriorityLevel = PriorityLevel,
                IsMyChallenge = IsMyChallenge,
                IsCounts = IsCounts,
                UserId = UserId,
                StatusList = StatusList,
                IsAdmin = IsAdmin,
                IsPending = IsPending
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }


        [HttpGet("{requestId}/details")]
        public Task<ActionResult<ApiResponse>> Details(Guid requestId, CancellationToken cancellationToken)
                     => Execute(new ChallengeDetailsQuery { ChallengeId = requestId }, cancellationToken);


        [HttpPost("Review")]
        public Task<ActionResult<ApiResponse>> Review([FromBody] ReviewChallengeRequestCommand reviewChallenge, CancellationToken cancellationToken)
             => Execute(reviewChallenge, cancellationToken);

        [HttpPut("Edit")]
        public Task<ActionResult<ApiResponse>> Edit([FromBody] EditChallengeRequestCommand editChallenge, CancellationToken cancellationToken)
         => Execute(editChallenge, cancellationToken);


        [HttpPost("ChallengeByCompanyId")]
        public Task<ActionResult<ApiResponse>> ChallengeByCompanyId(ChallengewithCampaignListQuery challengewithCampaignListQuery, CancellationToken cancellationToken)
              => Execute(challengewithCampaignListQuery, cancellationToken);


        [HttpGet("DashboardCounts")]
        public Task<ActionResult<ApiResponse>> DashboardCounts(CancellationToken cancellationToken)
              => Execute(new ChallengeDashboardQuery { }, cancellationToken);



        /// <summary>
        /// Add an attachment to a Challenge
        /// </summary>
        [HttpPost("{id}/attachments")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<Guid>>> AddAttachment(
            Guid id,
            [FromBody] AddChallengeAttachmentCommand command)
        {
            if (id != command.ChallengeId)
            {
                return BadRequest(Result<Guid>.Failure("Challenge ID mismatch"));
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
        /// Get all attachments for a Challenge
        /// </summary>
        [HttpGet("{id}/attachments")]
        [ProducesResponseType(typeof(Result<List<ChallengeAttachmentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<ChallengeAttachmentDto>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<List<ChallengeAttachmentDto>>>> GetAttachments(Guid id)
        {
            var query = new GetChallengeAttachmentsQuery { ChallengeId = id };
            var result = await _mediator.Send(query);

            if (result.IsFailure && result.Error!.Contains("not found"))
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Remove an attachment from a Challenge
        /// </summary>
        [HttpDelete("{id}/attachments/{attachmentId}")]
        public async Task<ActionResult<Result<bool>>> RemoveAttachment(
             Guid id,
             Guid attachmentId)
        {
            var command = new RemoveChallengeAttachmentCommand
            {
                ChallengeId = id,
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
}
