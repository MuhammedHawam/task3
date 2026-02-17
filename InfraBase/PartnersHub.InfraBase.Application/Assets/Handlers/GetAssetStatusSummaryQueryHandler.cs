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

        var groupedCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // "Pending" currently represents Draft in the UI.
            ["Pending"] = statusCounts.GetValueOrDefault(AssetStatuses.Draft, 0),
            ["Pending PC Admin"] = statusCounts.GetValueOrDefault(AssetStatuses.Submitted, 0),
            ["Pending PIF Review"] = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByPcAdmin, 0),
            ["Completed"] = statusCounts.GetValueOrDefault(AssetStatuses.AcceptedByInfrabase, 0),
            // Returned must include both rejection statuses.
            ["Returned"] = statusCounts.GetValueOrDefault(AssetStatuses.RejectedByPcAdmin, 0) +
                           statusCounts.GetValueOrDefault(AssetStatuses.RejectedByInfrabase, 0)
        };

        var handledStatuses = new HashSet<AssetStatuses>
        {
            AssetStatuses.Draft,
            AssetStatuses.Submitted,
            AssetStatuses.AcceptedByPcAdmin,
            AssetStatuses.AcceptedByInfrabase,
            AssetStatuses.RejectedByPcAdmin,
            AssetStatuses.RejectedByInfrabase
        };

        // Keep forward compatibility if new statuses are added later.
        foreach (var kvp in statusCounts.Where(kvp => !handledStatuses.Contains(kvp.Key)))
        {
            var displayName = kvp.Key.GetDisplayName();
            groupedCounts[displayName] = groupedCounts.GetValueOrDefault(displayName, 0) + kvp.Value;
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