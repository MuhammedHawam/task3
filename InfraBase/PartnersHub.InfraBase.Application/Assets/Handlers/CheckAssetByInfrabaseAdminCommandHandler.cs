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
    private readonly IUserDisplayNameService _userDisplayNameService;

    public CheckAssetByInfrabaseAdminCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        IUserDisplayNameService userDisplayNameService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _userDisplayNameService = userDisplayNameService;
    }

    public async Task<bool> Handle(CheckAssetByInfrabaseAdminCommand command, CancellationToken cancellationToken)
    {
        var actorDisplayName = await _userDisplayNameService.ResolveDisplayNameAsync(
            command.ContactId,
            cancellationToken: cancellationToken);

        var asset = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var result = asset.CheckByInfrabaseAdmin(actorDisplayName);
        if (result.IsFailure)
        {
            throw new ValidationException(result.Error ?? "Asset check failed.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
