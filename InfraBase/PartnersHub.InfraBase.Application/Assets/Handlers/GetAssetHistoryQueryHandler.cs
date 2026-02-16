using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetHistoryQueryHandler : IRequestHandler<GetAssetHistoryQuery, List<AssetHistoryDto>>
{
    private readonly IAssetRepository _repository;

    public GetAssetHistoryQueryHandler(IAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AssetHistoryDto>> Handle(GetAssetHistoryQuery query, 
        CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdWithDetailsAsync(query.AssetId, cancellationToken);
        
        if (asset == null)
            return new List<AssetHistoryDto>();

        return asset.History
            .OrderBy(h => h.PerformedAt)
            .Select(h => new AssetHistoryDto
            {
                Id = h.Id,
                Status = h.Status,
                StatusDisplayName = h.Status.GetDisplayName(),
                StatusShortDisplayName = h.Status.GetShortDisplayName(),
                Action = h.Action,
                PerformedBy = h.PerformedBy,
                PerformedAt = h.PerformedAt,
                Comments = h.Comments
            })
            .ToList();
    }
}
