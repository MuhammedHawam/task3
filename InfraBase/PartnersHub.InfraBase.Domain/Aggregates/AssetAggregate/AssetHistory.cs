using PartnersHub.InfraBase.Domain.Common;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

/// <summary>
/// Represents a history entry tracking changes to an asset
/// </summary>
public class AssetHistory : Entity
{
    public Guid AssetId { get; private set; }
    public AssetStatuses Status { get; private set; }
    public string Action { get; private set; }
    public string PerformedBy { get; private set; } = null!;
    public DateTime PerformedAt { get; private set; }
    public string? Comments { get; private set; }
    public string? FieldsChanged { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }

    private AssetHistory() 
    { 
        Action = string.Empty;
    }

    public AssetHistory(Guid assetId, AssetStatuses status, string action, string performedBy, 
        string? comments = null, string? fieldsChanged = null, string? oldValues = null, string? newValues = null)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action is required", nameof(action));
        }

        AssetId = assetId;
        Status = status;
        Action = action;
        PerformedBy = ActorIdentifierNormalizer.NormalizeAuditActor(performedBy);
        PerformedAt = DateTime.Now;
        Comments = comments;
        FieldsChanged = fieldsChanged;
        OldValues = oldValues;
        NewValues = newValues;
    }
}
