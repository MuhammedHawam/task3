using System.Net.Http.Json;
using PartnersHub.InfraBase.Application.Common.Interfaces;

namespace PartnersHub.InfraBase.Infrastructure.Services;

public class ConfigurationLookupService : IConfigurationLookupService
{
    private readonly HttpClient _httpClient;
    private Task<List<SectorLookupDto>?>? _sectorsTask;
    private Task<List<SubSectorLookupDto>?>? _subSectorsTask;
    private Task<List<AssetTypeLookupDto>?>? _assetTypesTask;
    private Task<List<UomLookupDto>?>? _uomsTask;

    public ConfigurationLookupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetSectorNameAsync(Guid sectorId, CancellationToken cancellationToken = default)
    {
        // ConfigurationHub: GET api/lookups/sectors/{id}
        var dto = await GetAsync<SectorLookupDto>($"api/lookups/sectors/{sectorId}", cancellationToken);
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

    public async Task<string?> GetSectorNameByCodeAsync(string sectorCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sectorCode)) return null;
        var sectors = await GetSectorsAsync(cancellationToken);
        var dto = sectors?.FirstOrDefault(x => string.Equals(x.Code, sectorCode, StringComparison.OrdinalIgnoreCase));
        return dto?.NameEn ?? dto?.NameAr;
    }

    public async Task<string?> GetSubSectorNameByCodeAsync(string sectorCode, string subSectorCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sectorCode) || string.IsNullOrWhiteSpace(subSectorCode)) return null;

        var sectors = await GetSectorsAsync(cancellationToken);
        var sector = sectors?.FirstOrDefault(x => string.Equals(x.Code, sectorCode, StringComparison.OrdinalIgnoreCase));
        if (sector == null) return null;

        var subSectors = await GetSubSectorsAsync(cancellationToken);
        var dto = subSectors?.FirstOrDefault(x =>
            x.SectorId == sector.Id &&
            string.Equals(x.Code, subSectorCode, StringComparison.OrdinalIgnoreCase));

        return dto?.NameEn ?? dto?.NameAr;
    }

    public async Task<string?> GetAssetTypeNameByCodeAsync(string assetTypeCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(assetTypeCode)) return null;
        var assetTypes = await GetAssetTypesAsync(cancellationToken);
        var dto = assetTypes?.FirstOrDefault(x => string.Equals(x.Code, assetTypeCode, StringComparison.OrdinalIgnoreCase));
        return dto?.NameEn ?? dto?.NameAr;
    }

    public async Task<string?> GetUomNameByCodeAsync(string uomCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uomCode)) return null;
        var uoms = await GetUomsAsync(cancellationToken);
        var dto = uoms?.FirstOrDefault(x => string.Equals(x.Code, uomCode, StringComparison.OrdinalIgnoreCase));
        return dto?.NameEn ?? dto?.NameAr;
    }

    public async Task<string?> GetSectorCodeAsync(Guid sectorId, CancellationToken cancellationToken = default)
    {
        if (sectorId == Guid.Empty) return null;
        var dto = await GetAsync<SectorLookupDto>($"api/lookups/sectors/{sectorId}", cancellationToken);
        return dto?.Code;
    }

    public async Task<string?> GetSubSectorCodeAsync(Guid subSectorId, CancellationToken cancellationToken = default)
    {
        if (subSectorId == Guid.Empty) return null;
        var list = await GetSubSectorsAsync(cancellationToken);
        return list?.FirstOrDefault(x => x.Id == subSectorId)?.Code;
    }

    public async Task<string?> GetAssetTypeCodeAsync(Guid assetTypeId, CancellationToken cancellationToken = default)
    {
        if (assetTypeId == Guid.Empty) return null;
        var list = await GetAssetTypesAsync(cancellationToken);
        return list?.FirstOrDefault(x => x.Id == assetTypeId)?.Code;
    }

    public async Task<string?> GetUomCodeAsync(Guid uomId, CancellationToken cancellationToken = default)
    {
        if (uomId == Guid.Empty) return null;
        var list = await GetUomsAsync(cancellationToken);
        return list?.FirstOrDefault(x => x.Id == uomId)?.Code;
    }

    private async Task<List<SectorLookupDto>?> GetSectorsAsync(CancellationToken cancellationToken)
    {
        // Cache successful results only; if a previous call failed (null), allow retry.
        if (_sectorsTask != null)
        {
            var cached = await _sectorsTask;
            if (cached != null)
            {
                return cached;
            }
        }

        _sectorsTask = GetAsync<List<SectorLookupDto>>("api/lookups/sectors", cancellationToken);
        return await _sectorsTask;
    }

    private async Task<List<SubSectorLookupDto>?> GetSubSectorsAsync(CancellationToken cancellationToken)
    {
        // Cache successful results only; if a previous call failed (null), allow retry.
        if (_subSectorsTask != null)
        {
            var cached = await _subSectorsTask;
            if (cached != null)
            {
                return cached;
            }
        }

        _subSectorsTask = GetAsync<List<SubSectorLookupDto>>("api/lookups/subsectors", cancellationToken);
        return await _subSectorsTask;
    }

    private async Task<List<AssetTypeLookupDto>?> GetAssetTypesAsync(CancellationToken cancellationToken)
    {
        // Cache successful results only; if a previous call failed (null), allow retry.
        if (_assetTypesTask != null)
        {
            var cached = await _assetTypesTask;
            if (cached != null)
            {
                return cached;
            }
        }

        _assetTypesTask = GetAsync<List<AssetTypeLookupDto>>("api/lookups/assettypes", cancellationToken);
        return await _assetTypesTask;
    }

    private async Task<List<UomLookupDto>?> GetUomsAsync(CancellationToken cancellationToken)
    {
        // Cache successful results only; if a previous call failed (null), allow retry.
        if (_uomsTask != null)
        {
            var cached = await _uomsTask;
            if (cached != null)
            {
                return cached;
            }
        }

        _uomsTask = GetAsync<List<UomLookupDto>>("api/lookups/uoms", cancellationToken);
        return await _uomsTask;
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }

    private sealed record SectorLookupDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string NameAr { get; init; } = string.Empty;
        public string NameEn { get; init; } = string.Empty;
    }

    private sealed record SubSectorLookupDto
    {
        public Guid Id { get; init; }
        public Guid SectorId { get; init; }
        public string Code { get; init; } = string.Empty;
        public string NameAr { get; init; } = string.Empty;
        public string NameEn { get; init; } = string.Empty;
    }

    private sealed record AssetTypeLookupDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string NameAr { get; init; } = string.Empty;
        public string NameEn { get; init; } = string.Empty;
    }

    private sealed record UomLookupDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string NameAr { get; init; } = string.Empty;
        public string NameEn { get; init; } = string.Empty;
    }
}