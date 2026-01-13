using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PartnersHub.InfraBase.Application.Common.Interfaces;

namespace PartnersHub.InfraBase.Infrastructure.Services;

public class ConfigurationLookupService : IConfigurationLookupService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Task<List<LookupDto>?>? _subSectorsTask;
    private Task<List<LookupDto>?>? _assetTypesTask;
    private Task<List<LookupDto>?>? _uomsTask;

    public ConfigurationLookupService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> GetSectorNameAsync(Guid sectorId, CancellationToken cancellationToken = default)
    {
        // ConfigurationHub: GET api/lookups/sectors/{id}
        var dto = await GetAsync<LookupDto>($"api/lookups/sectors/{sectorId}", cancellationToken);
        return dto?.NameEn ?? dto?.NameAr;
    }

    public async Task<string?> GetSubSectorNameAsync(Guid subSectorId, CancellationToken cancellationToken = default)
    {
        // No by-id endpoint; fetch all and find
        var list = await GetSubSectorsAsync(cancellationToken);
        var dto = list?.FirstOrDefault(x => x.Id == subSectorId);
        return dto?.NameEn ?? dto?.NameAr;
    }

    public async Task<string?> GetAssetTypeNameAsync(Guid assetTypeId, CancellationToken cancellationToken = default)
    {
        // No by-id endpoint; fetch all and find
        var list = await GetAssetTypesAsync(cancellationToken);
        var dto = list?.FirstOrDefault(x => x.Id == assetTypeId);
        return dto?.NameEn ?? dto?.NameAr;
    }

    public async Task<string?> GetUomNameAsync(Guid uomId, CancellationToken cancellationToken = default)
    {
        // No by-id endpoint; fetch all and find
        var list = await GetUomsAsync(cancellationToken);
        var dto = list?.FirstOrDefault(x => x.Id == uomId);
        return dto?.NameEn ?? dto?.NameAr;
    }

    private Task<List<LookupDto>?> GetSubSectorsAsync(CancellationToken cancellationToken)
        => _subSectorsTask ??= GetAsync<List<LookupDto>>("api/lookups/subsectors", cancellationToken);

    private Task<List<LookupDto>?> GetAssetTypesAsync(CancellationToken cancellationToken)
        => _assetTypesTask ??= GetAsync<List<LookupDto>>("api/lookups/assettypes", cancellationToken);

    private Task<List<LookupDto>?> GetUomsAsync(CancellationToken cancellationToken)
        => _uomsTask ??= GetAsync<List<LookupDto>>("api/lookups/uoms", cancellationToken);

    private async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            // Forward the caller's token to ConfigurationHub (endpoints are [Authorize]).
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authHeader["Bearer ".Length..].Trim());
            }
            else
            {
                // If it's already a token string, keep it as Bearer.
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authHeader.Trim());
            }
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    private sealed record LookupDto
    {
        public Guid Id { get; init; }
        public string NameAr { get; init; } = string.Empty;
        public string NameEn { get; init; } = string.Empty;
    }
}