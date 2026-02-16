using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Common.Interfaces;

namespace PartnersHub.InfraBase.Infrastructure.Services;

public class UserDisplayNameService : IUserDisplayNameService
{
    private readonly ITokenService _tokenService;
    private readonly IMiddlewareIntegrationService _middlewareIntegrationService;
    private readonly ILogger<UserDisplayNameService> _logger;

    public UserDisplayNameService(
        ITokenService tokenService,
        IMiddlewareIntegrationService middlewareIntegrationService,
        ILogger<UserDisplayNameService> logger)
    {
        _tokenService = tokenService;
        _middlewareIntegrationService = middlewareIntegrationService;
        _logger = logger;
    }

    public async Task<string> ResolveDisplayNameAsync(
        Guid? contactId = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveContactId = NormalizeContactId(contactId) ?? _tokenService.GetContactId();
        if (effectiveContactId.HasValue)
        {
            var contactDisplayName = await ResolveFromContactAsync(effectiveContactId.Value, cancellationToken);
            if (!string.IsNullOrWhiteSpace(contactDisplayName))
            {
                return contactDisplayName;
            }
        }

        var tokenDisplayName = _tokenService.GetUserDisplayName();
        if (!string.IsNullOrWhiteSpace(tokenDisplayName))
        {
            return tokenDisplayName;
        }

        return _tokenService.GetUserName();
    }

    private async Task<string?> ResolveFromContactAsync(Guid contactId, CancellationToken cancellationToken)
    {
        try
        {
            var contact = await _middlewareIntegrationService.GetContactByIdAsync(contactId, cancellationToken);
            if (contact == null)
            {
                return null;
            }

            var displayName = BuildFullName(contact.FirstName, contact.LastName)
                              ?? BuildFullName(contact.FirstNameAr, contact.LastNameAr);
            return displayName;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to resolve user display name from middleware contact {ContactId}. Falling back to token claims.",
                contactId);
            return null;
        }
    }

    private static Guid? NormalizeContactId(Guid? contactId)
    {
        if (!contactId.HasValue || contactId.Value == Guid.Empty)
        {
            return null;
        }

        return contactId.Value;
    }

    private static string? BuildFullName(string? firstName, string? lastName)
    {
        var normalizedFirstName = Normalize(firstName);
        var normalizedLastName = Normalize(lastName);
        var fullName = string.Join(" ", new[] { normalizedFirstName, normalizedLastName }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

        return string.IsNullOrWhiteSpace(fullName) ? null : fullName;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
