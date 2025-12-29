namespace PartnersHub.Synergy.Application.Common.Options;

/// <summary>
/// Configuration options for request code generation
/// </summary>
public class RequestCodeSettings
{
    /// <summary>
    /// Prefix for request codes (e.g., "SYN")
    /// </summary>
    public string Prefix { get; set; } = "SYN";

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
    /// Generates an request code from a number
    /// Example: GenerateCode(1) => "SYN-000001"
    /// </summary>
    public string GenerateCode(int number)
    {
        return $"{Prefix}{Separator}{number.ToString(NumberFormat)}";
    }

    /// <summary>
    /// Parses an request code to extract the number
    /// Example: ParseCode("SYN-000001") => 1
    /// </summary>
    public int? ParseCode(string requestCode)
    {
        if (string.IsNullOrWhiteSpace(requestCode))
            return null;

        var expectedPrefix = $"{Prefix}{Separator}";
        if (!requestCode.StartsWith(expectedPrefix))
            return null;

        var numberPart = requestCode.Substring(expectedPrefix.Length);
        if (int.TryParse(numberPart, out var number))
            return number;

        return null;
    }
}
