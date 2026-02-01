using PartnersHub.Synergy.Application.Common.Integration;
using PartnersHub.Synergy.Application.Interfaces.Services;
using PartnersHub.Synergy.Infrastructure.Persistence.Interfaces;


namespace PartnersHub.Synergy.Infrastructure.Persistence.StaticData;

public class StaticCompanyPifOwnerInfoProvider : IStaticCompanyPifOwnerInfoProvider
{
    private static readonly Dictionary<string, CompanyPifOwnerInfo> _data =
       new(StringComparer.OrdinalIgnoreCase)
       {
           ["Al Elm Information Security Company (Elm)"] = new()
           {
               CompanyPIFOwnerName = "Abdulaziz Abdulrahman Saleh Bin saeed",
               CompanyPIFOwnerEmail = "abinsaeed@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Shahd Abdulrahman H Attar",
               CompanyPIFOwnerSupervisorEmail = "sattar@pif.gov.sa"
           },
           ["Business Incubators and Accelerators Company (BIAC)"] = new()
           {
               CompanyPIFOwnerName = "Nida Osama W Alataba",
               CompanyPIFOwnerEmail = "nalataba@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Shahd Abdulrahman H Attar",
               CompanyPIFOwnerSupervisorEmail = "sattar@pif.gov.sa"
           },
           ["Future Artificial Intelligence Company (Humain)"] = new()
           {
               CompanyPIFOwnerName = "Ammar Jamal Jamal",
               CompanyPIFOwnerEmail = "ajamal@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Shahd Abdulrahman H Attar",
               CompanyPIFOwnerSupervisorEmail = "sattar@pif.gov.sa"
           },
           ["IoTsquared Company"] = new()
           {
               CompanyPIFOwnerName = "Rashad Yousuf Rashad Abuaish",
               CompanyPIFOwnerEmail = "rabuaish@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Shahd Abdulrahman H Attar",
               CompanyPIFOwnerSupervisorEmail = "sattar@pif.gov.sa"
           },
           ["National Real Estate Registration Services Company (RER)"] = new()
           {
               CompanyPIFOwnerName = "Abdulhakim Ibrahim Sultan Alruwais",
               CompanyPIFOwnerEmail = "aialruwais@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Shahd Abdulrahman H Attar",
               CompanyPIFOwnerSupervisorEmail = "sattar@pif.gov.sa"
           },
           ["Saudi Company for Artificial Intelligence (SCAI)"] = new()
           {
               CompanyPIFOwnerName = "Ammar Jamal Jamal",
               CompanyPIFOwnerEmail = "ajamal@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Shahd Abdulrahman H Attar",
               CompanyPIFOwnerSupervisorEmail = "sattar@pif.gov.sa"
           },
           ["Saudi Information Technology Company (SITE)"] = new()
           {
               CompanyPIFOwnerName = "Ammar Jamal Jamal",
               CompanyPIFOwnerEmail = "ajamal@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Shahd Abdulrahman H Attar",
               CompanyPIFOwnerSupervisorEmail = "sattar@pif.gov.sa"
           },
           ["SenseWonder Technology Limited"] = new()
           {
               CompanyPIFOwnerName = "Rashad Yousuf Rashad Abuaish",
               CompanyPIFOwnerEmail = "rabuaish@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Shahd Abdulrahman H Attar",
               CompanyPIFOwnerSupervisorEmail = "sattar@pif.gov.sa"
           },
           ["Folk Maritime Services Company"] = new()
           {
               CompanyPIFOwnerName = "Faris Abdullah S Aljarboa",
               CompanyPIFOwnerEmail = "faljarboa@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Baker Abdulrahman A Almohana",
               CompanyPIFOwnerSupervisorEmail = "balmuhanna@pif.gov.sa"
           },
           ["Red Sea Gateway Terminal"] = new()
           {
               CompanyPIFOwnerName = "Ranjith Baden Powell",
               CompanyPIFOwnerEmail = "rpowell@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Baker Abdulrahman A Almohana",
               CompanyPIFOwnerSupervisorEmail = "balmuhanna@pif.gov.sa"
           },
           ["Saudi Global Ports Company (Singapore)"] = new()
           {
               CompanyPIFOwnerName = "Ranjith Baden Powell",
               CompanyPIFOwnerEmail = "rpowell@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Baker Abdulrahman A Almohana",
               CompanyPIFOwnerSupervisorEmail = "balmuhanna@pif.gov.sa"
           },
           ["Saudi Public Transport Company"] = new()
           {
               CompanyPIFOwnerName = "Ype Juurd Hangelbroek",
               CompanyPIFOwnerEmail = "yhangelbroek@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Baker Abdulrahman A Almohana",
               CompanyPIFOwnerSupervisorEmail = "balmuhanna@pif.gov.sa"
           },
           ["The National Shipping Company of Saudi Arabia"] = new()
           {
               CompanyPIFOwnerName = "Faris Abdullah S Aljarboa",
               CompanyPIFOwnerEmail = "faljarboa@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Baker Abdulrahman A Almohana",
               CompanyPIFOwnerSupervisorEmail = "balmuhanna@pif.gov.sa"
           },
           ["Zamil Offshore Services Company"] = new()
           {
               CompanyPIFOwnerName = "Faris Abdullah S Aljarboa",
               CompanyPIFOwnerEmail = "faljarboa@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Baker Abdulrahman A Almohana",
               CompanyPIFOwnerSupervisorEmail = "balmuhanna@pif.gov.sa"
           },
           ["Saudi Re"] = new()
           {
               CompanyPIFOwnerName = "Fahad Ibrahim M Aljomaih",
               CompanyPIFOwnerEmail = "faljomaih@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Sultan Abdulmalek A Al sheikh",
               CompanyPIFOwnerSupervisorEmail = "sultanalsheikh@pif.gov.sa"
           },
           ["Tawrid"] = new()
           {
               CompanyPIFOwnerName = "Saud Abdulaziz Saleh Bin amer",
               CompanyPIFOwnerEmail = "sbinamer@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Sultan Abdulmalek A Al sheikh",
               CompanyPIFOwnerSupervisorEmail = "sultanalsheikh@pif.gov.sa"
           },
           ["Bada'el Company"] = new()
           {
               CompanyPIFOwnerName = "Karim Touzani",
               CompanyPIFOwnerEmail = "ktouzani@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Sultan Anas H Mousa",
               CompanyPIFOwnerSupervisorEmail = "smousa@pif.gov.sa"
           },
           ["Lean Business Services"] = new()
           {
               CompanyPIFOwnerName = "Saud Abdulhamid F Alharbi",
               CompanyPIFOwnerEmail = "saalharbi@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Sultan Anas H Mousa",
               CompanyPIFOwnerSupervisorEmail = "smousa@pif.gov.sa"
           },
           ["Pharmaceutical Investment Company (Lifera)"] = new()
           {
               CompanyPIFOwnerName = "Emerson Burke Murphy",
               CompanyPIFOwnerEmail = "emurphy@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Sultan Anas H Mousa",
               CompanyPIFOwnerSupervisorEmail = "smousa@pif.gov.sa"
           },
           ["National Unified Procurement Company For Medical Supplies (Nupco)"] = new()
           {
               CompanyPIFOwnerName = "Hamed Mansour Hamad Alobrah",
               CompanyPIFOwnerEmail = "halobrah@pif.gov.sa",
               CompanyPIFOwnerSupervisorName = "Sultan Anas H Mousa",
               CompanyPIFOwnerSupervisorEmail = "smousa@pif.gov.sa"
           }
       };

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

