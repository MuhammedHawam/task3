using MediatR;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Models;
using PartnersHub.InnovationHub.Application.Common.Paging;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;

namespace PartnersHub.InnovationHub.Application.Challenge.Queries.ChallengeRequest;

public record ChallengeRequestListQuery: IRequest<Result<PaginatedList<ChallengeCardDTO>>>
{

    public  string? Search  {get; set;}
    public List<Guid>? DevCoId { get; set; }
    public List<Guid>? SectorId { get; set; }
    public List<string>? PriorityLevel { get; set; }
    public bool? IsMyChallenge { get; set; }
    public Guid? UserId { get; set; }
    public bool? IsAdmin { get; set; }
    public bool? IsCounts { get; set; }
    public List<string>? StatusList { get; set; }
    public bool? IsPending { get; set; }    
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}




