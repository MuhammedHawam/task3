using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Events;
using PartnersHub.InnovationHub.Domain.ValueObjects;



namespace PartnersHub.InnovationHub.Domain.Aggregates.Lookups;

public class Company : AggregateRoot
{
    private readonly List<CompanySector> _sectors = new();

    public CompanyName Name { get; private set; } = null!;
    public string HeadquarterCountry { get; private set; } = null!;
    public string HeadquarterCity { get; private set; } = null!;
    public Description Description { get; private set; } = null!;
    public byte[]? Logo { get; private set; }
    public bool IsActive { get; set; }
    public RepresentativeInformation RepresentativeInformation { get; private set; } = null!;
    public IReadOnlyCollection<CompanySector> Sectors => _sectors.AsReadOnly();

    private Company() { }

    private Company(Guid companyId, CompanyName name, string headquarterCountry, string headquarterCity,
        Description description, byte[]? logo, RepresentativeInformation representativeInformation, Guid createdBy)
    {
        Id = companyId;
        Name = name;
        HeadquarterCountry = headquarterCountry;
        HeadquarterCity = headquarterCity;
        Description = description;
        Logo = logo;
        RepresentativeInformation = representativeInformation;

        MarkAsCreated(createdBy);
        AddDomainEvent(new CompanyCreatedEvent(Id, name.Value, createdBy));
    }

    public static Result<Company> Create(Guid companyId, string name, string headquarterCountry,
        string headquarterCity, string description, string repName, string repPosition, string repEmail,
        string repPhone, Guid createdBy, byte[]? logo)
    {
        var nameResult = CompanyName.Create(name);
        if (nameResult.IsFailure)
            return Result<Company>.Failure(nameResult.Error!);

        var descriptionResult = Description.Create(description);
        if (descriptionResult.IsFailure)
            return Result<Company>.Failure(descriptionResult.Error!);

        var repInfoResult = RepresentativeInformation.Create(repName, repPosition, repEmail, repPhone);
        if (repInfoResult.IsFailure)
            return Result<Company>.Failure(repInfoResult.Error!);

        if (string.IsNullOrWhiteSpace(headquarterCountry))
            return Result<Company>.Failure("Headquarter country is required");

        if (string.IsNullOrWhiteSpace(headquarterCity))
            return Result<Company>.Failure("Headquarter city is required");

        var company = new Company(companyId, nameResult.Value!, headquarterCountry.Trim(),
            headquarterCity.Trim(), descriptionResult.Value!, logo, repInfoResult.Value!, createdBy);

        return Result<Company>.Success(company);
    }

    public Result<bool> AddSector(Guid sectorId, string sectorName)
    {
        if (sectorId == Guid.Empty)
            return Result<bool>.Failure("Sector ID is required");

        if (string.IsNullOrWhiteSpace(sectorName))
            return Result<bool>.Failure("Sector name is required");

        if (_sectors.Any(s => s.SectorId == sectorId))
            return Result<bool>.Failure("Sector already assigned to this company");

        try
        {
            var companySector = new CompanySector(Id, sectorId, sectorName);
            _sectors.Add(companySector);
            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public Result<bool> AddSectors(Dictionary<Guid, string> sectors)
    {
        if (sectors == null || sectors.Count == 0)
            return Result<bool>.Failure("At least one sector is required");

        try
        {
            foreach (var sector in sectors.Where(s => !_sectors.Any(cs => cs.SectorId == s.Key)))
            {
                var companySector = new CompanySector(Id, sector.Key, sector.Value);
                _sectors.Add(companySector);
            }
            return Result<bool>.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public Result<bool> RemoveSector(Guid sectorId)
    {
        var sector = _sectors.FirstOrDefault(s => s.SectorId == sectorId);
        if (sector == null)
            return Result<bool>.Failure("Sector not assigned to this company");

        _sectors.Remove(sector);
        return Result<bool>.Success(true);
    }

    public Result<bool> UpdateInformation(string? name, string? headquarterCountry, string? headquarterCity,
        string? description, Guid userId)
    {
        bool hasChanges = false;

        if (!string.IsNullOrWhiteSpace(name) && name != Name.Value)
        {
            var nameResult = CompanyName.Create(name);
            if (nameResult.IsFailure)
                return Result<bool>.Failure(nameResult.Error!);

            Name = nameResult.Value!;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            var descResult = Description.Create(description);
            if (descResult.IsFailure)
                return Result<bool>.Failure(descResult.Error!);

            Description = descResult.Value!;
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(headquarterCountry) && headquarterCountry != HeadquarterCountry)
        {
            HeadquarterCountry = headquarterCountry.Trim();
            hasChanges = true;
        }

        if (!string.IsNullOrWhiteSpace(headquarterCity) && headquarterCity != HeadquarterCity)
        {
            HeadquarterCity = headquarterCity.Trim();
            hasChanges = true;
        }

        if (hasChanges)
        {
            MarkAsUpdated(userId);
            AddDomainEvent(new CompanyUpdatedEvent(Id, userId));
        }

        return Result<bool>.Success(true);
    }

    public Result<bool> SetLogoUrl(byte[]? logo, Guid updatedBy)
    {
        if (logo == null || logo.Length == default(int))
            return Result<bool>.Failure("Logo is required");

        Logo = logo;
        MarkAsUpdated(updatedBy);
        AddDomainEvent(new CompanyUpdatedEvent(Id, updatedBy));

        return Result<bool>.Success(true);
    }


}
