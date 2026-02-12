using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PartnersHub.Synergy.Application.Interfaces.Integration;
using PartnersHub.Synergy.Application.SynergyCompany.DTOs;
using PartnersHub.Synergy.Application.SynergyCompany.Queries;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;

namespace PartnersHub.Synergy.Infrastructure.Services.Integration;

/// <summary>
/// Service for integrating with PIF middleware/company service
/// </summary>
public class CompanyIntegrationService : ICompanyIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CompanyIntegrationService> _logger;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public CompanyIntegrationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CompanyIntegrationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["MiddlewareApi:BaseUrl"] ?? throw new InvalidOperationException("MiddlewareApi:BaseUrl configuration is missing");
        _apiKey = configuration["MiddlewareApi:ApiKey"] ?? throw new InvalidOperationException("MiddlewareApi:ApiKey configuration is missing");
        
        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
        
        // Add API Key to default headers
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);
    }

    /// <summary>
    /// Fetches company details from PIF middleware service
    /// </summary>
    public async Task<ExternalCompanyDto?> GetCompanyByIdAsync(Guid companyId)
    {
        try
        {
            _logger.LogInformation("Fetching company {CompanyId} from PIF middleware at {BaseUrl}", companyId, _baseUrl);

            // Call PIF API
            var response = await _httpClient.GetAsync($"/Companies/get-company/{companyId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Company {CompanyId} not found in PIF system", companyId);
                    return null;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Unauthorized access to PIF API for company {CompanyId}. Check API key configuration.", companyId);
                    throw new HttpRequestException($"Unauthorized access to PIF API. Status: {response.StatusCode}");
                }

                _logger.LogError("Failed to fetch company {CompanyId}. Status: {StatusCode}, Reason: {ReasonPhrase}",
                    companyId, response.StatusCode, response.ReasonPhrase);
                
                throw new HttpRequestException($"Failed to fetch company from PIF API. Status: {response.StatusCode}");
            }

            // PIF API returns wrapped response: { httpCode, status, data, error }
            var wrappedResponse = await response.Content.ReadFromJsonAsync<PifWrappedResponse<PifCompanyData>>();
            
            if (wrappedResponse == null || wrappedResponse.Data == null)
            {
                _logger.LogWarning("Company {CompanyId} returned null data from PIF system", companyId);
                return null;
            }

            // Map PIF response to ExternalCompanyDto
            var company = MapPifResponseToDto(wrappedResponse.Data);

            _logger.LogInformation("Successfully fetched company {CompanyId} ({CompanyName}) from PIF middleware", 
                companyId, company.Name);

            return company;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching company {CompanyId} from PIF middleware", companyId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching company {CompanyId} from PIF middleware", companyId);
            throw;
        }
    }

    public async Task<CompanyIntegrationResponseDto?> GetCompaniesList(CompaniesListQuery request)
    {
        try
        {
            var jsonContent = JsonConvert.SerializeObject(request, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.None
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Call PIF API
            var response = await _httpClient.PostAsync($"/Companies/get-companies", content);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Company not found in PIF system");
                    return null;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Unauthorized access to PIF API for companies. Check API key configuration.", "");
                    throw new HttpRequestException($"Unauthorized access to PIF API. Status: {response.StatusCode}");
                }

                _logger.LogError("Failed to fetch companies . Status: {StatusCode}, Reason: {ReasonPhrase}",
                    "", response.StatusCode, response.ReasonPhrase);

                throw new HttpRequestException($"Failed to fetch company from PIF API. Status: {response.StatusCode}");
            }

            // PIF API returns wrapped response: { httpCode, status, data, error }
            var wrappedResponse = await response.Content.ReadFromJsonAsync<PifWrappedResponse<CompanyIntegrationResponseDto>>();

            if (wrappedResponse == null || wrappedResponse.Data == null)
            {
                _logger.LogWarning("Companies list returned null data from PIF system");
                return null;
            }


            _logger.LogInformation("Successfully fetched companies list from PIF middleware",
                "", "");

            return wrappedResponse.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching companies from PIF middleware", "");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching companies from PIF middleware", "");
            throw;
        }
    }

    /// <summary>
    /// Maps PIF API company data to ExternalCompanyDto
    /// </summary>
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
            Country = data.Country ?? "Saudi Arabia", // Default to Saudi Arabia
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

    /// <summary>
    /// Maps PIF representative data to ExternalRepresentativeDto
    /// </summary>
    private ExternalRepresentativeDto? MapRepresentative(PifRepresentativeData? rep)
    {
        if (rep == null)
            return null;

        // Check if representative has any meaningful data
        bool hasData = !string.IsNullOrWhiteSpace(rep.Name) ||
                      !string.IsNullOrWhiteSpace(rep.Email) ||
                      !string.IsNullOrWhiteSpace(rep.Phone) ||
                      !string.IsNullOrWhiteSpace(rep.Mobile);

        if (!hasData)
            return null;

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

    /// <summary>
    /// Converts base64 string to byte array
    /// </summary>
    private byte[]? ConvertBase64ToByteArray(string? base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return null;

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

/// <summary>
/// PIF API wrapped response format
/// </summary>
internal class PifWrappedResponse<T>
{
    public int HttpCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// PIF API company data structure
/// Matches actual PIF API response format
/// </summary>
internal class PifCompanyData
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Logo { get; set; } // Base64 string
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

/// <summary>
/// PIF API representative data structure
/// </summary>
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
