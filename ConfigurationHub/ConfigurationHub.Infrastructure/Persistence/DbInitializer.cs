using PartnersHub.ConfigurationHub.Domain.Aggregates.Configuration;
using PartnersHub.ConfigurationHub.Domain.Aggregates.Lookups;
using PartnersHub.ConfigurationHub.Domain.Enums;
using System.Text.RegularExpressions;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence;

public static class DbInitializer
{
    private static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static async Task InitializeDatabaseAsync(ConfigurationHubDbContext context)
    {
        try
        {
            // IMPORTANT:
            // This initializer is executed on application startup.
            // It must be idempotent and MUST NOT wipe lookup tables (Sectors/SubSectors/AssetTypes/UOMs),
            // otherwise existing records in downstream services (InfraBase assets) will reference stale GUIDs.

            await SeedWhiteListIPsAsync(context);
            await SeedSectorsAsync(context);
            await SeedAssetTypesAsync(context);
            await SeedSubSectorAssetTypesAsync(context);
            await SeedUnitsOfMeasurementAsync(context);
            await SeedTermsAndConditionsAsync(context);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing database: {ex.Message}");
            throw;
        }
    }

    private static async Task SeedWhiteListIPsAsync(ConfigurationHubDbContext context)
    {
        if (context.WhiteListIPs.Any())
        {
            return;
        }

        var whitelistIPs = new[]
        {
            WhiteListIP.Create("127.0.0.1", DateTime.UtcNow.AddYears(1), "Localhost", SystemUserId).Value!,
            WhiteListIP.Create("192.168.1.1", DateTime.UtcNow.AddDays(90), "Office Network", SystemUserId).Value!,
            WhiteListIP.Create("10.0.0.1", DateTime.UtcNow.AddDays(180), "VPN Gateway", SystemUserId).Value!
        };

        await context.WhiteListIPs.AddRangeAsync(whitelistIPs);
    }

