using MediatR;
using Microsoft.AspNetCore.Http;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.Opportunities.DTOs;
using PartnersHub.Synergy.Domain.Common;
using System.Text.Json.Serialization;


public record CreateOpportunityCommand : IRequest<Result<Guid>>
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int TypeId { get; set; }
    public int ThematicAreaId { get; set; }
    public List<Guid>? CollaboratedProfiles { get; set; }
    public string CollaborationRationale { get; set; }
    public List<int>? CollaborationRequirements { get; set; } 
    public string? CollaborationRequirementOther { get; set; } 
    public List<int>? ExpectedOutcomes { get; set; }
    public string? ExpectedOutcomeOther { get; set; } 
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public Guid SectorId { get; set; }
    public string SectorName { get; set; }

    public bool? IsAdmin { get; set; }  

    public Guid CompanyId { get; set; }
    public string? ContactName { get;  set; }

    public string? ContactAddress { get;  set; }

    public string? ContactMobile { get;  set; }

    public Guid? UserCompanyId { get; set; }

    public string? UserFullName { get; set; }

    public string? UserEmail { get; set; }

    public string? UserMobile { get; set; }

    public List<AttachmentMetaDataDto>? Attachments { get; set; }

    public string? CompanyName { get; set; }

    [JsonIgnore]
    public List<FileUploadContent>? FilesToUpload { get; set; }
    public string? AttachmentDescription { get; set; }
}
