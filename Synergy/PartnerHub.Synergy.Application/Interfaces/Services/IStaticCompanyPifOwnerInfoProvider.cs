using PartnersHub.Synergy.Application.Common.Integration;


namespace PartnersHub.Synergy.Application.Interfaces.Services;

public interface IStaticCompanyPifOwnerInfoProvider
{
    CompanyPifOwnerInfo? GetByCompanyName(string companyName);
}
