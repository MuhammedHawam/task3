using MediatR;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Paging;


namespace PartnersHub.InnovationHub.Application.Challenge.Queries;

public record ChallengewithCampaignListQuery(List<Guid> copmanyIds,
                                             int PageSize = 8,
                                             int PageNumber = 1
                                             ) : IRequest<(PagingResult<ChallengeCompanyDTO>, PagingResult<CampaignCompanyDTO>)>;

