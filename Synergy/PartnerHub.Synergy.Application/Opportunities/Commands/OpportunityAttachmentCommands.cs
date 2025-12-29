using MediatR;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.Opportunities.Commands;

/// <summary>
/// Command to add an attachment to an opportunity
/// </summary>
public record AddOpportunityAttachmentCommand : IRequest<Result<Guid>>
{
    public Guid OpportunityId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public string? UploadedBy { get; init; }
}

/// <summary>
/// Command to remove an attachment from an opportunity
/// </summary>
public record RemoveOpportunityAttachmentCommand : IRequest<Result<bool>>
{
    public Guid OpportunityId { get; init; }
    public Guid AttachmentId { get; init; }
}

/// <summary>
/// Query to get all attachments for an opportunity
/// </summary>
public record GetOpportunityAttachmentsQuery : IRequest<Result<List<OpportunityAttachmentDto>>>
{
    public Guid OpportunityId { get; init; }
}

/// <summary>
/// DTO for opportunity attachment
/// </summary>
public record OpportunityAttachmentDto
{
    public Guid Id { get; init; }
    public Guid OpportunityId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public DateTime UploadedAt { get; init; }
    public string? UploadedBy { get; init; }
}
