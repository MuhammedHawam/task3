using MediatR;
using Microsoft.Extensions.Options;
using PartnersHub.InfraBase.Application.Assets.Queries;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Application.Common.Options;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class GetNextAssetCodeQueryHandler : IRequestHandler<GetNextAssetCodeQuery, string>
{
    private readonly IAssetRepository _repository;
    private readonly AssetCodeSettings _assetCodeSettings;

    public GetNextAssetCodeQueryHandler(
        IAssetRepository repository, 
        IOptions<AssetCodeSettings> assetCodeSettings)
    {
        _repository = repository;
        _assetCodeSettings = assetCodeSettings.Value;
    }

    public async Task<string> Handle(GetNextAssetCodeQuery query, 
        CancellationToken cancellationToken)
    {
        var nextNumber = await _repository.GetNextAssetNumberAsync(cancellationToken);
        return _assetCodeSettings.GenerateCode(nextNumber);
    }
}
