
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Common.DTOs;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PartnersHub.InfraBase.Infrastructure.Services;

public class MiddlewareIntegrationService : IMiddlewareIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MiddlewareIntegrationService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _baseUrl;
    private readonly string _apiKey;

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
        _apiKey = configuration["MiddlewareApi:ApiKey"]
            ?? throw new InvalidOperationException("MiddlewareApi:ApiKey configuration is missing");
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);
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

    public async Task<MiddlewareContactByIdDto?> GetContactByIdAsync(
        Guid contactId,
        CancellationToken cancellationToken = default)
    {
        if (contactId == Guid.Empty)
        {
            return null;
        }

        try
        {
            _logger.LogInformation("Fetching contact {ContactId} from middleware", contactId);

            var endpoint = $"{_baseUrl}/contact/get-by-id/{contactId}";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint);

            var token = GetAuthorizationToken();
            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _logger.LogWarning("No authorization token found in current request context for contact lookup.");
            }

            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Contact {ContactId} request failed. Status: {StatusCode}, Reason: {ReasonPhrase}",
                    contactId,
                    response.StatusCode,
                    response.ReasonPhrase);
                return null;
            }

            var contact = await response.Content.ReadFromJsonAsync<MiddlewareContactByIdDto>(
                cancellationToken: cancellationToken);
            if (contact == null)
            {
                _logger.LogWarning("Contact {ContactId} response is empty.", contactId);
                return null;
            }

            return contact;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Contact {ContactId} lookup was canceled.", contactId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contact {ContactId}. Exception: {Message}", contactId, ex.Message);
            return null;
        }
    }

    public async Task<FileUploadResult> UploadFilesAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var files = request.Files?.Where(f => !f.IsEmpty).ToList() ?? [];
            if (files.Count == 0)
            {
                return new FileUploadResult(true, null, Array.Empty<FileUploadItem>());
            }

            var endpoint = $"{_baseUrl}/PartnerHubFiles/upload-request-files";
            using var form = new MultipartFormDataContent();

            foreach (var file in files)
            {
                var fileContent = new StreamContent(file.OpenReadStream());

                var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType;

                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                form.Add(fileContent, "files", file.FileName);
            }

            form.Add(new StringContent(request.ReferenceId), "ReferenceId");
            form.Add(new StringContent(request.CompanyId.ToString()), "CompanyId");
            form.Add(new StringContent(request.ContactId.ToString()), "ContactId");
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                form.Add(new StringContent(request.Description), "Description");
            }

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = form
            };

            var token = GetAuthorizationToken();
            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            }
            catch (OperationCanceledException oce) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(oce, "Upload request was canceled by cancellationToken.");
                return new FileUploadResult(false, "Upload canceled.", Array.Empty<FileUploadItem>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SendAsync failed before receiving response. Endpoint: {Endpoint}",
                    requestMessage.RequestUri?.ToString());

                return new FileUploadResult(false, $"SendAsync failed: {ex.Message}", Array.Empty<FileUploadItem>());
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "File upload failed. Status: {StatusCode}, Reason: {ReasonPhrase}, Body: {Body}",
                    response.StatusCode,
                    response.ReasonPhrase,
                    errorBody);
                return new FileUploadResult(
                    false,
                    $"File upload failed: {(int)response.StatusCode} {response.ReasonPhrase}",
                    Array.Empty<FileUploadItem>());
            }

            var uploadResponse = await response.Content.ReadFromJsonAsync<PartnerHubFilesResponse>(
                cancellationToken: cancellationToken);

            if (uploadResponse == null)
            {
                return new FileUploadResult(false, "File upload response was empty.", Array.Empty<FileUploadItem>());
            }

            var mappedFiles = (uploadResponse.Data?.UploadedFiles ?? [])
                .Select(file => new FileUploadItem(
                    file.FileName ?? string.Empty,
                    file.SharePointUrl ?? string.Empty,
                    file.FileSize,
                    file.Uploaded,
                    file.Status,
                    file.UploadedOn))
                .ToList();

            var success = uploadResponse.HttpCode == 200 &&
                          string.Equals(uploadResponse.Status, "Success", StringComparison.OrdinalIgnoreCase) &&
                          uploadResponse.Data?.Success == true &&
                          mappedFiles.All(f => f.Uploaded);

            if (!success)
            {
                var message = BuildFailureMessage(uploadResponse, mappedFiles);
                return new FileUploadResult(false, message, mappedFiles);
            }

            return new FileUploadResult(true, uploadResponse.Data?.Message, mappedFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading files to middleware.");
            return new FileUploadResult(false, "File upload failed.", Array.Empty<FileUploadItem>());
        }
    }

    public async Task<DocumentInfo?> DownloadDocumentAsync(
        string sourceFilePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            _logger.LogWarning("Download document request rejected because sourceFilePath is empty.");
            return null;
        }

        try
        {
            var endpoint = $"{_baseUrl}/PartnerHubFiles/download?sourceFilePath={Uri.EscapeDataString(sourceFilePath)}";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, endpoint);

            var token = GetAuthorizationToken();
            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(requestMessage, cancellationToken);
            }
            catch (OperationCanceledException oce) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(oce, "Download document request was canceled by cancellationToken.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SendAsync failed before receiving response for download endpoint: {Endpoint}",
                    requestMessage.RequestUri?.ToString());
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Document download failed. Status: {StatusCode}, Reason: {ReasonPhrase}, Body: {Body}",
                    response.StatusCode,
                    response.ReasonPhrase,
                    errorBody);
                return null;
            }

            var downloadResponse = await response.Content.ReadFromJsonAsync<PartnerHubDocumentResponse>(
                cancellationToken: cancellationToken);

            if (downloadResponse?.Data == null ||
                downloadResponse.HttpCode != 200 ||
                !string.Equals(downloadResponse.Status, "Success", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Invalid document download response. HttpCode: {HttpCode}, Status: {Status}, Error: {Error}, Path: {SourceFilePath}",
                    downloadResponse?.HttpCode,
                    downloadResponse?.Status,
                    downloadResponse?.Error,
                    sourceFilePath);
                return null;
            }

            return downloadResponse.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document from middleware. Path: {SourceFilePath}", sourceFilePath);
            return null;
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

    private static string BuildFailureMessage(PartnerHubFilesResponse response, List<FileUploadItem> files)
    {
        var baseMessage = response.Data?.Message ?? "Some files failed to upload.";
        var failures = files
            .Where(f => !f.Uploaded)
            .Select(f => string.IsNullOrWhiteSpace(f.Status)
                ? f.FileName
                : $"{f.FileName}: {f.Status}")
            .ToList();

        return failures.Count == 0 ? baseMessage : $"{baseMessage} {string.Join(" | ", failures)}";
    }

    private sealed class PartnerHubFilesResponse
    {
        [JsonPropertyName("httpCode")]
        public int HttpCode { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("data")]
        public PartnerHubFilesData? Data { get; init; }

        [JsonPropertyName("error")]
        public string? Error { get; init; }
    }

    private sealed class PartnerHubDocumentResponse
    {
        [JsonPropertyName("httpCode")]
        public int HttpCode { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("data")]
        public DocumentInfo? Data { get; init; }

        [JsonPropertyName("error")]
        public string? Error { get; init; }
    }

    private sealed class PartnerHubFilesData
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }

        [JsonPropertyName("uploadedFiles")]
        public List<PartnerHubUploadedFile>? UploadedFiles { get; init; }
    }

    private sealed class PartnerHubUploadedFile
    {
        [JsonPropertyName("fileName")]
        public string? FileName { get; init; }

        [JsonPropertyName("filePath")]
        public string? FilePath { get; init; }

        [JsonPropertyName("sharePointUrl")]
        public string? SharePointUrl { get; init; }

        [JsonPropertyName("fileSize")]
        public long FileSize { get; init; } // long is safest

        [JsonPropertyName("uploaded")]
        public bool Uploaded { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("uploadedOn")]
        public DateTime UploadedOn { get; init; } // handles "...Z" reliably
    }
}
