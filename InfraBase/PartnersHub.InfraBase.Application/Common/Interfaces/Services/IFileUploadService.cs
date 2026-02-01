using PartnersHub.InfraBase.Application.Common.Models;

namespace PartnersHub.InfraBase.Application.Common.Interfaces.Services;

public interface IFileUploadService
{
    Task<FileUploadResult> UploadFilesAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default);
}
