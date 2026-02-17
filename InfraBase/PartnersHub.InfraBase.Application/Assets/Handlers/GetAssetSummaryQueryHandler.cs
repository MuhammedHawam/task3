using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetSummaryQueryHandler : IRequestHandler<GetAssetSummaryQuery, AssetSummaryDto>
{
    private readonly IAssetRepository _repository;
    private readonly ITokenService _tokenService;

    public GetAssetSummaryQueryHandler(IAssetRepository repository, ITokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    public async Task<AssetSummaryDto> Handle(GetAssetSummaryQuery query, 
        CancellationToken cancellationToken)
    {
        var requestingUser = _tokenService.GetUserName();
        var statusCounts = await _repository.GetStatusCountsAsync(
            query.CompanyId,
            requestingUser,
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
            ReturnedForCorrectionAssets = statusCounts.GetValueOrDefault(AssetStatuses.RejectedByPcAdmin, 0) +
                                          statusCounts.GetValueOrDefault(AssetStatuses.RejectedByInfrabase, 0)
        };
    }
}
