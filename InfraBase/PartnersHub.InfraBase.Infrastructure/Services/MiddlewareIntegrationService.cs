using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Common.DTOs;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PartnersHub.InfraBase.Infrastructure.Services;

public class MiddlewareIntegrationService : IMiddlewareIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MiddlewareIntegrationService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _baseUrl;

    public MiddlewareIntegrationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MiddlewareIntegrationService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient("MiddlewareApi");
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _baseUrl = configuration["MiddlewareApi:BaseUrl"] 
            ?? throw new InvalidOperationException("MiddlewareApi:BaseUrl configuration is missing");
    }

    public async Task<MiddlewareCompany?> GetCompanyByIdAsync(Guid companyId)
    {
        try
        {
            _logger.LogInformation("Fetching company {CompanyId} from middleware", companyId);

            var token = GetAuthorizationToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("Authorization header added to middleware request");
            }
            else
            {
                _logger.LogWarning("No authorization token found in current request context");
            }

            var endpoint = $"{_baseUrl}/Networking/get-networking-company-by-id?companyId={companyId}";
            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Company {CompanyId} request failed. Status: {StatusCode}, Reason: {ReasonPhrase}", 
                    companyId, response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var middlewareResponse = await response.Content.ReadFromJsonAsync<MiddlewareCompanyResponse>();
            
            if (middlewareResponse?.Data == null || 
                middlewareResponse.HttpCode != 200 || 
                middlewareResponse.Status != "Success")
            {
                _logger.LogWarning("Invalid response for company {CompanyId}. HttpCode: {HttpCode}, Status: {Status}", 
                    companyId, middlewareResponse?.HttpCode, middlewareResponse?.Status);
                return null;
            }

            _logger.LogInformation("Successfully fetched company {CompanyName} (ID: {CompanyId})", 
                middlewareResponse.Data.Name, companyId);
            return middlewareResponse.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching company {CompanyId}. Exception: {Message}", 
                companyId, ex.Message);
            return null;
        }
    }

    public async Task<List<PortfolioCompanyDto>> SearchPortfolioCompaniesAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching portfolio companies with search term: {SearchTerm}", searchTerm);

            var token = GetAuthorizationToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var endpoint = $"{_baseUrl}/Networking/get-networking-companies";
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                endpoint += $"?searchTerm={Uri.EscapeDataString(searchTerm)}";
            }

            var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Portfolio companies search request failed. Status: {StatusCode}, Reason: {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                return new List<PortfolioCompanyDto>();
            }

            var middlewareResponse = await response.Content.ReadFromJsonAsync<MiddlewareCompanyListResponse>(cancellationToken: cancellationToken);
            
            if (middlewareResponse?.Data == null || 
                middlewareResponse.HttpCode != 200 || 
                middlewareResponse.Status != "Success")
            {
                _logger.LogWarning("Invalid response for portfolio companies search. HttpCode: {HttpCode}, Status: {Status}", 
                    middlewareResponse?.HttpCode, middlewareResponse?.Status);
                return new List<PortfolioCompanyDto>();
            }

            var portfolioCompanies = middlewareResponse.Data.Select(company => new PortfolioCompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                SectorName = company.Sector?.Name,
                CompanyAdminRepresentativeName = company.Representative?.Name,
                CompanyAdminRepresentativeEmail = company.Representative?.Email
            }).ToList();

            _logger.LogInformation("Successfully fetched {Count} portfolio companies", portfolioCompanies.Count);
            return portfolioCompanies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching portfolio companies. Exception: {Message}", ex.Message);
            return new List<PortfolioCompanyDto>();
        }
    }

    private string? GetAuthorizationToken()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader))
        {
            return null;
        }

        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }

        return authHeader;
    }
}
