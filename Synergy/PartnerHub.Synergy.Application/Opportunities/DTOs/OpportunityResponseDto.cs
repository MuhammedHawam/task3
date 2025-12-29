using Microsoft.AspNetCore.Http;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.Opportunities.Commands;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.Opportunities.DTOs
{
    public class OpportunityResponseDto
    {
        public Guid Id { get; set; }    
        public Guid CompanyId { get; set; }
        public string RequestId { get; set; }
        public string? CompanyName { get; set; }

        /// <summary>
        /// Company logo in base64 format (can be used directly in img src)
        /// </summary>
        public string? CompanyLogo { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int TypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public OpportunityStatus Status { get; set; }
        public string StatusDescription { get; set; } = null!;

        public int ThematicAreaId { get; set; }
        public string ThematicAreaName { get; set; } = null!;
        public string SectorName { get; set; } = null!;
        public Guid SectorId { get; set; }
        //public AttachmentMetaDataDto? Attachment { get; set; }
        public List<GuidKeyValueDto> CollaboratedProfiles { get; set; } = new();
        public string CollaborationRationale { get; set; } = null!;
        public List<KeyValueDto> CollaborationRequirements { get; set; } = new();
        public string? CollaborationRequirementOther { get; set; }
        public List<KeyValueDto> ExpectedOutcomes { get; set; } = new();
        public string? ExpectedOutcomeOther { get; set; }
        public string? RepresentativeName { get; set; }
        public string? RepresentativePhone { get; set; }
        public string? RepresentaitveTitle { get; set; }
        public string RepresentativeEmail { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid TermsAndConditionId { get; set; }
        public Guid? CreatedBy { get; set; }
        public List<OpportunityAttachmentDto> Attachments { get; set; } = new();
        public string? RejectionReason { get; set; }
    }
}
