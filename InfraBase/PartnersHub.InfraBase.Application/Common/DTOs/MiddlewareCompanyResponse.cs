namespace PartnersHub.InfraBase.Application.Common.DTOs;

public class MiddlewareCompanyResponse
{
    public int HttpCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public MiddlewareCompany? Data { get; set; }
    public string? Error { get; set; }
}

public class MiddlewareCompany
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Logo { get; set; }
    public string? Industry { get; set; }
    public string? IndustryAr { get; set; }
    public string? Location { get; set; }
    public string? LocationAr { get; set; }
    public CompanySector? Sector { get; set; }
    public CompanyRepresentative? Representative { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public int ChallengesCount { get; set; }
    public int CampaignsCount { get; set; }
    public int TotalActivity { get; set; }
    public string? Website { get; set; }
    public DateTime? EstablishmentDate { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CompanySector
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? NameAr { get; set; }
}

public class CompanyRepresentative
{
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public string? Position { get; set; }
    public string? PositionAr { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Photo { get; set; }
}

/// <summary>
/// DTO for portfolio company selection in dropdown
/// </summary>
public class PortfolioCompanyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SectorName { get; set; }
    public string? CompanyAdminRepresentativeName { get; set; }
    public string? CompanyAdminRepresentativeEmail { get; set; }
}

/// <summary>
/// Response wrapper for list of companies from middleware
/// </summary>
public class MiddlewareCompanyListResponse
{
    public int HttpCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<MiddlewareCompany>? Data { get; set; }
    public string? Error { get; set; }
}
