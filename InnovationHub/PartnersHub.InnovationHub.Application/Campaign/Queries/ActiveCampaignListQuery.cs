using MediatR;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Models;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Application.Campaign.Queries;

public record ActiveCampaignListQuery : IRequest<Result<PaginatedList<ActiveCampaignCardDTO>>>
{
    public List<CampaignType>? CampaignType { get; set; }
    public List<CampaignStatus>? CampaignStatus { get; set; }
    public List<RequestState>? StatusList { get; set; }
    public DateTime? LaunchDate { get; set; }
    public Guid? UserId { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsMyCampaign { get; set; }
    public bool? IsAdmin { get; set; }
    public bool? IsPending { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
}

