using MediatR;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Application.Campaign.Commands;

public record CreateCampaignCommandHandler(
              IUnitOfWork _unitOfWork,
              ICurrentUserService _userService,
              ICampaignRequestRepository _campaignRequestRepository,
              ICampaignRequestLinkedChallengeRepository _campaignRequestLinkedChallengeRepository,
              ICampaignRequestSponsorRepository _campaignRequestSponsorRepository,
              ICampaignRequestTermsAndConditionRepository _campaignRequestTermsAndConditionRepository,
              INotificationService _notificationService
                           ) : IRequestHandler<CreateCampaignCommand, Results<Guid>>
{



    public async Task<Results<Guid>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate name
        if (await _campaignRequestRepository.ExistsByNameAsync(request.CampaignName, null, cancellationToken))
            return Results<Guid>.Failure("A Campaign with this name already exists");



        // Create the Campaign request using factory method
        var createResult = CampaignRequest.CreateCampaign(request.SubmitterId,
                                                  request.CampaignName,
                                                  request.Description,
                                                  request.ProblemStatement,
                                                  request.Type,
                                                  request.SubmitterName,
                                                  request.LaunchDate,
                                                  request.SubmissionDeadlineDate,
                                                  request.IsDraft == true ? CampaignRequestStatus.Draft :  CampaignRequestStatus.PendingReview,
                                                  request.EvaluatorList.Select(el => (Id: el.id, Name: el.name)).ToList(),
                                                  request.SponsorsList.Select(el => (Id: el.id, Name: el.name)).ToList(),
                                                  request.EvaluationCriteriaList.Select(el => (name: el.name,value: el.value)).ToList(),
                                                  request.LinkedDevCoChallenges,
                                                  string.IsNullOrWhiteSpace(request.SubmitterEmail) ? "con-mabdelkareem@pif.gov.sa" : request.SubmitterEmail);

        // Return early if creation failed
        if (createResult.IsFailure)
            return Results<Guid>.Failure(createResult.Error!);



        // Persist to database
        await _campaignRequestRepository.AddAsync(createResult.Value!, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _notificationService.SendCampaignSubmittedNotificationAsync(createResult.Value!.Id, request.CampaignName);

        return Results<Guid>.Success(createResult.Value!.Id);
    }
}
