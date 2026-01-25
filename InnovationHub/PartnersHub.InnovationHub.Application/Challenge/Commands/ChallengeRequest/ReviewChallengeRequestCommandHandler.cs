using MediatR;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;

namespace PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest;

public class ReviewChallengeRequestCommandHandler(
    IReviewChallengeRepository _repository,
    IChallengeRequestRepository _challengeRepository,
    ICurrentUserService _userService,
    IUnitOfWork _unitOfWork,
    INotificationService _NotificationService) : IRequestHandler<ReviewChallengeRequestCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ReviewChallengeRequestCommand request, CancellationToken cancellationToken)
    {
        var challenge = await _challengeRepository.GetById(request.ChallengeRequestId, cancellationToken);

        if (challenge != null && challenge.ChallengeStatus != ChallengeStatus.Pending)
            return Result<bool>.Failure("Challenge status must be pending review");

        if (request.Status != ChallengeStatus.Approved && request.Status != ChallengeStatus.RevisionsRequest)
            return Result<bool>.Failure("Status can only be changed to Approved or Revision Request");

        challenge.ChallengeStatus = request.Status;

        if (request.Status == ChallengeStatus.RevisionsRequest)
        {
            var revisionComment = ChallengeRequestRevisionComment.Create(
                request.ChallengeRequestId,
                request.Comment,
                "",
                DateTime.Now,
                true);
            await _repository.AddAsync(revisionComment, cancellationToken);
        }

        await _challengeRepository.Update(challenge, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (request.Status == ChallengeStatus.Approved )
        {
            _NotificationService.SendChallengeApprovedNotificationAsync(request.ChallengeRequestId, challenge.Name,challenge.UserEmail);
        }
        else if (request.Status == ChallengeStatus.RevisionsRequest)
        {
            _NotificationService.SendChallengeReturnedNotificationAsync(request.ChallengeRequestId,challenge.Name, challenge.UserEmail, request.Comment);

        }

        return Result<bool>.Success(true);
    }
}
