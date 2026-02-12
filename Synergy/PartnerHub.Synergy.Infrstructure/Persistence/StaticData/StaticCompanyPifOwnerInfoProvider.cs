using Microsoft.Extensions.Configuration;
using PartnersHub.Synergy.Application.Common.Integration;
using PartnersHub.Synergy.Application.Interfaces.Services;



namespace PartnersHub.Synergy.Infrastructure.Persistence.StaticData;

public class StaticCompanyPifOwnerInfoProvider : IStaticCompanyPifOwnerInfoProvider
{
    private readonly Dictionary<string, CompanyPifOwnerInfo> _data;

    public StaticCompanyPifOwnerInfoProvider(IConfiguration config)
    {
        var list = config.GetSection("CompanyPifOwners")
            .Get<List<CompanyPifOwnerInfo>>() ?? new List<CompanyPifOwnerInfo>();

        _data = list.ToDictionary(x => x.CompanyName, StringComparer.OrdinalIgnoreCase);
    }

    public CompanyPifOwnerInfo? GetByCompanyName(string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return null;

        var name = companyName.Trim();

        //Exact match
        if (_data.TryGetValue(name, out var exact))
            return exact;

        //partial match
        return _data.FirstOrDefault(x =>
            name.Contains(x.Key, StringComparison.OrdinalIgnoreCase)
        ).Value;
    }

}

