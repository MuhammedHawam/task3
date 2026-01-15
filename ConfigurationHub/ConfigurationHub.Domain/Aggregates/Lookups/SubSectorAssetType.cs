using PartnersHub.ConfigurationHub.Domain.Common;

namespace PartnersHub.ConfigurationHub.Domain.Aggregates.Lookups;

/// <summary>
/// Mapping between SubSector and AssetType (many-to-many).
/// </summary>
public class SubSectorAssetType : Entity
{
    public Guid SubSectorId { get; private set; }
    public Guid AssetTypeId { get; private set; }

    // EF Core constructor
    private SubSectorAssetType() { }

    private SubSectorAssetType(Guid subSectorId, Guid assetTypeId)
    {
        SubSectorId = subSectorId;
        AssetTypeId = assetTypeId;
    }

    public static Result<SubSectorAssetType> Create(Guid subSectorId, Guid assetTypeId)
    {
        if (subSectorId == Guid.Empty)
        {
            return Result<SubSectorAssetType>.Failure("SubSector is required");
        }

        if (assetTypeId == Guid.Empty)
        {
            return Result<SubSectorAssetType>.Failure("AssetType is required");
        }

        return Result<SubSectorAssetType>.Success(new SubSectorAssetType(subSectorId, assetTypeId));
    }
}
