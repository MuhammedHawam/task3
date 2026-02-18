using PartnersHub.InfraBase.Domain.Common;
using PartnersHub.InfraBase.Domain.ValueObjects;

namespace PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

public class AssetAttachment : Entity
{
    public Guid AssetId { get; private set; }
    public AttachmentMetadata Metadata { get; private set; } = null!;
    public string SharePointUrl { get; private set; } = string.Empty;
    public string UploadedBy { get; private set; } = null!;
    public DateTime UploadedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private AssetAttachment() { }

    internal AssetAttachment(Guid assetId, string fileName, long fileSizeInBytes,
        string contentType, string sharePointUrl, string uploadedBy)
    {
        var metadataResult = AttachmentMetadata.Create(fileName, fileSizeInBytes, contentType);
        if (metadataResult.IsFailure)
        {
            throw new ArgumentException(metadataResult.Error);
        }

        if (string.IsNullOrWhiteSpace(sharePointUrl))
        {
            throw new ArgumentException("SharePoint URL is required");
        }

        AssetId = assetId;
        Metadata = metadataResult.Value!;
        SharePointUrl = sharePointUrl;
        UploadedBy = ActorIdentifierNormalizer.NormalizeAuditActor(uploadedBy);
        UploadedAt = DateTime.Now;
        IsDeleted = false;
    }

    public Result<bool> MarkAsDeleted(string deletedBy)
    {
        if (IsDeleted)
        {
            return Result<bool>.Failure("Attachment is already deleted");
        }

        IsDeleted = true;
        DeletedBy = ActorIdentifierNormalizer.NormalizeAuditActor(deletedBy, UploadedBy);
        DeletedAt = DateTime.Now;

        return Result<bool>.Success(true);
    }
}
