using MediatR;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Common;
using System.Text.Json.Serialization;


namespace PartnersHub.Synergy.Application.Opportunities.Commands;

public record UpdateOpportunityCommand : IRequest<Result>
{
    public Guid OpportunityId { get; init; }

    public string Title { get; init; }
    public string Description { get; init; }
    public int TypeId { get; init; }
    public int ThematicAreaId { get; init; }
    public Guid SectorId { get; init; }
    public string SectorName { get; init; }
    public string CollaborationRationale { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }

    public string? ContactName { get; init; }
    public string? ContactAddress { get; init; }
    public string? ContactMobile { get; init; }

    public List<Guid>? CollaboratedProfiles { get; init; }
    public List<int>? CollaborationRequirements { get; init; }
    public string? CollaborationRequirementOther { get; init; }
    public List<int>? ExpectedOutcomes { get; init; }
    public string? ExpectedOutcomeOther { get; init; }

    public bool? IsAdmin { get; init; }

    public List<Guid>? AttachmentIdsToRemove { get; init; }

    [JsonIgnore]
    public List<FileUploadContent>? FilesToUpload { get; set; }
    public string? AttachmentDescription { get; set; }
}

