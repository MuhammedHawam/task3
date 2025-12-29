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
