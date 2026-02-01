using PartnersHub.InfraBase.Application.Common.DTOs;
using PartnersHub.InfraBase.Application.Common.Models;

namespace PartnersHub.InfraBase.Application.Common.Interfaces;

public interface IMiddlewareIntegrationService
{
    Task<MiddlewareCompany?> GetCompanyByIdAsync(Guid companyId);
    Task<FileUploadResult> UploadFilesAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default);
}
