using PartnersHub.InfraBase.Domain.Common;

namespace PartnersHub.InfraBase.Domain.ValueObjects;

public class LocationCity : ValueObject
{
    public string Value { get; private set; }

    private LocationCity(string value)
    {
        Value = value;
    }

    public static Result<LocationCity> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<LocationCity>.Failure("Location/City is required");
        }

        if (value.Length > 500)
        {
            return Result<LocationCity>.Failure("Location/City cannot exceed 500 characters");
        }

        return Result<LocationCity>.Success(new LocationCity(value.Trim()));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
