using MediatR;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Challenge.Queries.ChallengeRequest;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Application.Common.Paging;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Campaign.Queries
{
    public class CampaignDetailsHandler : IRequestHandler<CampaignDetailsQuery, CampaignDetailsDTO?>
    {
        private readonly ICampaignRequestRepository _repository;
        private readonly IChallengeRequestRepository _ChallengeRepository;
        private readonly IEvaluatorRepository _EvaluatorRepository; 

        public CampaignDetailsHandler(ICampaignRequestRepository repository, 
                                      IChallengeRequestRepository ChallengeRepository,
                                      IEvaluatorRepository evaluatorRepository)
        {
            _repository = repository;
            _ChallengeRepository = ChallengeRepository;
            _EvaluatorRepository = evaluatorRepository;
        }

        public async Task<CampaignDetailsDTO> Handle(CampaignDetailsQuery query, CancellationToken cancellationToken)
        {

            if (cancellationToken.IsCancellationRequested)
                return await Task.FromCanceled<CampaignDetailsDTO>(cancellationToken);

            var Campaign = await _repository.GetById(query.CampaignId, cancellationToken);
            if (Campaign == null)
                return null;


            var CampaignChallengesIDs = Campaign.LinkedChallenges.Select(x => x.ChallengeRequestId).ToList();
            List<ChallengeRequest> CampaignChallenges = new();

            if (CampaignChallengesIDs.Any())
            {
                CampaignChallenges = await _ChallengeRepository.GetByIDs(CampaignChallengesIDs);
            }


            var evaluatorIds = Campaign.Evaluators.Select(e => e.EvaluatorId).ToList();

            // Assuming you have a method to fetch evaluators' names
            var evaluators = await _EvaluatorRepository.GetByIds(evaluatorIds, cancellationToken);


            return new CampaignDetailsDTO
            {
                Name = Campaign.Name,
                Description = Campaign.Description,
                ProblemStatement = Campaign.ProblemStatement,
                CampaignRequestStatus = Campaign.CampaignRequestStatus,
                Type = Campaign.Type,
                LaunchDate = Campaign.LaunchDate,
                SubmissionDeadLine = Campaign.SubmissionDeadLine,
                OwnerId = Campaign.OwnerId,
                OwnerName = Campaign.OwnerName,
                Comments = Campaign.Comments,
                RemainingTime = Campaign.SubmissionDeadLine.HasValue ? Campaign.SubmissionDeadLine - DateTime.Now : null,



                CampaignSponsers = Campaign.Sponsors?
            .Select(s => new CampaignSponsersDTO
            {
                SponsorId = s.SponsorId,
                SponserName = s.SponserName
            }).ToList() ?? new List<CampaignSponsersDTO>(),


                CampaignEvaluationCriteria = Campaign.EvaluationCriterias?
            .Select(ec => new CampaignEvaluationCriteriaDTO
            {
                CriteriaName = ec.CriteriaName,
                CriteriaValue = ec.CriteriaValue
            }).ToList() ?? new List<CampaignEvaluationCriteriaDTO>(),


                LinkedChallenges = CampaignChallenges?
                .Select(s => new CampaignLinkedChallengesDTO
                {
                    ChallengeId = s.Id,
                    ChallengeName = s.Name,
                    Description = s.Description,
                    SubmitterName = s.SubmitterName,    
                    SectorName = s.AssociatedSector?.Name,
                    PriorityLevel = (PriorityLevel)s.PriorityLevelId,
                    DevCoName = s.SourceCompany?.Name,   
                    CreatedAt = s.CreatedAt

                }).ToList() ?? new List<CampaignLinkedChallengesDTO>(),
                TermsAndConditions = Campaign.TermsAndCondition?.Select(a => new AttachmentDto()
                {
                    Id = a.Id,
                    FileName = a.Metadata.Name,
                    FileSizeInBytes = a.Metadata.SizeInBytes,
                    ContentType = "",
                    SharePointUrl = a.SharePointUrl,
                    UploadedAt = a.UploadedAt
                }).ToList() ?? new List<AttachmentDto>(),
                EvaluatorList = Campaign.Evaluators?.Select(e => new CampaignEvaluatorDto()
                {
                    EvaluatorId = e.EvaluatorId,
                    EvaluatorName = evaluators.FirstOrDefault(ev => ev.Id == e.EvaluatorId)?.NameEn

                }).ToList() ?? new List<CampaignEvaluatorDto>()
            };

        }
    }
}
