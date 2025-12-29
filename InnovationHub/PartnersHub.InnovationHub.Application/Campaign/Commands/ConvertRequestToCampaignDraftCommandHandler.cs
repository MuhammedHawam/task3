using MediatR;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Application.Campaign.Commands;

public class ConvertRequestToCampaignDraftCommandHandler(
                                  IUnitOfWork _unitOfWork,
                                  ICampaignRequestRepository _campaignRequestRepository,
                                  ICampaignRequestLinkedChallengeRepository _campaignRequestLinkedChallengeRepository,
                                  IAttachmentRepository _attachmentRepository,
                                  ICampaignRequestSponsorRepository _CampaignRequestSponsorRepository,
                                  ICampaignRequestEvaluatorRepository _CampaignRequestEvaluatorRepository,
                                  ICampaignRequestEvaluationCriteriaRepository _CampaignRequestEvaluationCriteriaRepository
                                  ) : IRequestHandler<ConvertRequestToCampaignDraftCommand, CampaignRequestStatus>
{
    public async Task<CampaignRequestStatus> Handle(ConvertRequestToCampaignDraftCommand request, CancellationToken cancellationToken)
    {
        try
        {

            if (cancellationToken.IsCancellationRequested)
                return await Task.FromCanceled<CampaignRequestStatus>(cancellationToken);

            var Campaign = await _campaignRequestRepository.GetById(request.CampaignRequestId, cancellationToken);


            if (Campaign == null)
                return await Task.FromCanceled<CampaignRequestStatus>(cancellationToken);



            Campaign.Update(request.CampaignName, 
                            request.CampaignDescription, 
                            request.type, 
                            request.LaunchDate,
                            request.status);



            request.LinkedChallenges.ForEach(l =>
            {
                Campaign.LinkChallenge(l);
            });


            request.Sponsers.ForEach(s => {
                Campaign.AddSponsor(s.SponsorId, s.SponserName);
            });

            request.Evaluators.ForEach(e =>
            {
                Campaign.AddEvaluator(e);
            });

            request.CriteriaWeight.ForEach(c => {
                Campaign.AddOrUpdateEvaluationCriteria(c.CriteriaName, c.CriteriaValue);
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Campaign.CampaignRequestStatus;
        }
        catch(Exception ex)
        {
            return await Task.FromCanceled<CampaignRequestStatus>(cancellationToken);
        }
    }

}
