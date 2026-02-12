using PartnersHub.InnovationHub.Application.Models;


namespace PartnersHub.InnovationHub.Application.Common.Interfaces.Integration;

public interface IMiddlewareIntegrationService
{
    Task<FileUploadResult> UploadFilesAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
}
