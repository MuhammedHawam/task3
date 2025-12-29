using MediatR;
using PartnersHub.InnovationHub.Domain.Common;


namespace PartnersHub.InnovationHub.Application.Campaign.Commands;

/// <summary>
/// Command to add an attachment to a success story
/// </summary>
public record AddCampaignAttachmentCommand : IRequest<Result<Guid>>
{
    public Guid CampaignId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public Guid UploadedBy { get; init; }
}

/// <summary>
/// Command to remove an attachment from a success story
/// </summary>
public record RemoveCampaignAttachmentCommand : IRequest<Result<bool>>
{
    public Guid CampaignId { get; init; }
    public Guid AttachmentId { get; init; }
}

/// <summary>
/// Query to get all attachments for a success story
/// </summary>
public record GetCampaignAttachmentsQuery : IRequest<Result<List<CampaignAttachmentDto>>>
{
    public Guid CampaignId { get; init; }
}

/// <summary>
/// DTO for success story attachment
/// </summary>
public record CampaignAttachmentDto
{
    public Guid Id { get; init; }
    public Guid CampaignId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public DateTime UploadedAt { get; init; }
    public string? UploadedBy { get; init; }
}
