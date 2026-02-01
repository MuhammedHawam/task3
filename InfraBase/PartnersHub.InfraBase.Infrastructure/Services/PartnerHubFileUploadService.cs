using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Common.Interfaces.Services;
using PartnersHub.InfraBase.Application.Common.Models;

namespace PartnersHub.InfraBase.Infrastructure.Services;

public sealed class PartnerHubFileUploadService : IFileUploadService
{
    private const string UploadPath = "PartnerHubFiles/upload-request-files";
    private readonly HttpClient _httpClient;
    private readonly ILogger<PartnerHubFileUploadService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PartnerHubFileUploadService(
        HttpClient httpClient,
        ILogger<PartnerHubFileUploadService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FileUploadResult> UploadFilesAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Files == null || request.Files.Count == 0)
        {
            return new FileUploadResult(true, null, Array.Empty<FileUploadItem>());
        }

        if (_httpClient.BaseAddress == null)
        {
            throw new InvalidOperationException("PartnerHubFiles base URL is not configured.");
        }

        var files = request.Files.Where(f => !f.IsEmpty).ToList();
        if (files.Count == 0)
        {
            return new FileUploadResult(true, null, Array.Empty<FileUploadItem>());
        }

        using var form = new MultipartFormDataContent();

        foreach (var file in files)
        {
            var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
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

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, UploadPath)
        {
            Content = form
        };
        AddAuthorizationHeader(requestMessage);

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "PartnerHubFiles upload failed: {StatusCode} {Reason}. Body: {Body}",
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

    private void AddAuthorizationHeader(HttpRequestMessage request)
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return;
        }

        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authHeader["Bearer ".Length..].Trim());
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authHeader.Trim());
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
    }

    private sealed class PartnerHubFilesData
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }

        [JsonPropertyName("uploadedFiles")]
        public List<PartnerHubFilesUploadedFile>? UploadedFiles { get; init; }
    }

    private sealed class PartnerHubFilesUploadedFile
    {
        [JsonPropertyName("fileName")]
        public string? FileName { get; init; }

        [JsonPropertyName("sharePointUrl")]
        public string? SharePointUrl { get; init; }

        [JsonPropertyName("fileSize")]
        public long FileSize { get; init; }

        [JsonPropertyName("uploaded")]
        public bool Uploaded { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("uploadedOn")]
        public DateTime UploadedOn { get; init; }
    }
}
