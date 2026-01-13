using PartnersHub.InfraBase.Domain.Common;

namespace PartnersHub.InfraBase.Domain.ValueObjects;

public class AssetName : ValueObject
{
    public string Value { get; private set; }

    private AssetName(string value)
    {
        Value = value;
    }

    public static Result<AssetName> Create(string? value)
    {
        // Asset name is optional for now (can be empty/null).
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<AssetName>.Success(new AssetName(string.Empty));
        }

        var trimmed = value.Trim();
        if (trimmed.Length > 300)
        {
            return Result<AssetName>.Failure("Asset name cannot exceed 300 characters");
        }

        return Result<AssetName>.Success(new AssetName(trimmed));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
