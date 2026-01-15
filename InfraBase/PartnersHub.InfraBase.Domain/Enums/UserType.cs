using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InfraBase.Domain.Enums;

public enum UserType : byte
{
    PcAdmin = 1,
    InfraAdmin = 2,
    Contributor = 3,
}
