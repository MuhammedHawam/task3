using PartnersHub.Synergy.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.Interfaces.Integration;

public interface IMiddlewareIntegrationService
{
    Task<FileUploadResult> UploadFilesAsync(FileUploadRequest request,CancellationToken cancellationToken = default);
}
