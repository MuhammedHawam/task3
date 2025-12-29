using MediatR;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Challenge.Queries.ChallengeRequest;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Application.Common.Models;
using PartnersHub.InnovationHub.Application.Common.Paging;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Campaign.Queries;

public class ActiveCampaignListQueryHandler(ICampaignRequestRepository _campaignRequestRepository) : IRequestHandler<ActiveCampaignListQuery, Result<PaginatedList<ActiveCampaignCardDTO>>>
{
    public async Task<Result<PaginatedList<ActiveCampaignCardDTO>>> Handle(ActiveCampaignListQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return await Task.FromCanceled<Result<PaginatedList<ActiveCampaignCardDTO>>>(cancellationToken);

        var StatusList = ConvertRequestStateToCampaignRequestStatus(request.StatusList);

        var (items, totalCount) = await _campaignRequestRepository.GetActiveCampaignPaginatedAsync(request.CampaignType?.Select(ct => (int)ct).ToList(),
                                                                                    request.CampaignStatus?.Select(ct => (int)ct).ToList(),
                                                                                    StatusList?.Select(ct => (int)ct).ToList(),
                                                                                    request.LaunchDate,
                                                                                    request.SearchTerm,
                                                                                    request.IsMyCampaign,
                                                                                    request.UserId,
                                                                                    request.IsAdmin,
                                                                                    request.IsPending,
                                                                                    request.PageNumber,
                                                                                    request.PageSize, 
                                                                                    cancellationToken);

        var dtos = items.Select(MapToDto).ToList();


        var result = PaginatedList<ActiveCampaignCardDTO>.Create(dtos, totalCount, request.PageNumber, request.PageSize);
        return Result<PaginatedList<ActiveCampaignCardDTO>>.Success(result);
    }


    private ActiveCampaignCardDTO MapToDto(CampaignRequest request)
    {
        return new ActiveCampaignCardDTO
        {
            CampaignId = request.Id,
            CampaignName = request.Name,
            Description = request.Description,
            CampaignType = request.Type,
            SubmissionDeadline = request.SubmissionDeadLine,
            CampaignStatus = (request.LaunchDate > DateTime.Now) ? Domain.Enums.CampaignStatus.Upcoming : 
                                                                   ((request.LaunchDate <= DateTime.Now && (request.SubmissionDeadLine is null || request.SubmissionDeadLine >= DateTime.Now)) ?
                                                                   Domain.Enums.CampaignStatus.Open : (request.LaunchDate == null ? Domain.Enums.CampaignStatus.Upcoming : Domain.Enums.CampaignStatus.Closed)),
            LunchDate = request.LaunchDate,
            Submitter = request.OwnerName,
            ShortId = request.shortId,
            Status = request.CampaignRequestStatus
        };
    }

    private  List<CampaignRequestStatus> ConvertRequestStateToCampaignRequestStatus(List<RequestState> states)
    {
        return states.Select(state => state switch
        {
            RequestState.PendingReview => CampaignRequestStatus.PendingReview,
            RequestState.Approved => CampaignRequestStatus.Published,
            RequestState.Rejected => CampaignRequestStatus.Closed,
            RequestState.Returned => CampaignRequestStatus.Requested,
            _ => throw new InvalidOperationException(),
        }).ToList();
    }
}
