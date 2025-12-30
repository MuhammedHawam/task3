using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;


namespace PartnersHub.Synergy.Application.SuccessStories.Commands
{
    public class PublishSuccessStoryCommandHandler : IRequestHandler<PublishSuccessStoryCommand, Result>
    {
        private readonly ISuccessStoryRepository _successStoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        
        public PublishSuccessStoryCommandHandler(
            ISuccessStoryRepository successStoryRepository,
            IUnitOfWork unitOfWork, 
            IUserService userService)
        {
            _userService = userService;
            _successStoryRepository = successStoryRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Result> Handle(PublishSuccessStoryCommand request, CancellationToken cancellationToken)
        {
            var successStory = await _successStoryRepository.GetByIdAsync(request.SuccessStoryId);
            if (successStory == null)
                return Result.Failure("Success Story doesn't exist");

            var result = successStory.Publish(_userService.CurrentUserId, successStory.Title.Value, successStory.CompanyName,successStory.UserEmail);
            if (result.IsFailure)
                return Result.Failure(result.Error);

            _successStoryRepository.Update(successStory);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Result.Success();
        }
    }
}
