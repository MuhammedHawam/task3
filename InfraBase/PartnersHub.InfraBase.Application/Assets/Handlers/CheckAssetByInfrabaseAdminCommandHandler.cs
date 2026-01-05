using MediatR;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Common.Exceptions;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class CheckAssetByInfrabaseAdminCommandHandler : IRequestHandler<CheckAssetByInfrabaseAdminCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public CheckAssetByInfrabaseAdminCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<bool> Handle(CheckAssetByInfrabaseAdminCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var result = asset.CheckByInfrabaseAdmin(userName);
        if (result.IsFailure)
        {
            throw new ValidationException(result.Error ?? "Asset check failed.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
