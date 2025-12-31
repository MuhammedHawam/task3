using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.SuccessStories.Commands;

public class SetSuccessStoryVisibilityCommandHandler
     : IRequestHandler<SetSuccessStoryVisibilityCommand, Result>
{
    private readonly ISuccessStoryRepository _successStoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;

    public SetSuccessStoryVisibilityCommandHandler(
        ISuccessStoryRepository successStoryRepository,
        IUnitOfWork unitOfWork,
        IUserService userService)
    {
        _successStoryRepository = successStoryRepository;
        _unitOfWork = unitOfWork;
        _userService = userService;
    }

    public async Task<Result> Handle(
        SetSuccessStoryVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var story = await _successStoryRepository.GetByIdAsync(request.SuccessStoryId);
        if (story == null)
            return Result.Failure("Success story doesn't exist");

        var result = story.SetVisibility(request.Hide, _userService.CurrentUserId);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        _successStoryRepository.Update(story);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
