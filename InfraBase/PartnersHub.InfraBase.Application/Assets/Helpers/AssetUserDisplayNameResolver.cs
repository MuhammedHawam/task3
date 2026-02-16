namespace PartnersHub.InfraBase.Application.Assets.Helpers;

internal static class AssetUserDisplayNameResolver
{
    public static string? ResolveSubmittedBy(string? submittedBy, string? createdBy)
    {
        var normalizedSubmittedBy = Normalize(submittedBy);
        var normalizedCreatedBy = Normalize(createdBy);

        // Prefer a human-readable value (non-GUID) when available.
        if (IsHumanReadable(normalizedSubmittedBy))
        {
            return normalizedSubmittedBy;
        }

        if (IsHumanReadable(normalizedCreatedBy))
        {
            return normalizedCreatedBy;
        }

        return normalizedSubmittedBy ?? normalizedCreatedBy;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsHumanReadable(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && !Guid.TryParse(value, out _);
    }
}