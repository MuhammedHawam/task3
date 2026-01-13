using PartnersHub.Synergy.Application.Common.Helpers;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.SuccessStories.Commands;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.SuccessStories.DTOs
{
    public class SuccessStoryResponseDto
    {
        public string RequestId { get; set; }
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }

        private byte[]? _logo;

        [JsonIgnore]
        public byte[]? Logo
        {
            get => _logo;
            set
            {
                _logo = value;
                CompanyLogo = LogoHelper.ToBase64String(_logo);
            }
        }
        public string CompanyLogo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SuccessStoryType { get; set; }
        public SuccessStoryStatus SuccessStoryStatus { get; set; }
        public string SuccessStoryStatusDescription { get; set; }
        public List<PatnerCompany> CollaboratingPartners { get; set; }
        public List<GuidKeyValueDto> AssociatedOpportunities { get; set; }
        public List<OpportunityStoryDto> AssociatedOpportunitiesList { get; set; }
        public List<KeyValueDto> ExpectedOutcomes { get; set; }
        public List<GuidKeyValueDto> Sectors { get; set; }
        public string? SectorName { get; set; }

        public Guid? SectorId { get; set; }
        public List<KeyValueDto> ThematicAreas { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SubmissionDate { get; set; }
        public List<SuccessStoryAttachmentDto> Attachments { get; set; } = new();
        public string? RejectionReason { get; set; }

        public string OpportunityTypeName { get; set; } 

        public bool IsHide { get; set; }

        public SuccessStroyCollaborationStatus CollaborationStatus { get; set; }

        public string CollaborationStatusDescription { get; set; }

        public int SuccessStoryTypeId { get; set; }

        public int SuccessStoryCollaborationStatusId { get; set; }

        public Guid? AssociatedOpportunityId => AssociatedOpportunitiesList?.FirstOrDefault()?.Id;
    }


    public class OpportunityStoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public Guid SectorId { get; set; }
        public string SectorName { get; set; } = null!;

        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public byte[]? CompanyLogo { get; set; }

        public int OpportunityTypeId { get; set; }
        public string OpportunityTypeName { get; set; } = null!;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

}
