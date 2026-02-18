using System.Text.RegularExpressions;

namespace PartnersHub.InfraBase.Domain.Common;

internal static class ActorIdentifierNormalizer
{
    internal const string DefaultActor = "Admin";
    private const int MaxAuditValueLength = 255;

    internal static string NormalizeAuditActor(string? primaryValue, params string?[] fallbackValues)
    {
        var candidates = new[] { primaryValue }.Concat(fallbackValues ?? Array.Empty<string?>());
        foreach (var candidate in candidates)
        {
            var normalized = Normalize(candidate);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            if (Guid.TryParse(normalized, out _))
            {
                continue;
            }

            return normalized;
        }

        return DefaultActor;
    }

    internal static string NormalizeStoredActor(string? value, string defaultValue = DefaultActor)
    {
        var normalized = Normalize(value);
        return string.IsNullOrWhiteSpace(normalized) ? defaultValue : normalized;
    }

    internal static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();

        // Common AD identifier shape: DOMAIN\username. Keep the username part for readability.
        var slashIndex = normalized.LastIndexOf('\\');
        if (slashIndex >= 0 &&
            slashIndex < normalized.Length - 1 &&
            !normalized.Contains('@') &&
            !normalized.Contains(' '))
        {
            normalized = normalized[(slashIndex + 1)..];
        }

        // Remove claim provider prefixes if they exist and keep the actual actor value.
        var pipeIndex = normalized.LastIndexOf('|');
        if (pipeIndex >= 0 && pipeIndex < normalized.Length - 1)
        {
            var candidate = normalized[(pipeIndex + 1)..].Trim();
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                normalized = candidate;
            }
        }

        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
        if (normalized.Length == 0)
        {
            return null;
        }

        return normalized.Length <= MaxAuditValueLength
            ? normalized
            : normalized[..MaxAuditValueLength];
    }
}
