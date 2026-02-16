using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;

    public interface ICurrentUserService
    {
    string UserName { get; }
    string UserId { get; }
    string RoleId { get; }
    }

