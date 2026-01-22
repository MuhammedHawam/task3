using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PartnersHub.Shared.Integration.DTOs;
using PartnersHub.Shared.Integration.Options;

namespace PartnersHub.Shared.Integration;

/// <summary>
/// Service for integrating with PIF middleware/company service.
/// </summary>
public class CompanyIntegrationService : ICompanyIntegrationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<CompanyIntegrationService> _logger;
    private readonly MiddlewareApiOptions _options;

    public CompanyIntegrationService(
        HttpClient httpClient,
        IOptions<MiddlewareApiOptions> options,
        ILogger<CompanyIntegrationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value ?? throw new InvalidOperationException("MiddlewareApi configuration is missing");

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new InvalidOperationException("MiddlewareApi:BaseUrl configuration is missing");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("MiddlewareApi:ApiKey configuration is missing");
        }

        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }

        if (!_httpClient.DefaultRequestHeaders.Contains("X-API-KEY"))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _options.ApiKey);
        }
    }

    public async Task<ExternalCompanyDto?> GetCompanyByIdAsync(Guid companyId)
    {
        try
        {
            _logger.LogInformation("Fetching company {CompanyId} from middleware", companyId);

            var response = await _httpClient.GetAsync($"/Companies/get-company/{companyId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Company {CompanyId} not found in middleware", companyId);
                    return null;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Unauthorized access to middleware API for company {CompanyId}", companyId);
                    throw new HttpRequestException($"Unauthorized access to middleware API. Status: {response.StatusCode}");
                }

                _logger.LogError("Failed to fetch company {CompanyId}. Status: {StatusCode}, Reason: {ReasonPhrase}",
                    companyId, response.StatusCode, response.ReasonPhrase);

                throw new HttpRequestException($"Failed to fetch company from middleware API. Status: {response.StatusCode}");
            }

            var wrappedResponse = await response.Content.ReadFromJsonAsync<PifWrappedResponse<PifCompanyData>>();

            if (wrappedResponse?.Data == null)
            {
                _logger.LogWarning("Company {CompanyId} returned null data from middleware", companyId);
                return null;
            }

            var company = MapPifResponseToDto(wrappedResponse.Data);

            _logger.LogInformation("Successfully fetched company {CompanyId} ({CompanyName}) from middleware",
                companyId, company.Name);

            return company;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching company {CompanyId} from middleware", companyId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching company {CompanyId} from middleware", companyId);
            throw;
        }
    }

    public async Task<CompanyIntegrationResponseDto?> GetCompaniesAsync(CompanyIntegrationRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/Companies/get-companies", request, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Unauthorized access to middleware API for companies");
                    throw new HttpRequestException($"Unauthorized access to middleware API. Status: {response.StatusCode}");
                }

                _logger.LogError("Failed to fetch companies. Status: {StatusCode}, Reason: {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);

                throw new HttpRequestException($"Failed to fetch companies from middleware API. Status: {response.StatusCode}");
            }

            var wrappedResponse = await response.Content.ReadFromJsonAsync<PifWrappedResponse<CompanyIntegrationResponseDto>>();

            if (wrappedResponse?.Data == null)
            {
                _logger.LogWarning("Companies list returned null data from middleware");
                return null;
            }

            return wrappedResponse.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching companies from middleware");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching companies from middleware");
            throw;
        }
    }

    private ExternalCompanyDto MapPifResponseToDto(PifCompanyData data)
    {
        return new ExternalCompanyDto
        {
            Id = data.Id,
            Name = data.Name ?? string.Empty,
            NameAr = data.NameAr,
            Description = data.Description,
            DescriptionAr = data.DescriptionAr,
            Logo = ConvertBase64ToByteArray(data.Logo),
            Website = data.Website,
            Country = data.Country ?? "Saudi Arabia",
            City = data.City,
            CityAr = data.CityAr,
            SectorId = data.SectorId,
            SectorName = data.SectorName,
            SectorNameAr = data.SectorNameAr,
            DivisionId = data.DivisionId,
            DivisionName = data.DivisionName,
            DivisionNameAr = data.DivisionNameAr,
            EstablishmentDate = data.EstablishmentDate,
            CreatedOn = data.CreatedOn,
            Representative = MapRepresentative(data.Representative)
        };
    }

    private ExternalRepresentativeDto? MapRepresentative(PifRepresentativeData? rep)
    {
        if (rep == null)
        {
            return null;
        }

        var hasData = !string.IsNullOrWhiteSpace(rep.Name) ||
                      !string.IsNullOrWhiteSpace(rep.Email) ||
                      !string.IsNullOrWhiteSpace(rep.Phone) ||
                      !string.IsNullOrWhiteSpace(rep.Mobile);

        if (!hasData)
        {
            return null;
        }

        return new ExternalRepresentativeDto
        {
            Name = rep.Name,
            NameAr = rep.NameAr,
            Position = rep.Position,
            PositionAr = rep.PositionAr,
            Email = rep.Email,
            Phone = rep.Phone,
            Mobile = rep.Mobile
        };
    }

    private byte[]? ConvertBase64ToByteArray(string? base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(base64String);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert logo base64 string to byte array");
            return null;
        }
    }
}

internal class PifWrappedResponse<T>
{
    public int HttpCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? Error { get; set; }
}

internal class PifCompanyData
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? CityAr { get; set; }
    public Guid? SectorId { get; set; }
    public string? SectorName { get; set; }
    public string? SectorNameAr { get; set; }
    public Guid? DivisionId { get; set; }
    public string? DivisionName { get; set; }
    public string? DivisionNameAr { get; set; }
    public DateTime? EstablishmentDate { get; set; }
    public DateTime? CreatedOn { get; set; }
    public PifRepresentativeData? Representative { get; set; }
}

internal class PifRepresentativeData
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? Position { get; set; }
    public string? PositionAr { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
}
