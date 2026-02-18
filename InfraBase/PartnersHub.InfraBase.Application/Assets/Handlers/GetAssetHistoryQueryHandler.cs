using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetHistoryQueryHandler : IRequestHandler<GetAssetHistoryQuery, List<AssetHistoryDto>>
{
    private readonly IAssetRepository _repository;
    private readonly IAssetSubmittedByResolver _assetSubmittedByResolver;

    public GetAssetHistoryQueryHandler(
        IAssetRepository repository,
        IAssetSubmittedByResolver assetSubmittedByResolver)
    {
        _repository = repository;
        _assetSubmittedByResolver = assetSubmittedByResolver;
    }

    public async Task<List<AssetHistoryDto>> Handle(GetAssetHistoryQuery query, 
        CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdWithDetailsAsync(query.AssetId, cancellationToken);
        
        if (asset == null)
            return new List<AssetHistoryDto>();

        var performedByNames = await _assetSubmittedByResolver.ResolveUserValuesAsync(
            asset.History.Select(h => h.PerformedBy),
            cancellationToken);

        return asset.History
            .OrderBy(h => h.PerformedAt)
            .Select(h =>
            {
                var resolvedPerformedBy = ResolvePerformedBy(h.PerformedBy, performedByNames);
                return new AssetHistoryDto
                {
                    Id = h.Id,
                    Status = h.Status,
                    StatusDisplayName = h.Status.GetDisplayName(),
                    StatusShortDisplayName = h.Status.GetShortDisplayName(),
                    Action = h.Action,
                    PerformedBy = resolvedPerformedBy,
                    PerformedByName = resolvedPerformedBy,
                    PerformedAt = h.PerformedAt,
                    Comments = h.Comments
                };
            })
            .ToList();
    }

    private static string ResolvePerformedBy(
        string performedBy,
        IReadOnlyDictionary<string, string?> performedByNames)
    {
        var normalizedPerformedBy = string.IsNullOrWhiteSpace(performedBy)
            ? performedBy
            : performedBy.Trim();

        if (!string.IsNullOrWhiteSpace(normalizedPerformedBy) &&
            performedByNames.TryGetValue(normalizedPerformedBy, out var resolvedPerformedBy) &&
            !string.IsNullOrWhiteSpace(resolvedPerformedBy))
        {
            return resolvedPerformedBy;
        }

        return string.IsNullOrWhiteSpace(normalizedPerformedBy) ? performedBy : normalizedPerformedBy;
    }
}
