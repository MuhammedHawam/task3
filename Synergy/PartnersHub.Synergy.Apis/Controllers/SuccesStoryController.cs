using MediatR;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.Synergy.Apis.Controllers.Base;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.Opportunities.Commands;
using PartnersHub.Synergy.Application.Opportunities.DTOs;
using PartnersHub.Synergy.Application.Opportunity.Queries;
using PartnersHub.Synergy.Application.SuccessStories.Commands;
using PartnersHub.Synergy.Application.SuccessStories.DTOs;
using PartnersHub.Synergy.Application.SuccessStories.Queries;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuccesStoryController : ApiBaseController<SuccesStoryController>
    {
        private readonly IMediator _mediator;
        public SuccesStoryController(IMediator mediator)
        {
            _mediator = mediator;

        }
        [HttpPost]

        public async Task<ActionResult<Result<Guid>>> SaveRequestAsDraft([FromBody] CreateSuccessStoryCommand command)
        {
            var requestId = await _mediator.Send(command);

           if (requestId.IsFailure)
              {
                return BadRequest(requestId);
              }

            return Ok(requestId);
        }
        [HttpGet("{Id}")]
        public async Task<ActionResult<Result<SuccessStoryResponseDto>>> GetById( Guid Id)
        {
            var query = new GetSuccessStoryByIdQuery { Id = Id };
            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(Result<PaginatedList<SuccessStoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Result<PaginatedList<SuccessStoryResponseDto>>>> Search(
        [FromQuery] List<Guid>? companyIds ,
        [FromQuery] List<int>? collaborationTypes ,
        [FromQuery] List<Guid>? sectorIds,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? partnerCompanyName = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "CreatedAt:desc",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var query = new SearchSuccessStoriesQuery
            {
                CompanyIds = companyIds,
                CollaborationTypes = collaborationTypes,
                SectorIds = sectorIds,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                PartnerCompanyName = partnerCompanyName,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }
        private List<Guid>? ParseGuids(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Guid.TryParse(s, out var guid) ? guid : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();
        }
        private List<int>? ParseInts(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var num) ? num : (int?)null)
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .ToList();
        }

        [HttpPut("approve/{successStoryId}")]
        public async Task<ActionResult<Result>> Approve(Guid successStoryId)
        {
            var command = new ApproveSuccessStoryCommand()
            {
                SuccessStoryId = successStoryId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("asset-manager-reject")]
        public async Task<ActionResult<Result>> RejectSuccessStoryByAssestManager(RejectSuccessStoryByAssetManagerCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
        [HttpGet("by-status")]
        public async Task<ActionResult<Result<List<SuccessStoryResponseDto>>>> GetByStatus([FromQuery] SuccessStoryStatus status)
        {
            var query = new GetSuccessStoriesByStatusQuery { Status = status };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpPut("publish/{successStoryId}")]
        public async Task<ActionResult<Result>> Publish(Guid successStoryId)
        {
            var command = new PublishSuccessStoryCommand()
            {
                SuccessStoryId = successStoryId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("synergy-admin-reject")]
        public async Task<ActionResult<Result>> RejectSuccessStoryBySynergyAdmin(RejectSuccessStoryByAdminCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
        // ========== Success Story Attachments ==========

        /// <summary>
        /// Add an attachment to a success story
        /// </summary>
        [HttpPost("{id}/attachments")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<Guid>>> AddAttachment(
            Guid id,
            [FromBody] AddSuccessStoryAttachmentCommand command)
        {
            if (id != command.SuccessStoryId)
            {
                return BadRequest(Result<Guid>.Failure("Success story ID mismatch"));
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
        /// Get all attachments for a success story
        /// </summary>
        [HttpGet("{id}/attachments")]
        [ProducesResponseType(typeof(Result<List<SuccessStoryAttachmentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<SuccessStoryAttachmentDto>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<List<SuccessStoryAttachmentDto>>>> GetAttachments(Guid id)
        {
            var query = new GetSuccessStoryAttachmentsQuery { SuccessStoryId = id };
            var result = await _mediator.Send(query);

            if (result.IsFailure && result.Error!.Contains("not found"))
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Remove an attachment from a success story
        /// </summary>
        [HttpDelete("{id}/attachments/{attachmentId}")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<bool>>> RemoveAttachment(
            Guid id,
            Guid attachmentId)
        {
            var command = new RemoveSuccessStoryAttachmentCommand
            {
                SuccessStoryId = id,
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


        [HttpPut("visibility/{successStoryId}")]
        public async Task<ActionResult<Result>> SetSuccessStoryVisibility(
                                                                     [FromRoute] Guid successStoryId,
                                                                     [FromQuery] bool hide)
        {
            var command = new SetSuccessStoryVisibilityCommand(
                successStoryId,
                hide);

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);

            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<Result<Guid>>> Update(Guid id,[FromBody] UpdateSuccessStoryCommand command)
        {
            if (id != command.Id)
                return BadRequest("Invalid request");

            var result = await _mediator.Send(command);

            if (result.IsFailure)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