    private static async Task SeedSectorsAsync(ConfigurationHubDbContext context)
    {
        var lookupTriples = GetInfraBaseLookupSeedData();

        var sectorNames = DistinctInOrder(lookupTriples.Select(x => x.Sector));
        var usedSectorCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var desiredSectors = sectorNames
            .Select((nameEn, i) => new
            {
                Code = EnsureUniqueCode(ToBaseCode(nameEn), usedSectorCodes),
                NameEn = nameEn,
                DisplayOrder = i + 1
            })
            .ToList();

        var existingSectors = context.Sectors.ToList();
        var existingSectorByCode = existingSectors.ToDictionary(s => s.Code, s => s, StringComparer.OrdinalIgnoreCase);

        foreach (var desired in desiredSectors)
        {
            if (existingSectorByCode.TryGetValue(desired.Code, out var existing))
            {
                existing.Update(
                    nameAr: desired.NameEn,
                    nameEn: desired.NameEn,
                    descriptionAr: null,
                    descriptionEn: null,
                    displayOrder: desired.DisplayOrder,
                    updatedBy: SystemUserId);

                if (!existing.IsActive)
                {
                    existing.Activate(SystemUserId);
                }
            }
            else
            {
                var created = Sector.Create(
                    code: desired.Code,
                    nameAr: desired.NameEn,
                    nameEn: desired.NameEn,
                    descriptionAr: null,
                    descriptionEn: null,
                    displayOrder: desired.DisplayOrder,
                    createdBy: SystemUserId).Value!;
                await context.Sectors.AddAsync(created);
            }
        }

        await context.SaveChangesAsync(); // ensure sectors exist before seeding subsectors

        // Rebuild sector lookup by name to get persisted IDs (without relying on new GUIDs).
        var sectorIdByName = context.Sectors
            .Where(s => desiredSectors.Select(d => d.Code).Contains(s.Code))
            .ToList()
            .ToDictionary(s => s.NameEn, s => s.Id, StringComparer.OrdinalIgnoreCase);

        var subSectors = new List<SubSector>();
        foreach (var sectorName in sectorNames)
        {
            var sectorId = sectorIdByName[sectorName];
            var subSectorNames = DistinctInOrder(
                lookupTriples
                    .Where(x => string.Equals(x.Sector, sectorName, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.SubSector));

            var usedSubSectorCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var displayOrder = 1;

            var existingSubSectors = context.SubSectors.Where(ss => ss.SectorId == sectorId).ToList();
            var existingSubSectorByCode = existingSubSectors.ToDictionary(ss => ss.Code, ss => ss, StringComparer.OrdinalIgnoreCase);

            foreach (var subSectorName in subSectorNames)
            {
                var code = EnsureUniqueCode(ToBaseCode(subSectorName), usedSubSectorCodes);
                if (existingSubSectorByCode.TryGetValue(code, out var existing))
                {
                    existing.Update(
                        nameAr: subSectorName,
                        nameEn: subSectorName,
                        descriptionAr: null,
                        descriptionEn: null,
                        displayOrder: displayOrder++,
                        updatedBy: SystemUserId);

                    if (!existing.IsActive)
                    {
                        existing.Activate(SystemUserId);
                    }
                }
                else
                {
                    subSectors.Add(SubSector.Create(
                        sectorId: sectorId,
                        code: code,
                        nameAr: subSectorName,
                        nameEn: subSectorName,
                        descriptionAr: null,
                        descriptionEn: null,
                        displayOrder: displayOrder++,
                        createdBy: SystemUserId).Value!);
                }
            }
        }

        if (subSectors.Count > 0)
        {
            await context.SubSectors.AddRangeAsync(subSectors);
        }
    }

    private static async Task SeedAssetTypesAsync(ConfigurationHubDbContext context)
    {
        var lookupTriples = GetInfraBaseLookupSeedData();
        var assetNames = DistinctInOrder(lookupTriples.Select(x => x.Asset));

        var usedAssetCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var desiredAssetTypes = assetNames
            .Select((nameEn, i) => new
            {
                Code = EnsureUniqueCode(ToBaseCode(nameEn), usedAssetCodes),
                NameEn = nameEn,
                DisplayOrder = i + 1
            })
            .ToList();

        var existing = context.AssetTypes.ToList();
        var existingByCode = existing.ToDictionary(a => a.Code, a => a, StringComparer.OrdinalIgnoreCase);

        foreach (var desired in desiredAssetTypes)
        {
            if (existingByCode.TryGetValue(desired.Code, out var found))
            {
                found.Update(
                    nameAr: desired.NameEn,
                    nameEn: desired.NameEn,
                    descriptionAr: null,
                    descriptionEn: null,
                    displayOrder: desired.DisplayOrder,
                    updatedBy: SystemUserId);

                if (!found.IsActive)
                {
                    found.Activate(SystemUserId);
                }
            }
            else
            {
                var created = AssetType.Create(
                    code: desired.Code,
                    nameAr: desired.NameEn,
                    nameEn: desired.NameEn,
                    descriptionAr: null,
                    descriptionEn: null,
                    displayOrder: desired.DisplayOrder,
                    createdBy: SystemUserId).Value!;
                await context.AssetTypes.AddAsync(created);
            }
        }
    }

    private static async Task SeedSubSectorAssetTypesAsync(ConfigurationHubDbContext context)
    {
        var lookupTriples = GetInfraBaseLookupSeedData();

        // Build lookup maps based on persisted values (do not rely on deterministic GUIDs).
        var sectors = context.Sectors.ToList();
        var sectorNameById = sectors.ToDictionary(s => s.Id, s => s.NameEn);

        var subSectors = context.SubSectors.ToList();
        var subSectorIdBySectorAndName = subSectors
            .Where(ss => sectorNameById.ContainsKey(ss.SectorId))
            .ToDictionary(
                ss => $"{sectorNameById[ss.SectorId].Trim().ToUpperInvariant()}|{ss.NameEn.Trim().ToUpperInvariant()}",
                ss => ss.Id,
                StringComparer.OrdinalIgnoreCase);

        var assetTypes = context.AssetTypes.ToList();
        var assetTypeIdByName = assetTypes.ToDictionary(a => a.NameEn.Trim(), a => a.Id, StringComparer.OrdinalIgnoreCase);

        var desiredPairs = new HashSet<(Guid SubSectorId, Guid AssetTypeId)>();

        foreach (var (sector, subSector, asset) in lookupTriples)
        {
            var key = $"{sector.Trim().ToUpperInvariant()}|{subSector.Trim().ToUpperInvariant()}";
            if (!subSectorIdBySectorAndName.TryGetValue(key, out var subSectorId))
            {
                continue;
            }

            if (!assetTypeIdByName.TryGetValue(asset.Trim(), out var assetTypeId))
            {
                continue;
            }

            desiredPairs.Add((subSectorId, assetTypeId));
        }

        if (desiredPairs.Count == 0)
        {
            return;
        }

        var existingPairs = context.SubSectorAssetTypes
            .Select(x => new { x.SubSectorId, x.AssetTypeId })
            .ToList()
            .Select(x => (x.SubSectorId, x.AssetTypeId))
            .ToHashSet();

        var toAdd = new List<SubSectorAssetType>();
        foreach (var (subSectorId, assetTypeId) in desiredPairs)
        {
            if (existingPairs.Contains((subSectorId, assetTypeId)))
            {
                continue;
            }

            var created = SubSectorAssetType.Create(subSectorId, assetTypeId).Value!;
            toAdd.Add(created);
        }

        if (toAdd.Count > 0)
        {
            await context.SubSectorAssetTypes.AddRangeAsync(toAdd);
        }
    }

    private static async Task SeedUnitsOfMeasurementAsync(ConfigurationHubDbContext context)
    {
        var uoms = new[]
        {
            UnitOfMeasurement.Create("SQM", "??? ????", "Square Meter", "?²", 1, SystemUserId).Value!,
            UnitOfMeasurement.Create("KM", "???????", "Kilometer", "??", 2, SystemUserId).Value!,
            UnitOfMeasurement.Create("M", "???", "Meter", "?", 3, SystemUserId).Value!,
            UnitOfMeasurement.Create("UNIT", "????", "Unit", "????", 4, SystemUserId).Value!,
            UnitOfMeasurement.Create("HECTARE", "?????", "Hectare", "?????", 5, SystemUserId).Value!,
            UnitOfMeasurement.Create("TON", "??", "Ton", "??", 6, SystemUserId).Value!,
            UnitOfMeasurement.Create("KG", "????????", "Kilogram", "???", 7, SystemUserId).Value!,
            UnitOfMeasurement.Create("LITER", "???", "Liter", "???", 8, SystemUserId).Value!,
            UnitOfMeasurement.Create("CUBIC_M", "??? ????", "Cubic Meter", "?³", 9, SystemUserId).Value!,
            UnitOfMeasurement.Create("MW", "???????", "Megawatt", "???????", 10, SystemUserId).Value!,
            UnitOfMeasurement.Create("OTHER", "????", "Other", null, 999, SystemUserId).Value!
        };

        var existing = context.UnitsOfMeasurement.ToList();
        var existingByCode = existing.ToDictionary(u => u.Code, u => u, StringComparer.OrdinalIgnoreCase);

        foreach (var desired in uoms)
        {
            if (existingByCode.TryGetValue(desired.Code, out var found))
            {
                found.Update(
                    nameAr: desired.NameAr,
                    nameEn: desired.NameEn,
                    symbol: desired.Symbol,
                    displayOrder: desired.DisplayOrder,
                    updatedBy: SystemUserId);

                if (!found.IsActive)
                {
                    found.Activate(SystemUserId);
                }
            }
            else
            {
                await context.UnitsOfMeasurement.AddAsync(desired);
            }
        }
    }

    private static async Task SeedTermsAndConditionsAsync(ConfigurationHubDbContext context)
    {
        if (context.TermsAndConditions.Any())
        {
            return;
        }

        // Global Terms
        var globalTermsResult = TermsAndCondition.Create(
            version: "1.0",
            type: TermsType.Global,
            titleAr: "???? ?????? ???? ????? ????? ??????????? ??????",
            titleEn: "PIF Partners Hub Terms and Conditions",
            contentAr: @"?????? ?? ?? ???? ????? ????? ??????????? ??????. ????????? ???? ??????? ???? ????? ??? ???????? ??????? ???????? ???????:

1. ?????????
   - ??????: ???? ???? ????? ????? ??????????? ?????? ???????????
   - ????????: ?? ??? ?????? ??????
   - ???????: ???? ????????? ????????? ??????? ??? ??????

2. ??????? ??????
   - ??? ??????? ?????? ??????? ?????? ??? ???
   - ???? ??????? ?????? ??? ????? ??? ???????
   - ??? ?????? ??? ???? ?????? ??????

3. ???????? ?????? ????????
   - ????? ?????? ?????? ???????
   - ??? ??????? ?? ???????? ????? ?????? ????????

4. ??????? ???????
   - ???? ?????? ?????? ?????? ??????????? ??????
   - ???? ??? ?? ????? ????? ??????? ??? ???

5. ?????????
   - ?????? ????? '??? ??'
   - ?? ????? ????????? ?? ?? ????? ??? ??????

?????? ?? ?????????? ???? ??????? ????? ?????.",
            contentEn: @"Welcome to the PIF Partners Hub platform. By using this platform, you agree to comply with the following terms and conditions:

1. Definitions
   - Platform: means the PIF Partners Hub electronic platform
   - User: any person using the platform
   - Content: all information and data available on the platform

2. Use of Platform
   - The platform must be used for authorized purposes only
   - Using the platform for illegal activities is prohibited
   - Login credentials must be kept confidential

3. Privacy and Data Protection
   - We are committed to protecting your privacy
   - Data is handled according to our privacy policy

4. Intellectual Property
   - All rights reserved to the Public Investment Fund
   - Copying or reproducing content without permission is prohibited

5. Liability
   - The platform is provided 'as is'
   - We are not liable for any indirect damages

For more information, please contact the support team.",
            effectiveDate: DateTime.UtcNow,
            requiresAcceptance: true,
            createdBy: SystemUserId
        );

        if (globalTermsResult.IsSuccess)
        {
            var globalTerms = globalTermsResult.Value!;
            globalTerms.Publish(SystemUserId);
            await context.TermsAndConditions.AddAsync(globalTerms);
        }

        // Synergy Terms
        var synergyTermsResult = TermsAndCondition.Create(
            version: "1.0",
            type: TermsType.Synergy,
            titleAr: "???? ?????? ?????? ??????",
            titleEn: "Synergy Program Terms and Conditions",
            contentAr: @"???? ?????? ??????? ?????? ??????:

1. ???? ???? ??? ????????
   - ?????? ?????? ???? ??? ????? ??????? ??? ???????
   - ???? ???????? ?????? ????? ?????? ???????

2. ???? ????????
   - ??? ?? ???? ?????? ?? ????? ????? ??????????? ??????
   - ??? ?????? ?? ?????? ??????????

3. ??? ?????
   - ??? ?? ???? ????? ?????? ?????? ???????
   - ???? ??? ????? ???? ?? ??? ????

4. ??????
   - ??? ?????? ???? ????????? ????????
   - ???? ?????? ??????? ???? ??? ???

5. ???? ??????? ???????
   - ??????? ??????? ???? ????? ??????
   - ???? ?????? ??????? ?????? ???? ???????",
            contentEn: @"Terms and conditions for using the Synergy Program:

1. Program Overview
   - The Synergy Program aims to enhance collaboration between companies
   - The program allows sharing opportunities and success stories

2. Participation Requirements
   - Company must be a PIF partner
   - Identity and authorization must be verified

3. Publishing Opportunities
   - Opportunities must be genuine and executable
   - Publishing misleading or inaccurate content is prohibited

4. Confidentiality
   - Confidentiality of shared information must be respected
   - Sharing confidential information without permission is prohibited

5. Intellectual Property Rights
   - Published content remains the property of the publisher
   - Publisher grants platform a license to display content",
            effectiveDate: DateTime.UtcNow,
            requiresAcceptance: true,
            createdBy: SystemUserId
        );

        if (synergyTermsResult.IsSuccess)
        {
            var synergyTerms = synergyTermsResult.Value!;
            synergyTerms.Publish(SystemUserId);
            await context.TermsAndConditions.AddAsync(synergyTerms);
        }

        // InfraBase Terms
        var infraBaseTermsResult = TermsAndCondition.Create(
            version: "1.0",
            type: TermsType.InfraBase,
            titleAr: "???? ?????? ????????",
            titleEn: "InfraBase Terms and Conditions",
            contentAr: @"???? ?????? ??????? ???? ????????:

1. ????? ?? ??????
   - ???????? ???? ?????? ????? ?????? ???????
   - ????? ????? ????? ??????? ???????

2. ????? ???????
   - ??? ??????? ???? ?????? ????????
   - ??? ????? ????????? ???????
   - ??????? ??? ???????? ?? ?????

3. ?????? ???????
   - ??? ?????? ??????? ?? ??? ?????? ??????
   - ?? ?????? ????? ?? 5-10 ???? ???
   - ???? ?????? ????? ?????

4. ???????? ???????
   - ??? ?? ???? ???????? ??????? ?????
   - ??? ????? ??????? ?????? ??????

5. ?????????
   - ???? ????? ????? ?? ??? ?????????
   - ??? ?????? ??? ??????? ??????",
            contentEn: @"Terms and conditions for using the InfraBase platform:

1. Platform Purpose
   - InfraBase is a platform for managing infrastructure requests
   - Facilitates the process of submitting and reviewing requests

2. Submitting Requests
   - All required fields must be completed
   - Supporting documents must be attached
   - Incomplete requests may be rejected

3. Request Review
   - Requests are reviewed by the specialized team
   - May take 5-10 business days
   - You will be notified of the request status

4. Financial Data
   - Financial data must be accurate
   - Correct financial distribution must be provided

5. Liability
   - Applicant is responsible for information accuracy
   - Platform has the right to request additional information",
            effectiveDate: DateTime.UtcNow,
            requiresAcceptance: true,
            createdBy: SystemUserId
        );

        if (infraBaseTermsResult.IsSuccess)
        {
            var infraBaseTerms = infraBaseTermsResult.Value!;
            infraBaseTerms.Publish(SystemUserId);
            await context.TermsAndConditions.AddAsync(infraBaseTerms);
        }
    }

    private static string ToBaseCode(string name)
        => Regex.Replace(name.Trim().ToUpperInvariant(), @"[^A-Z0-9]+", "_").Trim('_');

    private static string EnsureUniqueCode(string baseCode, HashSet<string> usedCodes)
    {
        const int maxLen = 50;
        if (string.IsNullOrWhiteSpace(baseCode))
            baseCode = "CODE";

        string MakeCandidate(string core, int? suffix)
        {
            var suffixText = suffix.HasValue ? $"_{suffix.Value}" : string.Empty;
            var allowedCoreLen = Math.Max(1, maxLen - suffixText.Length);
            var coreTrimmed = core.Length > allowedCoreLen ? core[..allowedCoreLen] : core;
            return $"{coreTrimmed}{suffixText}";
        }

        var candidate = MakeCandidate(baseCode, null);
        if (usedCodes.Add(candidate))
            return candidate;

        for (var i = 2; i < 10_000; i++)
        {
            candidate = MakeCandidate(baseCode, i);
            if (usedCodes.Add(candidate))
                return candidate;
        }

        // Extremely unlikely fallback
        return MakeCandidate("CODE", Guid.NewGuid().GetHashCode());
    }

    private static List<string> DistinctInOrder(IEnumerable<string> values)
    {
        var list = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in values)
        {
            var value = (raw ?? string.Empty).Trim();
            if (value.Length == 0)
                continue;

            if (seen.Add(value))
                list.Add(value);
        }

        return list;
    }

