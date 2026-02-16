using MediatR;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Common.Exceptions;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class SubmitAssetCommandHandler : IRequestHandler<SubmitAssetCommand, string>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IUserDisplayNameService _userDisplayNameService;

    public SubmitAssetCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IUserDisplayNameService userDisplayNameService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _userDisplayNameService = userDisplayNameService;
    }

    public async Task<string> Handle(SubmitAssetCommand command, CancellationToken cancellationToken)
    {
        var actorDisplayName = await _userDisplayNameService.ResolveDisplayNameAsync(
            command.ContactId,
            cancellationToken: cancellationToken);
        var isPcAdmin = command.UserType == UserType.PcAdmin || _tokenService.IsPcAdmin();

        var asset = await _repository.GetByIdWithFinancialsAsync(command.Id, cancellationToken);
        
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var nextNumber = await _repository.GetNextAssetNumberAsync(cancellationToken);
        var assetCode = $"Infra-{nextNumber:D6}";

        var submitResult = asset.Submit(actorDisplayName, assetCode, isPcAdmin);
        if (submitResult.IsFailure)
        {
            throw new ValidationException(submitResult.Error!);
        }
        if (command.UserType == UserType.InfraAdmin)
        {
            var approveResult = asset.CheckByInfrabaseAdmin(actorDisplayName);
            if (approveResult.IsFailure)
            {
                throw new ValidationException(approveResult.Error!);
            }
        }
        //else if (isPcAdmin)
        //{
        //    var approveResult = asset.AcceptByPcAdmin(userName);
        //    if (approveResult.IsFailure)
        //    {
        //        throw new ValidationException(approveResult.Error!);
        //    }
        //}


        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return asset.AssetCode!;
    }
}
