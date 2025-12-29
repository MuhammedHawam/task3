using PartnersHub.InfraBase.Domain.Common;

namespace PartnersHub.InfraBase.Domain.ValueObjects;

public class AssetDescription : ValueObject
{
    public string Value { get; private set; }

    private AssetDescription(string value)
    {
        Value = value;
    }

    public static Result<AssetDescription> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<AssetDescription>.Success(new AssetDescription(string.Empty));
        }

        if (value.Length > 3000)
        {
            return Result<AssetDescription>.Failure("Asset description cannot exceed 3000 characters");
        }

        return Result<AssetDescription>.Success(new AssetDescription(value.Trim()));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
