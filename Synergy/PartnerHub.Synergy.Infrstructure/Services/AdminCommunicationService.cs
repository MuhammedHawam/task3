using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PartnersHub.Synergy.Application.Common.Interfaces.Services;
using PartnersHub.Synergy.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Infrastructure.Services
{
    public static class AdminpiConstants
    {
        public const string ClientName = "AdminClient";
    }
    public class AdminCommunicationService : IAdminCommunicationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AdminCommunicationService> _logger;

        public AdminCommunicationService(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AdminCommunicationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<string>> GetUserPermissions(Guid UserId)
        {
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Authorization header is missing or not in Bearer format.");
                throw new UnauthorizedAccessException("Missing or invalid authorization token in the current request.");
            }

            using var httpClient = _httpClientFactory.CreateClient(AdminpiConstants.ClientName);

            httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

            if (!httpClient.DefaultRequestHeaders.Accept.Any())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }

            var requestUri = $"/api/admin/roles/users/{UserId}/permissions";

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

            var adminResponse = await JsonSerializer.DeserializeAsync<List<string>>(responseStream, options);

            if (adminResponse == null)
            {
                _logger.LogWarning(" response received but resources array was null or empty.");
                return new List<string>();
            }

            return adminResponse;
        }
    }
}
