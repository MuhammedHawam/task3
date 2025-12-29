using PartnersHub.InfraBase.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InfraBase.Application.Common.Interfaces.Services
{
    public interface IAdminCommunicationService
    {
        Task<List<string>> GetUserPermissions(Guid UserId);
    }
}
