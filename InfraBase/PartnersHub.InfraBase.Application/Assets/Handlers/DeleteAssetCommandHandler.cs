using MediatR;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Common.Exceptions;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class DeleteAssetCommandHandler : IRequestHandler<DeleteAssetCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAssetCommandHandler(IAssetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteAssetCommand command, CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        _repository.Delete(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
