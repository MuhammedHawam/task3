using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;
using System.Collections.Concurrent;

namespace PartnersHub.InfraBase.Infrastructure.Services;

public class AssetSubmittedByResolver : IAssetSubmittedByResolver
{
    private readonly IMiddlewareIntegrationService _middlewareIntegrationService;
    private readonly ILogger<AssetSubmittedByResolver> _logger;
    private readonly ConcurrentDictionary<Guid, string?> _contactNameCache = new();

    public AssetSubmittedByResolver(
        IMiddlewareIntegrationService middlewareIntegrationService,
        ILogger<AssetSubmittedByResolver> logger)
    {
        _middlewareIntegrationService = middlewareIntegrationService;
        _logger = logger;
    }

    public async Task<string?> ResolveAsync(
        string? submittedBy,
        string? createdBy,
        CancellationToken cancellationToken = default)
    {
        var normalizedSubmittedBy = Normalize(submittedBy);
        var normalizedCreatedBy = Normalize(createdBy);

        if (IsHumanReadable(normalizedSubmittedBy))
        {
            return normalizedSubmittedBy;
        }

        var candidateContactId = ParseContactId(normalizedSubmittedBy) ?? ParseContactId(normalizedCreatedBy);
        if (candidateContactId.HasValue)
        {
            var contactDisplayName = await GetContactDisplayNameAsync(candidateContactId.Value, cancellationToken);
            if (!string.IsNullOrWhiteSpace(contactDisplayName))
            {
                return contactDisplayName;
            }
        }

        if (IsHumanReadable(normalizedCreatedBy))
        {
            return normalizedCreatedBy;
        }

        return normalizedSubmittedBy ?? normalizedCreatedBy;
    }

    public async Task<string?> ResolveUserValueAsync(
        string? userValue,
        CancellationToken cancellationToken = default)
    {
        var normalizedUserValue = Normalize(userValue);
        if (string.IsNullOrWhiteSpace(normalizedUserValue))
        {
            return null;
        }

        if (IsHumanReadable(normalizedUserValue))
        {
            return normalizedUserValue;
        }

        var contactId = ParseContactId(normalizedUserValue);
        if (contactId.HasValue)
        {
            var contactDisplayName = await GetContactDisplayNameAsync(contactId.Value, cancellationToken);
            if (!string.IsNullOrWhiteSpace(contactDisplayName))
            {
                return contactDisplayName;
            }
        }

        return normalizedUserValue;
    }

    public async Task<IReadOnlyDictionary<Guid, string?>> ResolveForAssetsAsync(
        IEnumerable<Asset> assets,
        CancellationToken cancellationToken = default)
    {
        const int maxConcurrency = 8;
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        var results = new ConcurrentDictionary<Guid, string?>();
        var tasks = assets.Select(async asset =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var displayName = await ResolveAsync(asset.SubmittedBy, asset.CreatedBy, cancellationToken);
                results[asset.Id] = displayName;
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }

    public async Task<IReadOnlyDictionary<string, string?>> ResolveUserValuesAsync(
        IEnumerable<string?> userValues,
        CancellationToken cancellationToken = default)
    {
        var normalizedValues = userValues
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        const int maxConcurrency = 8;
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        var results = new ConcurrentDictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var tasks = normalizedValues.Select(async value =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var resolvedValue = await ResolveUserValueAsync(value, cancellationToken);
                results[value] = resolvedValue;
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return results;
    }

    private async Task<string?> GetContactDisplayNameAsync(Guid contactId, CancellationToken cancellationToken)
    {
        if (_contactNameCache.TryGetValue(contactId, out var cachedName))
        {
            return cachedName;
        }

        var loadedName = await LoadContactDisplayNameAsync(contactId, cancellationToken);
        _contactNameCache.TryAdd(contactId, loadedName);
        return loadedName;
    }

    private async Task<string?> LoadContactDisplayNameAsync(Guid contactId, CancellationToken cancellationToken)
    {
        try
        {
            var contact = await _middlewareIntegrationService.GetContactByIdAsync(contactId, cancellationToken);
            if (contact == null)
            {
                return null;
            }

            return BuildFullName(contact.FirstName, contact.LastName) ??
                   BuildFullName(contact.FirstNameAr, contact.LastNameAr);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to resolve submitted by display name from contact {ContactId}.",
                contactId);
            return null;
        }
    }

    private static Guid? ParseContactId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var contactId) || contactId == Guid.Empty)
        {
            return null;
        }

        return contactId;
    }

    private static string? BuildFullName(string? firstName, string? lastName)
    {
        var normalizedFirstName = Normalize(firstName);
        var normalizedLastName = Normalize(lastName);
        var fullName = string.Join(" ", new[] { normalizedFirstName, normalizedLastName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));

        return string.IsNullOrWhiteSpace(fullName) ? null : fullName;
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
