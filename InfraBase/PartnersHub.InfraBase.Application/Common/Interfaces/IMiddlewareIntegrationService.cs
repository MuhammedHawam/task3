using PartnersHub.InfraBase.Application.Common.DTOs;
using PartnersHub.InfraBase.Application.Common.Models;

namespace PartnersHub.InfraBase.Application.Common.Interfaces;

public interface IMiddlewareIntegrationService
{
    Task<MiddlewareCompany?> GetCompanyByIdAsync(Guid companyId);
    Task<MiddlewareContactByIdDto?> GetContactByIdAsync(
        Guid contactId,
        CancellationToken cancellationToken = default);
    Task<FileUploadResult> UploadFilesAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default);
    Task<DocumentInfo?> DownloadDocumentAsync(
        string sourceFilePath,
        CancellationToken cancellationToken = default);
}
