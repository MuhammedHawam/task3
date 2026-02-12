using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.Synergy.Apis.Controllers.Base;
using PartnersHub.Synergy.Apis.Models;
using PartnersHub.Synergy.Application.Interfaces.Integration;
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
        private readonly IMiddlewareIntegrationService _middlewareIntegrationService;

        public OpportunityController(
            IMediator mediator,
            IMiddlewareIntegrationService middlewareIntegrationService)
        {
            _mediator = mediator;
            _middlewareIntegrationService = middlewareIntegrationService;
        }

        /// <summary>
        /// Search opportunities with pagination, search, and filtering
        /// </summary>
        /// <param name="companyIds">Comma-separated company IDs that created the opportunities</param>
        [HttpGet("search")]
        [ProducesResponseType(typeof(Result<PaginatedList<OpportunitySearchCardDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Result<PaginatedList<OpportunitySearchCardDto>>>> SearchOpportunities(
            [FromQuery] string? searchTerm,
            [FromQuery] List<Guid>? companyIds,
            [FromQuery] List<Guid>? sectorIds,
            [FromQuery] List<int>? opportunityTypeIds,
            [FromQuery] List<int>? thematicAreaIds,
            [FromQuery] List<int>? collaborationRequirementIds,
            [FromQuery] List<int>? expectedOutcomeIds,
            [FromQuery] List<int>? collaborationStatuses,
            [FromQuery] List<int>? status,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate,
            [FromQuery] bool? includeIsHide,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? sortBy = "CreatedAt:desc"

            )
        {
            var query = new SearchOpportunitiesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                CompanyIds = companyIds,
                SectorIds = sectorIds,
                OpportunityTypeIds = opportunityTypeIds,
                ThematicAreaIds = thematicAreaIds,
                CollaborationRequirementIds = collaborationRequirementIds,
                ExpectedOutcomeIds = expectedOutcomeIds,
                Statuses = status,
                CollaborationStatuses = collaborationStatuses,
                StartDate = ParseDateOnly(startDate),
                EndDate = ParseDateOnly(endDate),
                IncludeIsHide = includeIsHide,
                SortBy = sortBy
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<Guid>>> SaveRequestAsDraft([FromForm] CreateOpportunityFormRequest request)
        {
            var filesToUpload = MapFiles(request.Files);
            var command = request.Opportunity!;
            command.FilesToUpload = filesToUpload;
            command.AttachmentDescription = request.AttachmentDescription;

            var requestId = await _mediator.Send(command);

            if (requestId.IsFailure)
            {
                return BadRequest(requestId);
            }
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
                                                                         [FromRoute] Guid opportunityId,
                                                                         [FromQuery] bool hide)
        {
            var command = new SetOpportunityVisibilityCommand(
                opportunityId,
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

        [HttpPut("{opportunityId}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Result>> UpdateOpportunity(Guid opportunityId, [FromForm] UpdateOpportunityFormRequest request)
        {

            var filesToUpload = MapFiles(request.Files);
            var command = request.Opportunity!;
            command.FilesToUpload = filesToUpload;
            command.AttachmentDescription = request.AttachmentDescription;

            var updated = command with { OpportunityId = opportunityId };
            var result = await _mediator.Send(updated);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);

            }
           
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

        private static List<FileUploadContent> MapFiles(IEnumerable<IFormFile>? files)
        {
            if (files == null)
            {
                return [];
            }

            return files
                .Where(file => file is { Length: > 0 })
                .Select(file => new FileUploadContent(
                    Path.GetFileName(file.FileName),
                    file.ContentType ?? string.Empty,
                    file.Length,
                    file.OpenReadStream))
                .ToList();
        }

        #endregion
    }
}
