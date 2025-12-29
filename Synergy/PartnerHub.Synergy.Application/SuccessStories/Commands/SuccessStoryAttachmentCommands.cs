using MediatR;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SuccessStories.Commands;

/// <summary>
/// Command to add an attachment to a success story
/// </summary>
public record AddSuccessStoryAttachmentCommand : IRequest<Result<Guid>>
{
    public Guid SuccessStoryId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public string? UploadedBy { get; init; }
}

/// <summary>
/// Command to remove an attachment from a success story
/// </summary>
public record RemoveSuccessStoryAttachmentCommand : IRequest<Result<bool>>
{
    public Guid SuccessStoryId { get; init; }
    public Guid AttachmentId { get; init; }
}

/// <summary>
/// Query to get all attachments for a success story
/// </summary>
public record GetSuccessStoryAttachmentsQuery : IRequest<Result<List<SuccessStoryAttachmentDto>>>
{
    public Guid SuccessStoryId { get; init; }
}

/// <summary>
/// DTO for success story attachment
/// </summary>
public record SuccessStoryAttachmentDto
{
    public Guid Id { get; init; }
    public Guid SuccessStoryId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public DateTime UploadedAt { get; init; }
    public string? UploadedBy { get; init; }
}
