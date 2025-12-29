using PartnersHub.InnovationHub.Domain.Common;


namespace PartnersHub.InnovationHub.Domain.Aggregates.Lookups;

public class Sponsor : AggregateRoot
{
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;

}
