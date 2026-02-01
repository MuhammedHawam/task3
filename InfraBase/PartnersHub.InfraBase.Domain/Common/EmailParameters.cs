using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InfraBase.Domain.Common;

public class EmailParameters
{
    public string InfraBaseModuleCC { get; set; }
    public string BaseURL { get; set; }
}


public class CompanyAssetManager
{
    public string PCName { get; set; }
    public string AssetManagerEmail { get; set; }
    public string Sector { get; set; }
    public string Industry { get; set; }
    public string SectorHeadName { get; set; }
    public string SectorHeadEmail { get; set; }
    public string AssetManagerName { get; set; }
}

public class InfraMember
{
    public string InfraMemberName { get; set; }
    public string Email { get; set; }

}
