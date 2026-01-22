using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Application.Middleware.DTOs;
using PartnersHub.ConfigurationHub.Application.Middleware.Interfaces;
using System.Net.Http.Json;

namespace PartnersHub.ConfigurationHub.Infrastructure.Services;

public class MiddlewareCompanyService : IMiddlewareCompanyService
{
    private const int DefaultAllCompaniesPageSize = 200;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MiddlewareCompanyService> _logger;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public MiddlewareCompanyService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MiddlewareCompanyService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["MiddlewareApi:BaseUrl"] ?? throw new InvalidOperationException("MiddlewareApi:BaseUrl configuration is missing");
        _apiKey = configuration["MiddlewareApi:ApiKey"] ?? throw new InvalidOperationException("MiddlewareApi:ApiKey configuration is missing");

        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);
    }

    public async Task<PaginatedList<MiddlewareCompanyDto>> GetCompaniesAsync(MiddlewareCompanyRequestDto request)
    {
        try
        {
            _logger.LogInformation("Fetching companies from middleware with page {PageNumber}, size {PageSize}", 
                request.PageNumber, request.PageSize);

            var response = await _httpClient.PostAsJsonAsync("/Companies/get-companies", request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Unauthorized access to middleware API. Check API key configuration.");
                    throw new HttpRequestException($"Unauthorized access to middleware API. Status: {response.StatusCode}");
                }

                _logger.LogError("Failed to fetch companies. Status: {StatusCode}, Reason: {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);

                throw new HttpRequestException($"Failed to fetch companies from middleware API. Status: {response.StatusCode}");
            }

            var wrappedResponse = await response.Content.ReadFromJsonAsync<MiddlewareWrappedResponse<MiddlewareCompanyResponseDto>>();

            if (wrappedResponse?.Data == null)
            {
                _logger.LogWarning("Companies returned null data from middleware");
                return new PaginatedList<MiddlewareCompanyDto>(new List<MiddlewareCompanyDto>(), 0, request.PageNumber, request.PageSize);
            }

            _logger.LogInformation("Successfully fetched {Count} companies from middleware", 
                wrappedResponse.Data.Companies?.Count ?? 0);

            return new PaginatedList<MiddlewareCompanyDto>(
                wrappedResponse.Data.Companies ?? new List<MiddlewareCompanyDto>(),
                wrappedResponse.Data.TotalCount,
                wrappedResponse.Data.PageNumber,
                wrappedResponse.Data.PageSize);
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

    public async Task<List<MiddlewareCompanyDto>> GetAllCompaniesAsync()
    {
        try
        {
            var request = new MiddlewareCompanyRequestDto
            {
                PageNumber = 1,
                PageSize = DefaultAllCompaniesPageSize
            };

            var firstPage = await GetCompaniesAsync(request);
            var companies = new List<MiddlewareCompanyDto>(firstPage.Items);

            if (firstPage.TotalCount <= firstPage.PageSize)
            {
                return companies;
            }

            var totalPages = (int)Math.Ceiling((double)firstPage.TotalCount / request.PageSize);
            for (var pageNumber = 2; pageNumber <= totalPages; pageNumber++)
            {
                request.PageNumber = pageNumber;
                var page = await GetCompaniesAsync(request);
                companies.AddRange(page.Items);
            }

            return companies;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching all companies from middleware");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all companies from middleware");
            throw;
        }
    }

    public async Task<MiddlewareCompanyDto?> GetCompanyByIdAsync(Guid companyId)
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
                    _logger.LogError("Unauthorized access to middleware API for company {CompanyId}. Check API key configuration.", companyId);
                    throw new HttpRequestException($"Unauthorized access to middleware API. Status: {response.StatusCode}");
                }

                _logger.LogError("Failed to fetch company {CompanyId}. Status: {StatusCode}, Reason: {ReasonPhrase}",
                    companyId, response.StatusCode, response.ReasonPhrase);

                throw new HttpRequestException($"Failed to fetch company from middleware API. Status: {response.StatusCode}");
            }

            var wrappedResponse = await response.Content.ReadFromJsonAsync<MiddlewareWrappedResponse<MiddlewareCompanyDto>>();

            if (wrappedResponse?.Data == null)
            {
                _logger.LogWarning("Company {CompanyId} returned null data from middleware", companyId);
                return null;
            }

            _logger.LogInformation("Successfully fetched company {CompanyId} ({CompanyName}) from middleware",
                companyId, wrappedResponse.Data.Name);

            return wrappedResponse.Data;
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

    public async Task<List<MiddlewareSectorDto>> GetSectorsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching sectors from middleware");

            var response = await _httpClient.GetAsync("/Companies/get-sectors");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogError("Unauthorized access to middleware API. Check API key configuration.");
                    throw new HttpRequestException($"Unauthorized access to middleware API. Status: {response.StatusCode}");
                }

                _logger.LogError("Failed to fetch sectors. Status: {StatusCode}, Reason: {ReasonPhrase}",
                    response.StatusCode, response.ReasonPhrase);

                throw new HttpRequestException($"Failed to fetch sectors from middleware API. Status: {response.StatusCode}");
            }

            var wrappedResponse = await response.Content.ReadFromJsonAsync<MiddlewareWrappedResponse<List<MiddlewareSectorDto>>>();

            if (wrappedResponse?.Data == null)
            {
                _logger.LogWarning("Sectors returned null data from middleware");
                return new List<MiddlewareSectorDto>();
            }

            _logger.LogInformation("Successfully fetched {Count} sectors from middleware", wrappedResponse.Data.Count);

            return wrappedResponse.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching sectors from middleware");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching sectors from middleware");
            throw;
        }
    }
}

internal class MiddlewareWrappedResponse<T>
{
    public int HttpCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string? Error { get; set; }
}
