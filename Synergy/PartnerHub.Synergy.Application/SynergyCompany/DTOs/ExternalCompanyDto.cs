namespace PartnersHub.Synergy.Application.SynergyCompany.DTOs;

/// <summary>
/// DTO representing company data from PIF middleware service
/// Matches the actual PIF API response structure
/// </summary>
public class ExternalCompanyDto
{
    // Basic information
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public byte[]? Logo { get; set; }
    public string? Website { get; set; }
    
    // Location
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? CityAr { get; set; }
    
    // Sector information
    public Guid? SectorId { get; set; }
    public string? SectorName { get; set; }
    public string? SectorNameAr { get; set; }
    
    // Division information
    public Guid? DivisionId { get; set; }
    public string? DivisionName { get; set; }
    public string? DivisionNameAr { get; set; }
    
    // Dates
    public DateTime? EstablishmentDate { get; set; }
    public DateTime? CreatedOn { get; set; }
    
    // Representative/contact information
    public ExternalRepresentativeDto? Representative { get; set; }
}

/// <summary>
/// Sector information from external system
/// </summary>
public class ExternalSectorDto
{
    public Guid SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public string? SectorNameAr { get; set; }
}

/// <summary>
/// Representative/contact information from PIF API
/// </summary>
public class ExternalRepresentativeDto
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? Position { get; set; }
    public string? PositionAr { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
}


public class CompanyIntegrationResponseDto
{
    public List<CompanyIntegrationDto> Companies { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CompanyIntegrationDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string NameAr { get; set; }
    public string Description { get; set; }
    public string DescriptionAr { get; set; }
    public string Logo { get; set; }
    public string Website { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string CityAr { get; set; }
    public string SectorId { get; set; }
    public string SectorName { get; set; }
    public string SectorNameAr { get; set; }
    public string DivisionId { get; set; }
    public string DivisionName { get; set; }
    public string DivisionNameAr { get; set; }
    public DateTime? EstablishmentDate { get; set; }
    public DateTime? CreatedOn { get; set; }
    public CompanyRepresentativeDto Representative { get; set; }
}

public class CompanyRepresentativeDto
{
    public string Name { get; set; }
    public string NameAr { get; set; }
    public string Position { get; set; }
    public string PositionAr { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Mobile { get; set; }
}

public class CompanyIntegrationRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SearchText { get; set; }
    public List<Guid> SectorIds { get; set; }
    public List<Guid> CityIds { get; set; }
}
