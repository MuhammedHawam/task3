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
        // Allow empty or null values - AssetName is optional
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<AssetName>.Success(new AssetName(string.Empty));
        }

        if (value.Length > 500)
        {
            return Result<AssetName>.Failure("Asset name cannot exceed 500 characters");
        }

        return Result<AssetName>.Success(new AssetName(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
