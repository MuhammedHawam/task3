using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;


namespace PartnersHub.Synergy.Application.SuccessStories.Commands;

public class UpdateSuccessStoryCommandHandler(ISuccessStoryRepository _successStoryRepository,
                                              IUnitOfWork _unitOfWork,
                                              ISynergyCompanyRepository _synergyCompanyRepository
    )
                                              : IRequestHandler<UpdateSuccessStoryCommand, Result>
{

    public async Task<Result> Handle(UpdateSuccessStoryCommand request, CancellationToken cancellationToken)
    {
        var successStory = await _successStoryRepository.GetByIdAsync(request.Id);
        if (successStory == null)
            return Result.Failure("Success story not found");

        var isTitleExist = await _successStoryRepository.CheckTitleUniqueness(request.Title, request.Id);
        if (isTitleExist)
            return Result.Failure("A success story with this title already exists.");

        var updateResult = successStory.Update(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.SuccessStoryTypeId,
            request.SuccessStoryCollaborationStatusId,
            request.IsAdmin);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

}
