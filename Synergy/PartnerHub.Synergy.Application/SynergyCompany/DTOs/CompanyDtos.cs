using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SynergyCompany.DTOs;

public class RegisteredCompanyCardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Company logo in base64 format (can be used directly in img src)
    /// </summary>
    public string? Logo { get; set; }
    
    public List<CompanySectorDto> Sectors { get; set; } = new();
    public string HeadquarterCountry { get; set; } = null!;
    public string HeadquarterCity { get; set; } = null!;
    public int TotalCollaborationNumber { get; set; }
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CompanyDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Company logo in base64 format (can be used directly in img src)
    /// </summary>
    public string? Logo { get; set; }
    
    public CompanySectorDto Sector { get; set; }
    public string HeadquarterCountry { get; set; } = null!;
    public string HeadquarterCity { get; set; } = null!;
    public int TotalCollaborationNumber { get; set; }
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; }
    public List<string> Services { get; set; } = new();
    public List<string> CollaborationFocus { get; set; } = new();
    public RepresentativeInfoDto Representative { get; set; } = null!;
    public List<OpportunityCollaborationDto> Collaborations { get; set; } = new();
    public List<SuccessStoryPreviewDto> SuccessStories { get; set; } = new();
}

public class CompanySectorDto
{
    public CompanySectorDto(Guid sectorId, string sectorName)
    {
        SectorId = sectorId;
        SectorName = sectorName;
    }
    public CompanySectorDto()
    {

    }

    public Guid SectorId { get; set; }
    public string SectorName { get; set; } = null!;
}

public class RepresentativeInfoDto
{
    public string Name { get; set; } = null!;
    public string Position { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
}

public class OpportunityCollaborationDto
{
    public Guid OpportunityId { get; set; }
    public string Title { get; set; } = null!;
    public string CollaborationType { get; set; } = null!;
    public string Sector { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string PostedByCompany { get; set; } = null!
;
    public string Description { get; set; } = null!;

    public string RequestId { get; set; }
    public Guid Id { get; set; }

    public OpportunityStatus Status { get; set; }
    public string StatusDescription { get; set; } = null!;

    // Company info
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = null!;

    /// <summary>
    /// Company logo in base64 format (can be used directly in img src)
    /// </summary>
    public string? CompanyLogo { get; set; }

    // Classification
    public int OpportunityTypeId { get; set; }
    public string OpportunityTypeName { get; set; } = null!;
    public int ThematicAreaId { get; set; }
    public string ThematicAreaName { get; set; } = null!;
    public Guid SectorId { get; set; }
    public string SectorName { get; set; } = null!;

    // Collaboration details
    public List<string> CollaborationRequirements { get; set; } = new();
    public List<string> ExpectedOutcomes { get; set; } = new();
    public int CollaboratedCompaniesCount { get; set; }

    // Timeline

    public DateTime CreatedAt { get; set; }
}

public class SuccessStoryPreviewDto
{
    public Guid StoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Type { get; set; } = null!;
    public List<CompanyNameLogoDto> PartnerCompanies { get; set; } = new();
    public string PostedBy { get; set; } = null!;
    public DateTime PostedDate { get; set; }
    public string? SectorName { get; set; } 
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Description { get; set; } = null!;


    public string RequestId { get; set; }
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; }
    public string SuccessStoryType { get; set; }
    public SuccessStoryStatus SuccessStoryStatus { get; set; }
    public string SuccessStoryStatusDescription { get; set; }
    public DateTime SubmissionDate { get; set; }

}

public class CompanyNameLogoDto
{
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Company logo in base64 format
    /// </summary>
    public string? Logo { get; set; }
}
