using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PartnersHub.ConfigurationHub.Infrastructure.Presistence.Services;

public static class ScimApiConstants
{
    public const string ClientName = "CiamScimClient";
    public const string UsersPath = "/scim2/Users";
    public const string QueryParams = "?excludedAttributes=groups%2Croles&domain=PRIMARY";
}

public class ScimUserService : IScimUserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor; 
    private readonly ILogger<ScimUserService> _logger; 

    public ScimUserService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ScimUserService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<List<SimpleUser>> GetUsersAsync()
    {
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Authorization header is missing or not in Bearer format.");
            throw new UnauthorizedAccessException("Missing or invalid authorization token in the current request.");
        }

        using var httpClient = _httpClientFactory.CreateClient(ScimApiConstants.ClientName);

        httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

        if (!httpClient.DefaultRequestHeaders.Accept.Any())
        {
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        var requestUri = ScimApiConstants.UsersPath + ScimApiConstants.QueryParams;

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(requestUri);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while calling SCIM endpoint at {Uri}", requestUri);
            throw; 
        }

        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var scimResponse = await JsonSerializer.DeserializeAsync<ScimUserListResponse>(responseStream, options);

        if (scimResponse?.Resources == null)
        {
            _logger.LogWarning("SCIM response received but resources array was null or empty.");
            return new List<SimpleUser>();
        }

        var simpleUsers = scimResponse.Resources
            .Select(r => new SimpleUser
            {
                Name = $"{r.Name?.GivenName} {r.Name?.FamilyName}".Trim(),
                Email = r.Emails?.FirstOrDefault() ?? string.Empty,
                RoleId = r.CustomExtension?.RoleIds,
                UserId = r.CustomExtension?.ContactId==null? null:Guid.Parse(r.CustomExtension.ContactId)
            })
            .ToList();

        return simpleUsers;
    }
}