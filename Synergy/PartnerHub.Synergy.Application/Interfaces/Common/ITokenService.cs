using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.Interfaces.Common;

public interface ITokenService
{
    string GetUserEmail();
    string GetUserName(); 
    Guid? GetCompanyId();  
    string? GetCompanyName();
    List<Guid> GetUserRoleIds();
    bool IsPcAdmin();
    bool IsInfrabaseAdmin();
}
