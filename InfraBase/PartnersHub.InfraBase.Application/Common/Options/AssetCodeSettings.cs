namespace PartnersHub.InfraBase.Application.Common.Options;

/// <summary>
/// Configuration options for asset code generation
/// </summary>
public class AssetCodeSettings
{
    /// <summary>
    /// Prefix for asset codes (e.g., "Infra")
    /// </summary>
    public string Prefix { get; set; } = "Infra";

    /// <summary>
    /// Number format for padding (e.g., "000000" for 6 digits)
    /// Examples: "000000" produces 000001, "00000" produces 00001
    /// </summary>
    public string NumberFormat { get; set; } = "000000";

    /// <summary>
    /// Separator between prefix and number (e.g., "-")
    /// </summary>
    public string Separator { get; set; } = "-";

    /// <summary>
    /// Generates an asset code from a number
    /// Example: GenerateCode(1) => "Infra-000001"
    /// </summary>
    public string GenerateCode(int number)
    {
        return $"{Prefix}{Separator}{number.ToString(NumberFormat)}";
    }

    /// <summary>
    /// Parses an asset code to extract the number
    /// Example: ParseCode("Infra-000001") => 1
    /// </summary>
    public int? ParseCode(string assetCode)
    {
        if (string.IsNullOrWhiteSpace(assetCode))
            return null;

        var expectedPrefix = $"{Prefix}{Separator}";
        if (!assetCode.StartsWith(expectedPrefix))
            return null;

        var numberPart = assetCode.Substring(expectedPrefix.Length);
        if (int.TryParse(numberPart, out var number))
            return number;

        return null;
    }
}
