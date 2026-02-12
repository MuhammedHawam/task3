using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.Synergy.Application.Common.Converters;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.SuccessStories.Commands
{
    public class CreateSuccessStoryCommand : IRequest<Result<Guid>>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int SuccessStoryTypeId { get; set; }
        public int SuccessStoryCollaborationStatusId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Guid>? CollaboratedProfiles { get; set; }

        [JsonConverter(typeof(EmptyStringToNullableGuidConverter))]
        public Guid? AssociatedOpportunities { get; set; }
        public List<IFormFile?> ? Attachments { get; set; }

        public bool? IsAdmin { get; set; }
        public Guid? UserCompanyId { get; set; }

        public Guid SectorId { get;  set; }

        public string? SectorName { get;  set; }
        public string? UserEmail { get;  set; }

        public string? CompanyName { get; set; }

        [JsonIgnore]
        public List<FileUploadContent>? FilesToUpload { get; set; }
        public string? AttachmentDescription { get; set; }

    }


    
}
