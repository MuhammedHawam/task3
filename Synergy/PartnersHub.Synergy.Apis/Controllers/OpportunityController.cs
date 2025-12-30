using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.Synergy.Apis.Controllers.Base;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.Opportunities.Commands;
using PartnersHub.Synergy.Application.Opportunities.DTOs;
using PartnersHub.Synergy.Application.Opportunities.Queries;
using PartnersHub.Synergy.Application.Opportunity.Queries;
using PartnersHub.Synergy.Application.SynergyCompany.Queries;
using PartnersHub.Synergy.Domain.Common;
using System.Net;
using System.Runtime.CompilerServices;

namespace PartnersHub.Synergy.Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpportunityController : ApiBaseController<OpportunityController>
    {
        private readonly IMediator _mediator;

        public OpportunityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Search opportunities with pagination, search, and filtering
        /// </summary>
        /// <param name="companyIds">Comma-separated company IDs that created the opportunities</param>
        [HttpGet("search")]
        [ProducesResponseType(typeof(Result<PaginatedList<OpportunitySearchCardDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Result<PaginatedList<OpportunitySearchCardDto>>>> SearchOpportunities(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? companyIds = null,
            [FromQuery] string? sectorIds = null,
            [FromQuery] string? opportunityTypeIds = null,
            [FromQuery] string? thematicAreaIds = null,
            [FromQuery] string? collaborationRequirementIds = null,
            [FromQuery] string? expectedOutcomeIds = null,
            [FromQuery] string? collaborationStatuses = null,
            [FromQuery] string? status = null,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
            var query = new SearchOpportunitiesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                CompanyIds = ParseGuids(companyIds),
                SectorIds = ParseGuids(sectorIds),
                OpportunityTypeIds = ParseInts(opportunityTypeIds),
                ThematicAreaIds = ParseInts(thematicAreaIds),
                CollaborationRequirementIds = ParseInts(collaborationRequirementIds),
                ExpectedOutcomeIds = ParseInts(expectedOutcomeIds),
                Statuses = ParseInts(status),
                CollaborationStatuses = ParseInts(collaborationStatuses),
                StartDate = ParseDateOnly(startDate),
                EndDate = ParseDateOnly(endDate),
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<Guid>>> SaveRequestAsDraft([FromBody] CreateOpportunityCommand command)
        {
            var requestId = await _mediator.Send(command);
            return Ok(requestId);
        }

        [HttpGet("details/{Id}")]
        public async Task<ActionResult<Result<OpportunityResponseDto>>> GetOpportunityById(Guid Id)
        {
            var query = new GetOpportunityDetailsQuery { Id = Id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("companies/{companyId}")]
        public async Task<ActionResult<Result<List<OpportunityResponseDto>>>> GetOpportunityByCompanyId(Guid companyId)
        {
            var query = new GetOpportunitiesByCompanyIdQuery { CompanyId = companyId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        [HttpGet("by-status")]
        public async Task<ActionResult<Result<List<OpportunityResponseDto>>>> GetByStatus([FromQuery] OpportunityStatus status)
        {
            var query = new GetOpportunitiesByStatusQuery { Status = status };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("approve/{opportunityId}")]
        public async Task<ActionResult<Result>> ApproveOpportunityByAssetManager(Guid opportunityId)
        {
            var command = new ApproveOpportunityCommand()
            {
                OpportunityId = opportunityId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("asset-manager-reject")]
        public async Task<ActionResult<Result>> RejectOpportunityByAssetManager(RejectOpportunityByAssetManagerCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPut("publish/{opportunityId}")]
        public async Task<ActionResult<Result>> PublishOpportunityBySynergyAdmin(Guid opportunityId)
        {
            var command = new PublishOpportunityCommand()
            {
                OpportunityId = opportunityId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("synergy-admin-reject")]
        public async Task<ActionResult<Result>> RejectOpportunityBySynergyAdmin(RejectOpportunityBySynergyAdminCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse>> GetAll()
        {
            var query = new GetAllOpportunitiesQuery();
            return await Execute(query);
        }

        // ========== Opportunity Attachments ==========

        /// <summary>
        /// Add an attachment to an opportunity
        /// </summary>
        [HttpPost("{id}/attachments")]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<Guid>>> AddAttachment(
            Guid id,
            [FromBody] AddOpportunityAttachmentCommand command)
        {
            if (id != command.OpportunityId)
            {
                return BadRequest(Result<Guid>.Failure("Opportunity ID mismatch"));
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
        /// Get all attachments for an opportunity
        /// </summary>
        [HttpGet("{id}/attachments")]
        [ProducesResponseType(typeof(Result<List<OpportunityAttachmentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<OpportunityAttachmentDto>>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<List<OpportunityAttachmentDto>>>> GetAttachments(Guid id)
        {
            var query = new GetOpportunityAttachmentsQuery { OpportunityId = id };
            var result = await _mediator.Send(query);

            if (result.IsFailure && result.Error!.Contains("not found"))
            {
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Remove an attachment from an opportunity
        /// </summary>
        [HttpDelete("{id}/attachments/{attachmentId}")]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Result<bool>>> RemoveAttachment(
            Guid id,
            Guid attachmentId)
        {
            var command = new RemoveOpportunityAttachmentCommand
            {
                OpportunityId = id,
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


        [HttpPut("visibility/{opportunityId}")]
        public async Task<ActionResult<Result>> SetOpportunityVisibility(
                                                                         Guid opportunityId,
                                                                         [FromQuery] bool hide)
        {
            var command = new SetOpportunityVisibilityCommand(
                opportunityId,
                hide);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{opportunityId}")]
        public async Task<ActionResult<Result>> UpdateOpportunity(Guid opportunityId, [FromBody] UpdateOpportunityCommand command)
        {
            var updated = command with { OpportunityId = opportunityId };
            var result = await _mediator.Send(updated);
            return Ok(result);
        }
        #region Helper Methods

        private List<Guid>? ParseGuids(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Guid.TryParse(s, out var guid) ? guid : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();
        }
        private DateOnly? ParseDateOnly(string? dateString)
        {
            return DateOnly.TryParse(dateString, out DateOnly parsedDate) ? (DateOnly?)parsedDate : null;
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

        #endregion
    }
}
