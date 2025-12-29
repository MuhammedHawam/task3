namespace PartnersHub.InnovationHub.Application.Common.Helpers;

/// <summary>
/// Helper methods for converting company logo between formats
/// </summary>
public static class LogoHelper
{
    /// <summary>
    /// Converts byte array logo to base64 string with data URI prefix
    /// Returns null if logo is null or empty
    /// </summary>
    /// <param name="logoBytes">The logo as byte array</param>
    /// <returns>Base64 string with data:image prefix, or null</returns>
    public static string? ToBase64String(byte[]? logoBytes)
    {
        if (logoBytes == null || logoBytes.Length == 0)
            return null;

        try
        {
            var base64 = Convert.ToBase64String(logoBytes);
            // Return with data URI scheme for direct use in img src
            return $"data:image/png;base64,{base64}";
        }
        catch
        {
            // If conversion fails, return null
            return null;
        }
    }

    /// <summary>
    /// Converts byte array logo to plain base64 string without prefix
    /// Returns null if logo is null or empty
    /// </summary>
    /// <param name="logoBytes">The logo as byte array</param>
    /// <returns>Plain base64 string, or null</returns>
    public static string? ToBase64StringPlain(byte[]? logoBytes)
    {
        if (logoBytes == null || logoBytes.Length == 0)
            return null;

        try
        {
            return Convert.ToBase64String(logoBytes);
        }
        catch
        {
            return null;
        }
    }
}

