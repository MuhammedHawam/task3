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
    private readonly IUserDisplayNameService _userDisplayNameService;

    public AddAssetAttachmentCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        IUserDisplayNameService userDisplayNameService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _userDisplayNameService = userDisplayNameService;
    }

    public async Task<Guid> Handle(AddAssetAttachmentCommand command, CancellationToken cancellationToken)
    {
        var actorDisplayName = await _userDisplayNameService.ResolveDisplayNameAsync(
            cancellationToken: cancellationToken);

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
            actorDisplayName);

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
