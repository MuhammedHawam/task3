using PartnersHub.InnovationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Domain.Aggregates.Lookups;

public class CompanySector : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid SectorId { get; private set; }
    public string SectorName { get; private set; } = null!;
    public DateTime AssignedDate { get; private set; }

    private CompanySector() { }

    public CompanySector(Guid companyId, Guid sectorId, string sectorName)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company ID is required", nameof(companyId));

        if (sectorId == Guid.Empty)
            throw new ArgumentException("Sector ID is required", nameof(sectorId));

        if (string.IsNullOrWhiteSpace(sectorName))
            throw new ArgumentException("Sector name is required", nameof(sectorName));

        CompanyId = companyId;
        SectorId = sectorId;
        SectorName = sectorName.Trim();
        AssignedDate = DateTime.UtcNow;
    }
}
