using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetAssetStatusSummaryQueryHandler
: IRequestHandler<GetAssetStatusSummaryQuery, List<AssetStatusSummaryDto>>
{
    private readonly IAssetRepository _repository;
    private readonly ITokenService _tokenService;

    public GetAssetStatusSummaryQueryHandler(IAssetRepository repository, ITokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    public async Task<List<AssetStatusSummaryDto>> Handle(
        GetAssetStatusSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var requestingUser = _tokenService.GetUserName();
        var statusCounts = await _repository.GetStatusCountsAsync(
            request.CompanyId,
            requestingUser,
            cancellationToken);

        var groupedCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var status in Enum.GetValues<AssetStatuses>())
        {
            var displayName = status.GetDisplayName();
            var count = statusCounts.GetValueOrDefault(status, 0);

            groupedCounts[displayName] = groupedCounts.GetValueOrDefault(displayName, 0) + count;
        }

        var orderedDisplayNames = new[]
        {
        "Pending",
        "Pending PC Admin",
        "Pending PIF Review",
        "Completed",
        "Returned"
    };

        var result = new List<AssetStatusSummaryDto>();
        foreach (var name in orderedDisplayNames)
        {
            groupedCounts.TryGetValue(name, out var count);
            result.Add(new AssetStatusSummaryDto
            {
                DisplayName = name,
                Count = count
            });
        }

        var remaining = groupedCounts.Keys
            .Where(name => !orderedDisplayNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);

        foreach (var name in remaining)
        {
            result.Add(new AssetStatusSummaryDto
            {
                DisplayName = name,
                Count = groupedCounts[name]
            });
        }

        return result;
    }
}