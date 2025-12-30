using PartnersHub.Synergy.Application.SynergyCompany.DTOs;

namespace PartnersHub.Synergy.Application.Dashboard.DTOs;

public class CompanyInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Company logo in base64 format
    /// </summary>
    public string? Logo { get; set; }
}

public class RecentOpportunityCardDto
{
    public Guid Id { get; set; }
    public string RequestId { get; set; }
    public string Title { get; set; } = null!;
    public CompanyInfoDto PostedByCompany { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string CollaborationType { get; set; } = null!;
    public string Sector { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string CompanyName { get; set; }
    public bool IsHide { get; set; }
}

public class RecentSuccessStoryCardDto
{
    public Guid Id { get; set; }
    public string RequestId { get; set; }
    public string Title { get; set; } = null!;
    public CompanyInfoDto SourceCompany { get; set; } = null!;
    public List<CompanyInfoDto> PartnerCompanies { get; set; } = new();
    public string Description { get; set; } = null!;
    public string Type { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string CompanyName { get; set; } 

    public bool IsHide { get; set; }
}

public class RecentCompanyCardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Company logo in base64 format
    /// </summary>
    public string? Logo { get; set; }
    
    public string Description { get; set; } = null!;
    public List<CompanySectorDto> Sectors { get; set; } = new();
    public string HeadquarterCountry { get; set; } = null!;
    public string HeadquarterCity { get; set; } = null!;
    public DateTime RegisteredDate { get; set; }
    public int TotalCollaborationNumber { get; set; }
}
