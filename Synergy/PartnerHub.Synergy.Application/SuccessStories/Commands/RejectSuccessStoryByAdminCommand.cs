using MediatR;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.SuccessStories.Commands
{
    public class RejectSuccessStoryByAdminCommand : IRequest<Result>
    {
        public Guid SuccessStoryId { get; set; }
        public string RejectionReason { get; set; }
    }
}
