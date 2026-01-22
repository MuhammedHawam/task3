using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetSummaryQueryHandler : IRequestHandler<GetAssetSummaryQuery, AssetSummaryDto>
{
    private readonly IAssetRepository _repository;

    public GetAssetSummaryQueryHandler(IAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<AssetSummaryDto> Handle(GetAssetSummaryQuery query, 
        CancellationToken cancellationToken)
    {
        var statusCounts = await _repository.GetStatusCountsAsync(query.CompanyId, 
            cancellationToken);

        return new AssetSummaryDto
        {
            TotalAssets = statusCounts.Values.Sum(),
            DraftAssets = statusCounts.GetValueOrDefault(AssetStatuses.Draft, 0),
            SubmittedAssets = statusCounts.GetValueOrDefault(AssetStatuses.Submitted, 0),
            PcAdminApprovedAssets = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByPcAdmin, 0),
            RejectedAssets = statusCounts.GetValueOrDefault(AssetStatuses.RejectedByPcAdmin, 0) +
                             statusCounts.GetValueOrDefault(AssetStatuses.RejectedByInfrabase, 0),
            CheckedAssets = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByInfrabase, 0),
            ReturnedForCorrectionAssets = statusCounts.GetValueOrDefault(
                AssetStatuses.RejectedByInfrabase, 0)
        };
    }
}
