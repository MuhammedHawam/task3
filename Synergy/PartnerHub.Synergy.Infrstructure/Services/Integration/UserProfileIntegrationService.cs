using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PartnersHub.Synergy.Infrastructure.Services.Integration;

public class UserProfileDataIntegrationService : IUserProfileDataIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UserProfileDataIntegrationService> _logger;
    private readonly IHttpContextAccessor _httpContext;

    public UserProfileDataIntegrationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<UserProfileDataIntegrationService> logger,
        IHttpContextAccessor httpContext)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _httpContext = httpContext;
    }

    public async Task<UserProfileDataDto?> GetUserProfileData()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MiddlewareApi");


            var request = new HttpRequestMessage(HttpMethod.Get, "Synergy/get-user-profile-data-with-sector");
            var token = GetAuthorizationToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("Authorization header added to middleware request");
            }
            else
            {
                _logger.LogWarning("No authorization token found in current request context");
            }

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("User Data are not found in external system");
                    return default;
                }
                throw new HttpRequestException($"Failed to fetch User Date from middleware. Status: {response.StatusCode}");
            }

            var userInfo = await response.Content.ReadFromJsonAsync<DataWrapper<UserProfileDataDto>>();

            if (userInfo == null)
            {
                _logger.LogWarning("user info returned null from external system");
                return default;
            }

            _logger.LogInformation("Successfully fetched user info from external middleware");

            return userInfo.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching sectors from external middleware");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching sectors from external middleware");
            throw;
        }
    }
    private string? GetAuthorizationToken()
    {
        var authHeader = _httpContext.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

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
