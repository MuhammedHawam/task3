namespace PartnersHub.Shared.Integration.DTOs;

/// <summary>
/// DTO representing company data from PIF middleware service.
/// </summary>
public class ExternalCompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public byte[]? Logo { get; set; }
    public string? Website { get; set; }

    public string? Country { get; set; }
    public string? City { get; set; }
    public string? CityAr { get; set; }

    public Guid? SectorId { get; set; }
    public string? SectorName { get; set; }
    public string? SectorNameAr { get; set; }

    public Guid? DivisionId { get; set; }
    public string? DivisionName { get; set; }
    public string? DivisionNameAr { get; set; }

    public DateTime? EstablishmentDate { get; set; }
    public DateTime? CreatedOn { get; set; }

    public ExternalRepresentativeDto? Representative { get; set; }
}

public class ExternalSectorDto
{
    public Guid SectorId { get; set; }
    public string SectorName { get; set; } = string.Empty;
    public string? SectorNameAr { get; set; }
}

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
    public List<CompanyIntegrationDto> Companies { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CompanyIntegrationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string Logo { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string CityAr { get; set; } = string.Empty;
    public string SectorId { get; set; } = string.Empty;
    public string SectorName { get; set; } = string.Empty;
    public string SectorNameAr { get; set; } = string.Empty;
    public string DivisionId { get; set; } = string.Empty;
    public string DivisionName { get; set; } = string.Empty;
    public string DivisionNameAr { get; set; } = string.Empty;
    public DateTime? EstablishmentDate { get; set; }
    public DateTime? CreatedOn { get; set; }
    public CompanyRepresentativeDto Representative { get; set; } = new();
}

public class CompanyRepresentativeDto
{
    public string Name { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string PositionAr { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
}

public class CompanyIntegrationRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchText { get; set; }
    public List<Guid>? SectorIds { get; set; }
    public List<Guid>? CityIds { get; set; }
}
