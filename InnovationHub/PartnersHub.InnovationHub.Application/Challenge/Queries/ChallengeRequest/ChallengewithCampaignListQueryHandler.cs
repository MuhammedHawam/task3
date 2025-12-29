using MediatR;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Application.Common.Paging;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Challenge.Queries;

public class ChallengewithCampaignListQueryHandler(IChallengeRequestRepository _challengeRequestReadRepo,
                                                   ICampaignRequestRepository _campaignRequestRepository) 
                                                  : IRequestHandler<ChallengewithCampaignListQuery, (PagingResult<ChallengeCompanyDTO>, PagingResult<CampaignCompanyDTO>)>
{
    public async Task<(PagingResult<ChallengeCompanyDTO>, PagingResult<CampaignCompanyDTO>)> Handle(
            ChallengewithCampaignListQuery request,
            CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return await Task.FromCanceled<(PagingResult<ChallengeCompanyDTO>, PagingResult<CampaignCompanyDTO>)>(cancellationToken);

        var challengeData = await _challengeRequestReadRepo.GetByCompanyId(request.copmanyIds, request.PageNumber, request.PageSize, cancellationToken);
        var campaignDataDTOs = await _campaignRequestRepository.GetByIdsAsync(challengeData.CampaignIds, request.PageNumber, request.PageSize, cancellationToken);

        var challengeCardDTOs = MapChallengeToDto(challengeData.Items.ToList());
        var campaignCardDTOs = MapCampaignToDto(campaignDataDTOs.Items.ToList(), challengeCardDTOs);

        return (new PagingResult<ChallengeCompanyDTO>
        {
            Items = challengeCardDTOs,
            TotalCount = challengeData.TotalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        },
           new PagingResult<CampaignCompanyDTO>
       {
           Items = campaignCardDTOs,
           TotalCount = campaignDataDTOs.TotalCount,
           Page = request.PageNumber,
           PageSize = request.PageSize
           });
    }


    private List<ChallengeCompanyDTO> MapChallengeToDto(List<Domain.Aggregates.ChallengeRequest.ChallengeRequest> request)
    {
        return request.Select(cr => new ChallengeCompanyDTO
        {
            Id = cr.Id,
            Name = cr.Name,
            Description = cr.Description,
            DevCoName = cr.SourceCompany.Name,
            SectorName = cr.AssociatedSector.Name,
            PriorityLevel = (PriorityLevel)cr.PriorityLevelId,
            ChallengeStatus = cr.ChallengeStatus,   
            CreatedAt = cr.CreatedAt,
            SourceCompanyId = cr.SourceCompanyId,
        }).ToList();


    }

    private List<CampaignCompanyDTO> MapCampaignToDto(List<CampaignRequest> request,List<ChallengeCompanyDTO> ChallengeList)
    {
        return request.Select(cr => new CampaignCompanyDTO
        {
            CampaignId = cr.Id,
            CampaignName = cr.Name,
            Description = cr.Description,
            SubmissionDeadline = cr.SubmissionDeadLine,
            LunchDate = cr.LaunchDate,
            CampaignType = cr.Type,
            SourceCompanyIds = ChallengeList.Where(e => cr.LinkedChallenges.Select(x => x.ChallengeRequestId).Contains( e.Id) ).Select(z => z.SourceCompanyId).ToList(),
            CampaignStatus = (cr.LaunchDate > DateTime.Now) ? CampaignStatus.Upcoming :
                                                                   ((cr.LaunchDate <= DateTime.Now && cr.SubmissionDeadLine >= DateTime.Now) ?
                                                                     CampaignStatus.Open : CampaignStatus.Closed)
            
        }).ToList();

    }

}
