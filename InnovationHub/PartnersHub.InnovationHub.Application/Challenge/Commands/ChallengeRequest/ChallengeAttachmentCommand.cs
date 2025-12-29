using MediatR;
using PartnersHub.InnovationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest;

/// <summary>
/// Command to add an attachment to a success story
/// </summary>
public record AddChallengeAttachmentCommand : IRequest<Result<Guid>>
{
    public Guid ChallengeId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public Guid UploadedBy { get; init; }
}

/// <summary>
/// Command to remove an attachment from a success story
/// </summary>
public record RemoveChallengeAttachmentCommand : IRequest<Result<bool>>
{
    public Guid ChallengeId { get; init; }
    public Guid AttachmentId { get; init; }
}

/// <summary>
/// Query to get all attachments for a success story
/// </summary>
public record GetChallengeAttachmentsQuery : IRequest<Result<List<ChallengeAttachmentDto>>>
{
    public Guid ChallengeId { get; init; }
}

/// <summary>
/// DTO for success story attachment
/// </summary>
public record ChallengeAttachmentDto
{
    public Guid Id { get; init; }
    public Guid ChallengeId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public DateTime UploadedAt { get; init; }
    public string? UploadedBy { get; init; }
}
