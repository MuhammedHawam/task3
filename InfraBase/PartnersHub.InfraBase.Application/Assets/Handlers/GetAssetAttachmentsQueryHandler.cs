using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetAttachmentsQueryHandler : IRequestHandler<GetAssetAttachmentsQuery, List<AssetAttachmentDto>>
{
    private readonly IAssetRepository _repository;

    public GetAssetAttachmentsQueryHandler(IAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AssetAttachmentDto>> Handle(GetAssetAttachmentsQuery query, 
        CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdWithAttachmentsAsync(query.AssetId, cancellationToken);

        if (asset == null)
            return new List<AssetAttachmentDto>();

        return asset.GetAttachments()
            .Select(a => new AssetAttachmentDto
            {
                Id = a.Id,
                FileName = a.Metadata.FileName,
                FileSizeInBytes = a.Metadata.FileSizeInBytes,
                ContentType = a.Metadata.ContentType,
                SharePointUrl = a.SharePointUrl,
                UploadedBy = a.UploadedBy,
                UploadedAt = a.UploadedAt
            })
            .ToList();
    }
}
