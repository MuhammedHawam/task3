using MediatR;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Common.Exceptions;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class AddAssetAttachmentCommandHandler : IRequestHandler<AddAssetAttachmentCommand, Guid>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public AddAssetAttachmentCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<Guid> Handle(AddAssetAttachmentCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdAsync(command.AssetId, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.AssetId);
        }

        var attachmentResult = asset.AddAttachment(
            command.FileName, 
            command.FileSizeInBytes, 
            command.ContentType, 
            command.SharePointUrl, 
            userName);

        if (attachmentResult.IsFailure)
        {
            throw new ValidationException(attachmentResult.Error!);
        }

        if (attachmentResult.Value == null)
        {
            throw new InvalidOperationException("Attachment value is null");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return attachmentResult.Value.Id;
    }
}
