using PartnersHub.Synergy.Application.Common.Helpers;
using System.Text.Json.Serialization;

namespace PartnersHub.Synergy.Application.Models;

public record KeyValueDto<TKey>(TKey Id, string Name);

public record KeyValueDto(int Id, string Name);
public class GuidKeyValueDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public GuidKeyValueDto()
    {

    }
    public GuidKeyValueDto(Guid id, string name)
    {
        Id = id;
        Name = name;

    }
}

public class PatnerCompany
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    [JsonIgnore]
    public byte[]? Logo
    {
        get => _logo;
        set
        {
            _logo = value;
            LogoImage = LogoHelper.ToBase64String(_logo);
        }
    }
    public string? LogoImage { get; set; }
    private byte[]? _logo;

    public string? Description { get; set; }

    // Location
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? CityAr { get; set; }

    // Sector information
    public Guid? SectorId { get; set; }
    public string? SectorName { get; set; }
    public string? SectorNameAr { get; set; }

    public PatnerCompany()
    {

    }
    public PatnerCompany(Guid id, string name, byte[]? logo)
    {
        Id = id;
        Name = name;
        Logo = logo;
        LogoImage = LogoHelper.ToBase64String(logo);

    }
}
