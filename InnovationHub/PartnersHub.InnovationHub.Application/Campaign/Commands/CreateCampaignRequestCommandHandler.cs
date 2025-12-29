using MediatR;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Application.Campaign.Commands;

public record CreateCampaignRequestCommandHandler(
              IUnitOfWork _unitOfWork,
              ICurrentUserService _userService,
              ICampaignRequestRepository _campaignRequestRepository,
              ICampaignRequestLinkedChallengeRepository _campaignRequestLinkedChallengeRepository,
              ICampaignRequestSponsorRepository _campaignRequestSponsorRepository,
              ICampaignRequestTermsAndConditionRepository _campaignRequestTermsAndConditionRepository
                           ) : IRequestHandler<CreateCampaignRequestCommand, Results<Guid>>
{



    public async Task<Results<Guid>> Handle(CreateCampaignRequestCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate name
        if (await _campaignRequestRepository.ExistsByNameAsync(request.CampaignName, null, cancellationToken))
            return Results<Guid>.Failure("A Campaign with this name already exists");



        // Create the Campaign request using factory method
        var createResult = CampaignRequest.Create(request.SubmitterId,
                                                  request.CampaignName,
                                                  request.Description,
                                                  request.ProblemStatement,
                                                  request.Type,
                                                  request.SubmitterName,
                                                  request.LaunchDate,
                                                  request.Comment,
                                                  CampaignRequestStatus.PendingReview,
                                                  request.SponsorsList.Select(el => (Id: el.id, Name: el.name)).ToList(),
                                                  request.LinkedDevCoChallenges,
                                                  request.SubmitterEmail);

        // Return early if creation failed
        if (createResult.IsFailure)
            return Results<Guid>.Failure(createResult.Error!);

        // Persist to database
        await _campaignRequestRepository.AddAsync(createResult.Value!, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Results<Guid>.Success(createResult.Value!.Id);
    }
}
