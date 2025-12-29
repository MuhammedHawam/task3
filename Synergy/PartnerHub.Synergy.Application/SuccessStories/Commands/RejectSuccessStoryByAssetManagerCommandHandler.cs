using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SuccessStories.Commands
{
    public class RejectSuccessStoryByAssetManagerCommandHandler : IRequestHandler<RejectSuccessStoryByAssetManagerCommand, Result>
    {
        private readonly ISuccessStoryRepository _successStoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        
        public RejectSuccessStoryByAssetManagerCommandHandler(
            ISuccessStoryRepository successStoryRepository,
            IUnitOfWork unitOfWork, 
            IUserService userService)
        {
            _userService = userService;
            _successStoryRepository = successStoryRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Result> Handle(RejectSuccessStoryByAssetManagerCommand request, CancellationToken cancellationToken)
        {
            var successStory = await _successStoryRepository.GetByIdAsync(request.SuccessStoryId);
            if (successStory == null)
                return Result.Failure("Success Story doesn't exist");

            var result = successStory.RejectByAssetManager(_userService.CurrentUserId, request.RejectionReason);
            if (result.IsFailure)
                return Result.Failure(result.Error);

            _successStoryRepository.Update(successStory);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Result.Success();
        }
    }
}
