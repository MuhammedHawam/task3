using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.Models;

public sealed record FileUploadContent(
    string FileName,
    string ContentType,
    long Length,
    [property: JsonIgnore] Func<Stream> OpenReadStream)
{
    [JsonIgnore]
    public bool IsEmpty => Length <= 0;
}

public sealed record FileUploadRequest(
    string ReferenceId,
    Guid CompanyId,
    Guid ContactId,
    string Description,
    IReadOnlyCollection<FileUploadContent> Files);

public sealed record FileUploadResult(
    bool Success,
    string? Message,
    IReadOnlyList<FileUploadItem> UploadedFiles);

public sealed record FileUploadItem(
    string FileName,
    string SharePointUrl,
    long FileSize,
    bool Uploaded,
    string? Status,
    DateTime UploadedOn);

public sealed record DocumentInfo(
    string DocumentName,
    string DocumentContent,
    string DocumentPath);
