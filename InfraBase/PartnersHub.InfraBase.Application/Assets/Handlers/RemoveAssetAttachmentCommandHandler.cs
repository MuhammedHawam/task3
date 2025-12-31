using MediatR;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Common.Exceptions;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class RemoveAssetAttachmentCommandHandler : IRequestHandler<RemoveAssetAttachmentCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RemoveAssetAttachmentCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<bool> Handle(RemoveAssetAttachmentCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdWithDetailsAsync(command.AssetId, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.AssetId);
        }

        var result = asset.RemoveAttachment(command.AttachmentId, userName);
        if (result.IsFailure)
        {
            throw new ValidationException(result.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
