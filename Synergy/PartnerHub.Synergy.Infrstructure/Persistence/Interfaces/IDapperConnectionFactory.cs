using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Infrastructure.Persistence.Interfaces
{
    public interface IDapperConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
