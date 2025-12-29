using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string RoleId { get; }
    }
}
