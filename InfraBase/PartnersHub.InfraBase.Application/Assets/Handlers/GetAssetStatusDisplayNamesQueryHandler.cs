using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetStatusDisplayNamesQueryHandler
    : IRequestHandler<GetAssetStatusDisplayNamesQuery, List<AssetStatusDisplayDto>>
{
    public Task<List<AssetStatusDisplayDto>> Handle(
        GetAssetStatusDisplayNamesQuery request,
        CancellationToken cancellationToken)
    {
        var statuses = Enum.GetValues<AssetStatuses>()
            .Select(status => new AssetStatusDisplayDto
            {
                Status = status,
                DisplayName = status.GetDisplayName(),
                ShortDisplayName = status.GetShortDisplayName()
            })
            .ToList();

        return Task.FromResult(statuses);
    }
}
