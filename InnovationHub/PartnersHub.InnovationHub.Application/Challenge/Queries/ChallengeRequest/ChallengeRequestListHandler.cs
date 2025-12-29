using MediatR;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Application.Common.Models;
using PartnersHub.InnovationHub.Application.Common.Paging;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;

namespace PartnersHub.InnovationHub.Application.Challenge.Queries.ChallengeRequest
{
    public class ChallengeRequestListHandler : IRequestHandler<ChallengeRequestListQuery, Result<PaginatedList<ChallengeCardDTO>>>
    {
        private readonly IChallengeRequestRepository _challengeRequestReadRepo;

        public ChallengeRequestListHandler(IChallengeRequestRepository challengeRequestReadRepo)
        {
            _challengeRequestReadRepo = challengeRequestReadRepo;
        }

        public async Task<Result<PaginatedList<ChallengeCardDTO>>> Handle(
            ChallengeRequestListQuery request,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return await Task.FromCanceled<Result<PaginatedList<ChallengeCardDTO>>>(cancellationToken);

            var data = await _challengeRequestReadRepo.ListAsync(request.Search,
                                                                 request.DevCoId,
                                                                 request.SectorId,
                                                                 request.PriorityLevel,
                                                                 request.IsMyChallenge,
                                                                 request.UserId,
                                                                 request.IsAdmin,
                                                                 request.IsCounts,
                                                                 request.StatusList,
                                                                 request.IsPending,
                                                                 request.PageSize,
                                                                 request.PageNumber,
                                                                 cancellationToken);
           


            var challengeCardDtos = data.Item1.Select(c => new ChallengeCardDTO
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DevCoName = c.SourceCompany?.Name,
                SectorName = c.AssociatedSector?.Name,
                DevCoLogoUrl = "",
                PriorityLevel = (PriorityLevel)c.PriorityLevelId,
                CreatedAt = c.CreatedAt,
                Status = c.IsArchived  == true ? ChallengeStatus.Archived : c.ChallengeStatus,
                ShortId = c.shortId,
                IsArchived = c.IsArchived ?? false
            }).ToList();



            var result = PaginatedList<ChallengeCardDTO>.Create(challengeCardDtos, data.TotalCount, request.PageNumber, request.PageSize);
            return Result<PaginatedList<ChallengeCardDTO>>.Success(result);


        }
    }
}