    private static (string Sector, string SubSector, string Asset)[] GetInfraBaseLookupSeedData()
        => new[]
        {
            // Digital & Technology
            ("Digital & Technology", "Data Center", "Data Centre"),
            ("Digital & Technology", "Data Storage", "Data Storage"),
            ("Digital & Technology", "Data Transmission", "Data Transmission Network"),
            ("Digital & Technology", "Data Transmission", "Telecommunication towers"),
            ("Digital & Technology", "Data Transmission", "Fiber Optics"),
            ("Digital & Technology", "Data Transmission", "Cable‑Only Network"),
            ("Digital & Technology", "Other", "Other"),
            ("Digital & Technology", "N/A" , "N/A" ),

            // Enabling Works
            ("Enabling Works", "Earthwork", "Earthwork"),
            ("Enabling Works", "Ground improvement", "Ground Improvement"),
            ("Enabling Works", "Marine works", "Dredging"),
            ("Enabling Works", "Marine works", "Jetty / Pontoon"),
            ("Enabling Works", "Other", "Other"),
            ("Enabling Works", "N/A" , "N/A" ),

            // Energy & Power
            ("Energy & Power", "EV system", "EV Charging Station"),
            ("Energy & Power", "Power Distribution", "33 kV / 13.8 kV Underground Transmission Line"),
            ("Energy & Power", "Power Distribution", "33 kV Distribution Substation"),
            ("Energy & Power", "Power Distribution", "69 kV Distribution Substation"),
            ("Energy & Power", "Power Distribution", "13.8 kV Secondary Substation"),
            ("Energy & Power", "Power Distribution", "13.8 kV Ring‑Main Unit"),
            ("Energy & Power", "Power Distribution", "LV Equipment (Major Feeders)"),
            ("Energy & Power", "Power Transmission", "HV Network (High‑Voltage Lines)"),
            ("Energy & Power", "Power Transmission", "MV Network (Medium‑Voltage Lines)"),
            ("Energy & Power", "Power Distribution", "LV Network (Low‑Voltage Distribution)"),
            ("Energy & Power", "Power Distribution", "Utility Tunnel / Corridor"),
            ("Energy & Power", "Power Generation", "Fuel‑Based Power Plant"),
            ("Energy & Power", "Power Generation", "Coal‑Based Power Plant"),
            ("Energy & Power", "Power Generation", "Natural‑Gas Power Plant"),
            ("Energy & Power", "Power Generation", "Hydrogen Power Plant"),
            ("Energy & Power", "Power Generation", "Solar Power Plant"),
            ("Energy & Power", "Power Generation", "Wind Power Plant"),
            ("Energy & Power", "Power Generation", "Bio‑Fuel Power Plant"),
            ("Energy & Power", "Power Generation", "Stand‑by Generator"),
            ("Energy & Power", "Power Storage", "Battery Energy Storage System"),
            ("Energy & Power", "Power Transmission", "380 kV Overhead Transmission Line"),
            ("Energy & Power", "Power Transmission", "132 kV Overhead Transmission Line"),
            ("Energy & Power", "Power Transmission", "115 kV Overhead Transmission Line"),
            ("Energy & Power", "Power Transmission", "110 kV Overhead Transmission Line"),
            ("Energy & Power", "Power Transmission", "69 kV Overhead Transmission Line"),
            ("Energy & Power", "Power Transmission", "380 kV Underground Transmission Line"),
            ("Energy & Power", "Power Transmission", "132 kV Underground Transmission Line"),
            ("Energy & Power", "Power Transmission", "115 kV Underground Transmission Line"),
            ("Energy & Power", "Power Transmission", "110 kV Underground Transmission Line"),
            ("Energy & Power", "Power Transmission", "380 kV Bulk Substation"),
            ("Energy & Power", "Power Transmission", "132 kV Step‑Down Substation"),
            ("Energy & Power", "Power Transmission", "110 kV Step‑Down Substation"),
            ("Energy & Power", "Power Transmission", "Sub‑Sea Utility Infrastructure (cable & pipe)"),
            ("Energy & Power", "Other", "Other"),
            ("Energy & Power", "N/A" , "N/A"),

            // Industrial Infrastructure
            ("Industrial Infrastructure", "Fuel / Gas Facility", "Gas stations"),
            ("Industrial Infrastructure", "Industrial Cities & Zones", "Industrial Cities & Zones"),
            ("Industrial Infrastructure", "Manufacturing Plants", "Manufacturing Plants"),
            ("Industrial Infrastructure", "Gas", "Gas Networks"),
            ("Industrial Infrastructure", "Factories", "Factories"),
            ("Industrial Infrastructure", "Other", "Other"),
            ("Industrial Infrastructure", "N/A" , "N/A"),

            // Social Infrastructure
            ("Social Infrastructure", "Public Realm", "Park"),
            ("Social Infrastructure", "Public Realm", "Beach"),
            ("Social Infrastructure", "Public Realm", "Sea Front"),
            ("Social Infrastructure", "Public Realm", "Promenade"),
            ("Social Infrastructure", "Public Realm", "Soft‑scape (Land‑scaping)"),
            ("Social Infrastructure", "Public Realm", "Hard‑scape (Paving, Plazas)"),
            ("Social Infrastructure", "Public Realm", "Open Space"),
            ("Social Infrastructure", "Public Realm", "Sport Field"),
            ("Social Infrastructure", "Other", "Other"),
            ("Social Infrastructure", "N/A" , "N/A"),
            ("Social Infrastructure", "Marine", "Canal"),
            ("Social Infrastructure", "Marine", "Marina Berth"),
            ("Social Infrastructure", "Culture", "Exhibition Centers"),
            ("Social Infrastructure", "Culture", "Museums"),
            ("Social Infrastructure", "Culture", "Libraries"),
            ("Social Infrastructure", "Culture", "Opera Houses"),
            ("Social Infrastructure", "Education", "Private Schools"),
            ("Social Infrastructure", "Education", "Public Schools"),
            ("Social Infrastructure", "Education", "Private Universities"),
            ("Social Infrastructure", "Education", "Public Universities"),
            ("Social Infrastructure", "Education", "Training centers"),
            ("Social Infrastructure", "Education", "Students Accommodation"),
            ("Social Infrastructure", "Government", "Police stations"),
            ("Social Infrastructure", "Government", "Civil defense"),
            ("Social Infrastructure", "Government", "Post office"),
            ("Social Infrastructure", "Government", "Government buildings"),
            ("Social Infrastructure", "Health", "Private Hospitals"),
            ("Social Infrastructure", "Health", "Public Hospitals"),
            ("Social Infrastructure", "Health", "Clinics"),
            ("Social Infrastructure", "Health", "Elderly care"),
            ("Social Infrastructure", "Housing", "Staff Housing"),
            ("Social Infrastructure", "Housing", "Labour Accommodation"),
            ("Social Infrastructure", "Housing", "Construction Villages"),
            ("Social Infrastructure", "Religious", "Mosques"),
            ("Social Infrastructure", "Sport & Enter", "Stadiums"),
            ("Social Infrastructure", "Sport & Enter", "Theme Parks"),
            ("Social Infrastructure", "Sport & Enter", "Sport Centers"),

            // Transportation & Logistics
            ("Transportation & Logistics", "Ports", "Port Facility"),
            ("Transportation & Logistics", "Airports", "Airport Facility (runway & terminal)"),
            ("Transportation & Logistics", "Airports", "Cargo"),
            ("Transportation & Logistics", "Buses", "Stations"),
            ("Transportation & Logistics", "Buses", "Bus Network"),
            ("Transportation & Logistics", "Logistics", "Logistics Centers"),
            ("Transportation & Logistics", "Logistics", "Special Economic Zone"),
            ("Transportation & Logistics", "Logistics", "Storges"),
            ("Transportation & Logistics", "Ports", "Dry Ports"),
            ("Transportation & Logistics", "Ports", "Sea Ports"),
            ("Transportation & Logistics", "Rail", "Monorail"),
            ("Transportation & Logistics", "Rail", "Passenger"),
            ("Transportation & Logistics", "Rail", "Metro"),
            ("Transportation & Logistics", "Roads", "Highways"),
            ("Transportation & Logistics", "Roads", "Road Network (including upgrades)"),
            ("Transportation & Logistics", "Roads", "Interchange"),
            ("Transportation & Logistics", "Roads", "Bridge / Underpass / Culvert Structure"),
            ("Transportation & Logistics", "Roads", "Tunnel"),
            ("Transportation & Logistics", "Car Parking", "Car Parking"),
            ("Transportation & Logistics", "Other", "Other"),
            ("Transportation & Logistics", "N/A" , "N/A"),

            // Water & Waste
            ("Water & Waste", "District cooling", "District‑Cooling Plant"),
            ("Water & Waste", "District cooling", "Chilled‑Water Transmission Network"),
            ("Water & Waste", "District cooling", "Chilled‑Water Distibution Network"),
            ("Water & Waste", "District cooling", "Existing District‑Cooling Upgrade"),
            ("Water & Waste", "Irrigation", "Irrigation Transmission Network"),
            ("Water & Waste", "Irrigation", "Irrigation Distribution Network"),
            ("Water & Waste", "Potable water", "Sea‑Water RO Desalination Plant"),
            ("Water & Waste", "Potable water", "Well‑Water RO Desalination Plant"),
            ("Water & Waste", "Potable water", "Potable‑Water Transmission Network"),
            ("Water & Waste", "Potable water", "Existing Water‑Treatment Plant Upgrade"),
            ("Water & Waste", "Potable water", "Potable‑Water Chlorination Plant"),
            ("Water & Waste", "Potable water", "Potable‑Water Distribution Network"),
            ("Water & Waste", "Sewage", "Sewage‑Treatment Plant"),
            ("Water & Waste", "Sewage", "Sewage Transmission Network"),
            ("Water & Waste", "Sewage", "Sewage Distribution Network"),
            ("Water & Waste", "Sewage", "Existing Sewage‑Treatment Plant Upgrade"),
            ("Water & Waste", "Solid waste", "Solid Waste Transfer Station"),
            ("Water & Waste", "Solid waste", "Solid Waste Treatment Plant"),
            ("Water & Waste", "Solid waste", "Automated Waste Collection System"),
            ("Water & Waste", "Storm water", "Storm‑Water Storage Pond"),
            ("Water & Waste", "Storm water", "Bioretention Wetland System"),
            ("Water & Waste", "Storm water", "Storm‑Water Network"),
            ("Water & Waste", "Other", "Other"),
            ("Water & Waste", "N/A" , "N/A"),

            // Other
            ("Other", "Other", "Other"),

            // N/A
            ("N/A" , "N/A" , "N/A")
        };
}