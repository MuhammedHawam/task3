using PartnersHub.InfraBase.Domain.Common;

namespace PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

/// <summary>
/// Represents a CAPEX (Capital Expenditure) year entry for an asset
/// </summary>
public class AssetCapex : Entity
{
    public Guid AssetId { get; private set; }
    public int Year { get; private set; }
    public decimal Amount { get; private set; }

    private AssetCapex() { }

    public AssetCapex(Guid assetId, int year, decimal amount)
    {
        if (year < 2000 || year > 2099)
        {
            throw new ArgumentException("Year must be between 2000 and 2099", nameof(year));
        }

        if (amount <= 0)
        {
            throw new ArgumentException("CAPEX amount must be greater than zero", nameof(amount));
        }

        if (amount > 999999999999999m) // Max 15 digits
        {
            throw new ArgumentException("CAPEX amount cannot exceed 15 digits", nameof(amount));
        }

        AssetId = assetId;
        Year = year;
        Amount = amount;
    }

    public Result<bool> UpdateAmount(decimal newAmount)
    {
        if (newAmount <= 0)
        {
            return Result<bool>.Failure("CAPEX amount must be greater than zero");
        }

        if (newAmount > 999999999999999m)
        {
            return Result<bool>.Failure("CAPEX amount cannot exceed 15 digits");
        }

        Amount = newAmount;
        return Result<bool>.Success(true);
    }
}
