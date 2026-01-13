using Dapper;
using PartnersHub.Synergy.Application.Common.Helpers;
using PartnersHub.Synergy.Application.Dashboard.DTOs;
using PartnersHub.Synergy.Application.Interfaces.Repository.Dapper;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.SuccessStories.Commands;
using PartnersHub.Synergy.Application.SuccessStories.DTOs;
using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;
using PartnersHub.Synergy.Domain.Common;
using PartnersHub.Synergy.Infrastructure.Persistence.Interfaces;
using System.Data;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PartnersHub.Synergy.Infrastructure.Repositories.Dapper
{
    public class DapperRepository : IDapperRepository
    {
        private readonly IDapperConnectionFactory _connectionFactory;

        public DapperRepository(IDapperConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<(List<SuccessStoryResponseDto> Items, int TotalCount)> Search(
            int pageNumber,
            int pageSize,
            List<Guid>? companyIds = null,
            string? partnerCompanyName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            SuccessStoryStatus? status = null,
            List<Guid>? sectorIds = null,
            List<int>? collaborationTypeIds = null,
            string? searchTerm = null,
            string? sortBy = null,
            bool asNoTracking = false)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@PageSize", pageSize);
            parameters.Add("@Offset", (pageNumber - 1) * pageSize);

            using var connection = _connectionFactory.CreateConnection();
            var whereConditions = new List<string>();

            #region Filters

            if (companyIds?.Any() == true)
            {
                whereConditions.Add(@"
            (s.CompanyId IN @CompanyIds OR EXISTS (
                SELECT 1 FROM SuccessStorySynergyCompanies ss2
                WHERE ss2.SuccessStoryId = s.Id
                AND ss2.SynergyCompanyId IN @CompanyIds
            ))");
                parameters.Add("@CompanyIds", companyIds);
            }

            if (!string.IsNullOrWhiteSpace(partnerCompanyName))
            {
                whereConditions.Add(@"
            EXISTS (
                SELECT 1
                FROM SuccessStorySynergyCompanies ssc
                JOIN SynergyCompanies sc ON sc.Id = ssc.SynergyCompanyId
                WHERE ssc.SuccessStoryId = s.Id
                AND LOWER(sc.Name) = LOWER(@PartnerCompanyName)
            )");
                parameters.Add("@PartnerCompanyName", partnerCompanyName.Trim());
            }

            if (startDate.HasValue)
            {
                whereConditions.Add("s.StartDate >= @StartDate");
                parameters.Add("@StartDate", startDate.Value);
            }

            if (endDate.HasValue)
            {
                whereConditions.Add("s.EndDate <= @EndDate");
                parameters.Add("@EndDate", endDate.Value);
            }

            if (status.HasValue)
            {
                whereConditions.Add("s.Status = @Status");
                parameters.Add("@Status", (int)status.Value);
            }

            if (sectorIds?.Any() == true)
            {
                whereConditions.Add(@"
            EXISTS (
                SELECT 1 FROM SynergyCompanySectors scs
                WHERE scs.CompanyId = s.CompanyId
                AND scs.SectorId IN @SectorIds
            )");
                parameters.Add("@SectorIds", sectorIds);
            }

            if (collaborationTypeIds?.Any() == true)
            {
                whereConditions.Add("s.SuccessStoryTypeId IN @CollaborationTypeIds");
                parameters.Add("@CollaborationTypeIds", collaborationTypeIds);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                whereConditions.Add(@"
            (LOWER(s.Title) LIKE LOWER(@SearchTerm)
             OR LOWER(s.Description) LIKE LOWER(@SearchTerm)
             OR LOWER(c.Name) LIKE LOWER(@SearchTerm)
             OR LOWER(OT.Name) LIKE LOWER(@SearchTerm))");
                parameters.Add("@SearchTerm", $"%{searchTerm.Trim()}%");
            }

            #endregion

            var whereClause = whereConditions.Any()
                ? "WHERE " + string.Join(" AND ", whereConditions)
                : string.Empty;


            var (field, descending) = ParseHelper.ParseSort(sortBy);

            var orderByField = field?.ToLower() switch
            {
                "title" => "s.Title",
                "status" => "s.Status",
                "id" => "s.Id",
                "startdate" => "s.StartDate",
                "enddate" => "s.EndDate",
                "submissiondate" => "s.CreatedAt",
                _ => "s.CreatedAt"
            };

            var sortOrder = descending ? "DESC" : "ASC";


            #region Count Query

            var countSql = $@"
        SELECT COUNT(DISTINCT s.Id)
        FROM SuccessStories s
        JOIN SynergyCompanies c ON c.Id = s.CompanyId
        LEFT JOIN SuccessStoryOpportunities sso ON sso.SuccessStoryId = s.Id
        LEFT JOIN Opportunities o ON o.Id = sso.OpportunityId
        LEFT JOIN OpportunityTypes OT on OT.Id = o.OpportunityTypeId
        {whereClause};";

            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            if (totalCount == 0)
                return (new List<SuccessStoryResponseDto>(), 0);

            #endregion

            #region Data Query (ROW_NUMBER FIX)

            var sql = $@"
    WITH RankedStories AS (
        SELECT
            s.Id,
            ROW_NUMBER() OVER (ORDER BY {orderByField} {sortOrder}) AS RowNum
        FROM SuccessStories s
        JOIN SynergyCompanies c ON c.Id = s.CompanyId
        LEFT JOIN SuccessStoryOpportunities sso ON sso.SuccessStoryId = s.Id
        LEFT JOIN Opportunities o ON o.Id = sso.OpportunityId
        LEFT JOIN OpportunityTypes OT on OT.Id = o.OpportunityTypeId

        {whereClause}
    )
    SELECT
        s.Id,
        s.Title,
        s.Description,
        s.StartDate,
        s.EndDate,
        s.RequestId,
        s.IsHide,
        s.Status AS SuccessStoryStatus,
        s.CreatedAt AS SubmissionDate,
        s.RejectionReason AS RejectionReason, 
        s.CollaborationStatusId as CollaborationStatus,
        OT.Name as OpportunityTypeName,

        c.Id AS CompanyId,
        c.Name AS CompanyName,

        t.Name AS SuccessStoryType,

        sc.Id AS CollabId,
        sc.Name AS CollabName,
        sc.Logo as Logo,

        o.Id AS OppId,
        o.Title AS OppName,

        eo.Id AS EOId,
        eo.Name AS EOName,

        s.SectorId AS SectorId,
        s.SectorName AS SectorName

        

    FROM RankedStories rs
    JOIN SuccessStories s ON s.Id = rs.Id
    JOIN SynergyCompanies c ON c.Id = s.CompanyId
    JOIN SuccessStoryTypes t ON t.Id = s.SuccessStoryTypeId

    LEFT JOIN SuccessStorySynergyCompanies ssc ON ssc.SuccessStoryId = s.Id
    LEFT JOIN SynergyCompanies sc ON sc.Id = ssc.SynergyCompanyId

    LEFT JOIN SuccessStoryOpportunities sso ON sso.SuccessStoryId = s.Id
    LEFT JOIN Opportunities o ON o.Id = sso.OpportunityId
    LEFT JOIN OpportunityTypes OT on OT.Id = o.OpportunityTypeId

    LEFT JOIN OpportunityExpectedOutcomes oe ON oe.OpportunityId = o.Id
    LEFT JOIN ExpectedOutcomes eo ON eo.Id = oe.ExpectedOutcomeId



    WHERE   rs.RowNum > @Offset
      AND rs.RowNum <= (@Offset + @PageSize)
       

    ORDER BY {orderByField} {sortOrder};";

            #endregion

            var lookup = new Dictionary<Guid, SuccessStoryResponseDto>();

            await connection.QueryAsync<
                SuccessStoryResponseDto,
                dynamic,
                dynamic,
                dynamic,
                
                SuccessStoryResponseDto>(
                sql,
                (story, collab, opp, eo) =>
                {
                    if (!lookup.TryGetValue(story.Id, out var current))
                    {
                        current = story;
                        current.CollaboratingPartners = new();
                        current.AssociatedOpportunities = new();
                        current.ExpectedOutcomes = new();
                        current.Sectors = new();
                        current.SuccessStoryStatusDescription =
                            MapStatusToDisplay(current.SuccessStoryStatus);
                        current.CollaborationStatusDescription = MapSuccessStroyCollaborationToDisplay(current.CollaborationStatus);

                        lookup.Add(current.Id, current);
                    }

                    if (collab?.CollabId != null && collab.CollabId != Guid.Empty &&
                        !current.CollaboratingPartners.Any(x => x.Id == collab.CollabId))
                        current.CollaboratingPartners.Add(new(collab.CollabId, collab.CollabName, collab.Logo));

                    if (opp?.OppId != null && opp.OppId != Guid.Empty &&
                        !current.AssociatedOpportunities.Any(x => x.Id == opp.OppId))
                        current.AssociatedOpportunities.Add(new(opp.OppId, opp.OppName));

                    if (eo?.EOId > 0 &&
                        !current.ExpectedOutcomes.Any(x => x.Id == eo.EOId))
                        current.ExpectedOutcomes.Add(new(eo.EOId, eo.EOName));

                    //if (sector?.CompSecId != null && sector.CompSecId != Guid.Empty &&
                    //    !current.Sectors.Any(x => x.Id == sector.CompSecId))
                    //    current.Sectors.Add(new(sector.CompSecId, sector.CompSecName));

                    return current;
                },
                parameters,
                splitOn: "CollabId,OppId,EOId"
            );

            return (lookup.Values.ToList(), totalCount);
        }


        public async Task<List<SuccessStoryResponseDto>> GetByStatusAsync(SuccessStoryStatus status)
        {
            string statusValue = status.ToString();
            var parameters = new { Status = statusValue };
            var _connection = _connectionFactory.CreateConnection();
            string singleQuery = @"
        SELECT
            s.Id, s.Title, s.Description, s.StartDate, s.EndDate, s.Status AS SuccessStoryStatus,
            c.Id AS CompanyId, c.Name AS CompanyName,
            t.Name AS SuccessStoryType,
            -- Collaborator fields (T2 mapping)
            sc.Id AS CollabId, sc.Name AS CollabName,sc.Logo AS Logo,
            -- Opportunity fields (T3 mapping)
            o.Id AS OppId, o.Title AS OppName,
            -- Expected Outcome fields (T4 mapping)
            eo.Id AS EOId, eo.Name AS EOName, 
            -- Company Sector fields (T5 mapping)
            scs.SectorId AS CompSecId, scs.SectorName AS CompSecName ,
           -- Thematic Area fields (T6 mapping)
            ta.Id AS TAId, ta.Name AS TAName
        FROM SuccessStories s
        JOIN SynergyCompanies c ON s.CompanyId = c.Id
        JOIN SuccessStoryTypes t ON s.SuccessStoryTypeId = t.Id
        
        -- Join for Collaborators (sc)
        LEFT JOIN SuccessStorySynergyCompanies ssc ON ssc.SuccessStoryId = s.Id
        LEFT JOIN SynergyCompanies sc ON ssc.SynergyCompanyId = sc.Id
        
        -- Join for Opportunities (o)
        LEFT JOIN SuccessStoryOpportunities sso ON sso.SuccessStoryId = s.Id
        LEFT JOIN Opportunities o ON sso.OpportunityId = o.Id
        
        -- Join for Expected Outcomes (eo) via Opportunity (o)
        LEFT JOIN OpportunityExpectedOutcomes oe ON oe.OpportunityId = o.Id
        LEFT JOIN ExpectedOutcomes eo ON oe.ExpectedOutcomeId = eo.Id
        
        -- Join for the Success Story Company's Sectors (scs)
        LEFT JOIN SynergyCompanySectors scs ON scs.CompanyId = c.Id 
        LEFT JOIN ThematicAreas ta ON ta.Id = o.ThematicAreaId
        WHERE s.Status = @Status -- Filter by status only
        ORDER BY s.CreatedAt DESC; -- Default sorting
    ";


            var storyDictionary = new Dictionary<Guid, SuccessStoryResponseDto>();

            // The mapping function needs 5 types after the main DTO (SuccessStoryResponseDto):
            // 1. Collaborator (dynamic)
            // 2. Opportunity (dynamic)
            // 3. Expected Outcome (dynamic)
            // 4. Sector (dynamic)
            var result = await _connection.QueryAsync<
                SuccessStoryResponseDto,
                dynamic, // Collaborator
                dynamic, // Opportunity
                dynamic, // Expected Outcome
                dynamic, // Sector
                dynamic, // Thematic Area
                SuccessStoryResponseDto>(
                singleQuery,
                (story, collaboratorDynamic, opportunityDynamic, expectedOutcomesDynamic, sectorsDynamic, thematicAreaDynamic) =>
                {
                    if (!storyDictionary.TryGetValue(story.Id, out var currentStory))
                    {
                        currentStory = story;
                        currentStory.CollaboratingPartners = new List<PatnerCompany>();
                        currentStory.AssociatedOpportunities = new List<GuidKeyValueDto>();
                        currentStory.ExpectedOutcomes = new List<KeyValueDto>();
                        currentStory.Sectors = new List<GuidKeyValueDto>();
                        currentStory.ThematicAreas = new List<KeyValueDto>();
                        
                        currentStory.SuccessStoryStatusDescription = MapStatusToDisplay(currentStory.SuccessStoryStatus);
                        // Status conversion
                        //if (currentStory.SuccessStoryStatus != null && int.TryParse(currentStory.SuccessStoryStatus, out int statusInt))
                        //{
                        //    currentStory.SuccessStoryStatus = ((SuccessStoryStatus)statusInt).ToString();
                        //    currentStory.SuccessStoryStatusDescription = MapStatusToDisplay((SuccessStoryStatus)statusInt);
                        //}

                        storyDictionary.Add(currentStory.Id, currentStory);
                    }

                    if (collaboratorDynamic?.CollabId != null && collaboratorDynamic.CollabId != Guid.Empty)
                    {
                        Guid collabId = collaboratorDynamic.CollabId;
                        string collabName = collaboratorDynamic.CollabName;
                        byte[]? Logo = collaboratorDynamic.Logo;

                        if (!currentStory.CollaboratingPartners.Any(cp => cp.Id == collabId))
                        {
                            currentStory.CollaboratingPartners.Add(new PatnerCompany(collabId, collabName, Logo));
                        }
                    }

                    if (sectorsDynamic?.CompSecId != null && sectorsDynamic.CompSecId != Guid.Empty)
                    {
                        Guid sectorId = sectorsDynamic.CompSecId;
                        string sectorName = sectorsDynamic.CompSecName;

                        if (!currentStory.Sectors.Any(s => s.Id == sectorId))
                        {
                            currentStory.Sectors.Add(new GuidKeyValueDto(sectorId, sectorName));
                        }
                    }

                    if (opportunityDynamic?.OppId != null && opportunityDynamic.OppId != Guid.Empty)
                    {
                        Guid oppId = opportunityDynamic.OppId;
                        string oppName = opportunityDynamic.OppName;

                        if (!currentStory.AssociatedOpportunities.Any(op => op.Id == oppId))
                        {
                            currentStory.AssociatedOpportunities.Add(new GuidKeyValueDto(oppId, oppName));
                        }


                        if (expectedOutcomesDynamic != null && expectedOutcomesDynamic.EOId > default(int))
                        {
                            int eoId = expectedOutcomesDynamic.EOId;
                            string eoName = expectedOutcomesDynamic.EOName;



                            if (!currentStory.ExpectedOutcomes.Any(eo => eo.Id == eoId))
                            {

                                currentStory.ExpectedOutcomes.Add(new KeyValueDto(eoId, eoName));
                            }
                        }
                        if (thematicAreaDynamic != null && thematicAreaDynamic.TAId > default(int))
                        {
                            int taId = thematicAreaDynamic.TAId;
                            string taName = thematicAreaDynamic.TAName;

                            if (!currentStory.ThematicAreas.Any(ta => ta.Id == taId))
                            {
                                currentStory.ThematicAreas.Add(new KeyValueDto(taId, taName));
                            }
                        }
                    }

                    return currentStory;
                },
                parameters,
                // The splitOn value must match the new aliases for the ID fields in the SELECT statement.
                splitOn: "CompanyId,CollabId,OppId,EOId,CompSecId,TAId"
            );

            return storyDictionary.Values.ToList();
        }
        public async Task<SuccessStoryResponseDto?> GetByIdAsync(Guid id)
        {
            var _connection = _connectionFactory.CreateConnection();
            const string mainQuery = @"
        SELECT 
      s.Id AS Id, s.Title, s.Description, s.StartDate, s.EndDate, s.RejectionReason,
      s.Status AS SuccessStoryStatus,
      c.Id AS CompanyId, c.Name AS CompanyName, c.Logo as Logo,
      t.Name AS SuccessStoryType , s.SuccessStoryTypeId as SuccessStoryTypeId,
	  s.CollaborationStatusId as SuccessStoryCollaborationStatusId
  FROM SuccessStories s
  JOIN SynergyCompanies c ON s.CompanyId = c.Id
  JOIN SuccessStoryTypes t ON s.SuccessStoryTypeId = t.Id
  WHERE s.Id = @Id;
    ";

            // Query 2: Collaborating Partners
            const string collaboratorsQuery = @"
        SELECT 
            sc.Id, sc.Name ,sc.Logo
        FROM SuccessStorySynergyCompanies ssc
        JOIN SynergyCompanies sc ON ssc.SynergyCompanyId = sc.Id
        WHERE ssc.SuccessStoryId = @Id;
    ";

            // Query 3: Associated Opportunities (requires joining to the Opportunities table)
            const string opportunitiesQuery = @"
        SELECT 
         o.Id, o.Title AS Name , o.Description , o.OpportunityTypeId , t.Name as OpportunityTypeName,
	     o.CompanyId , c.Name as CompanyName, c.Logo as CompanyLogo, o.SectorId , o.SectorName , o.StartDate , o.EndDate
        FROM SuccessStoryOpportunities sso
        JOIN Opportunities o ON sso.OpportunityId = o.Id
        JOIN OpportunityTypes t on t.Id = o.OpportunityTypeId
        LEFT JOIN SynergyCompanies C on C.Id = o.CompanyId
        WHERE sso.SuccessStoryId = @Id;
        ";
            const string expectedOutcomeQuery = @"
            SELECT 
                eo.Id, eo.Name 
            FROM SuccessStoryOpportunities sso
            JOIN Opportunities o ON sso.OpportunityId = o.Id
            -- Join from Opportunity to its Expected Outcomes junction table
            JOIN OpportunityExpectedOutcomes oe ON oe.OpportunityId = o.Id
            -- Join to the Expected Outcomes lookup table
            JOIN ExpectedOutcomes eo ON oe.ExpectedOutcomeId = eo.Id
            WHERE sso.SuccessStoryId = @Id
            GROUP BY eo.Id, eo.Name; -- Group to avoid duplicates if multiple opportunities share the same outcome
            ";

            // Query 5: Attachments
            const string attachmentsQuery = @"
        SELECT 
            Id, SuccessStoryId, FileName, SharePointUrl, FileExtension, 
            FileSizeInBytes, UploadedAt, UploadedBy
        FROM SuccessStoryAttachments
        WHERE SuccessStoryId = @Id
        ORDER BY UploadedAt DESC;
    ";
            const string sectorQuery = @"
       		Select CS.SectorName AS Name, CS.SectorId AS Id from SuccessStories s
		join SynergyCompanySectors CS on CS.CompanyId = s.CompanyId
		where s.Id = @Id;
    ";
            // Combine queries for a single database call
            string fullSql = mainQuery + collaboratorsQuery + opportunitiesQuery + expectedOutcomeQuery + attachmentsQuery + sectorQuery;

            SuccessStoryResponseDto? resultDto;

            // Use QueryMultiple for maximum efficiency (single round trip), no data redundancy.
            await using (var multi = await _connection.QueryMultipleAsync(fullSql, new { Id = id }))
            {
                //Map the main SuccessStory DTO
                var mainStory = await multi.ReadFirstOrDefaultAsync<SuccessStoryResponseDto>();

                if (mainStory == null)
                {
                    return null;
                }

                var collaborators = await multi.ReadAsync<PatnerCompany>();
                mainStory.CollaboratingPartners = collaborators.ToList();
                var opportunities = await multi.ReadAsync<OpportunityStoryDto>();
                mainStory.AssociatedOpportunitiesList = opportunities.ToList();

                var expectedOutcomes = await multi.ReadAsync<KeyValueDto>();
                mainStory.ExpectedOutcomes = expectedOutcomes.ToList();

                var attachments = await multi.ReadAsync<SuccessStoryAttachmentDto>();
                mainStory.Attachments = attachments.ToList();

                mainStory.SuccessStoryStatusDescription = MapStatusToDisplay(mainStory.SuccessStoryStatus);

                //if (mainStory.SuccessStoryStatus != null && int.TryParse(mainStory.SuccessStoryStatus, out int statusInt))
                //{
                //    mainStory.SuccessStoryStatus = ((SuccessStoryStatus)statusInt).ToString();
                //    mainStory.SuccessStoryStatusDescription = MapStatusToDisplay((SuccessStoryStatus)statusInt);
                //}
                var sectors = await multi.ReadAsync<GuidKeyValueDto>();
               mainStory.Sectors = sectors.ToList();

                resultDto = mainStory;
            }

            return resultDto;
        }
        public async Task<List<EngagementTrend>> FetchYTDMonthlyEngagementTrends()
        {
            var currentYear = DateTime.Today.Year;
            var currentMonth = DateTime.Today.Month;

            var ytdMonths = Enumerable.Range(1, currentMonth)
                .Select(m => new
                {
                    MonthNumber = m,
                    MonthName = new DateTime(currentYear, m, 1).ToString("MMM")
                })
                .ToDictionary(m => m.MonthNumber, m => m.MonthName);

            var sql = @"
                WITH MonthlyCounts AS (
                    -- Opportunities Count
                    SELECT 
                        MONTH(ApprovedAt) AS MonthNumber, 
                        COUNT(Id) AS TotalOpportunitiesCount,
                        0 AS TotalSuccessStoriesCount
                    FROM Opportunities (NOLOCK)
                    WHERE [Status] = @PublishedStatusOpp
                    AND YEAR(ApprovedAt) = @CurrentYear
                    GROUP BY MONTH(ApprovedAt)

                    UNION ALL

                    -- Success Stories Count
                    SELECT 
                        MONTH(ApprovedAt) AS MonthNumber, 
                        0 AS TotalOpportunitiesCount,
                        COUNT(Id) AS TotalSuccessStoriesCount
                    FROM SuccessStories (NOLOCK)
                    WHERE [Status] = @PublishedStatusSS
                    AND YEAR(ApprovedAt) = @CurrentYear
                    GROUP BY MONTH(ApprovedAt)
                )
                SELECT 
                    MonthNumber,
                    SUM(TotalOpportunitiesCount) AS TotalOpportunitiesCount,
                    SUM(TotalSuccessStoriesCount) AS TotalSuccessStoriesCount
                FROM MonthlyCounts
                GROUP BY MonthNumber
                ORDER BY MonthNumber;
            ";

            var parameters = new
            {
                CurrentYear = currentYear,
                PublishedStatusOpp = OpportunityStatus.Published,
                PublishedStatusSS = SuccessStoryStatus.Published
            };

            var trends = new List<EngagementTrend>();

            using (var connection = _connectionFactory.CreateConnection())
            {
                var dbResults = await connection.QueryAsync<dynamic>(sql, parameters);

                // 3. Combine with YTD template to fill in zero counts (GroupJoin equivalent)
                foreach (var month in ytdMonths)
                {
                    var result = dbResults.FirstOrDefault(r => r.MonthNumber == month.Key);

                    trends.Add(new EngagementTrend
                    {
                        Month = month.Value,
                        TotalOpportunitiesCount = result?.TotalOpportunitiesCount ?? 0,
                        TotalSuccessStoriesCount = result?.TotalSuccessStoriesCount ?? 0
                    });
                }
            }

            return trends;
        }

        public async Task<List<CompanyKPI>> FetchCompanyKPIsAsync(DateTime? startDate = null, int? topCount = null)
        {
            var yearParam = startDate.HasValue ? startDate.Value.Year : (int?)null;

            // Uses a CTE to combine and sum counts for both Opportunities and Success Stories
            var sql = new StringBuilder();
            sql.Append(@"
        WITH CompanyCounts AS (
            -- Opportunities Count
            SELECT CompanyId, COUNT(Id) AS OppCount, 0 AS SSCount
            FROM Opportunities (NOLOCK)
            WHERE [Status] = @PublishedStatusOpp
            AND (@Year IS NULL OR YEAR(CreatedAt) = @Year)
            GROUP BY CompanyId

            UNION ALL

            -- Success Stories Count
            SELECT CompanyId, 0 AS OppCount, COUNT(Id) AS SSCount
            FROM SuccessStories (NOLOCK)
            WHERE [Status] = @PublishedStatusSS
            AND (@Year IS NULL OR YEAR(CreatedAt) = @Year)
            GROUP BY CompanyId
        )
        SELECT
            c.[Name] AS CompanyName,
            SUM(cc.OppCount) AS TotalOpportunitiesCount,
            SUM(cc.SSCount) AS TotalSuccessStoriesCount
        FROM CompanyCounts cc
        INNER JOIN SynergyCompanies c (NOLOCK) ON c.Id = cc.CompanyId
        GROUP BY c.Id, c.[Name]
        -- FIX: Use the full aggregate expressions in the ORDER BY clause 
        -- to prevent 'Invalid column name' error when using OFFSET/FETCH
        ORDER BY SUM(cc.OppCount) + SUM(cc.SSCount) DESC
    ");

            if (topCount.HasValue)
            {
                // SQL Server Paging Syntax requires an ORDER BY clause, which is now explicitly defined.
                sql.Append($" OFFSET 0 ROWS FETCH NEXT {topCount.Value} ROWS ONLY");
            }

            var parameters = new
            {
                Year = yearParam,
                // Using ToString() for published status, consistent with prior fix
                PublishedStatusOpp = OpportunityStatus.Published,
                PublishedStatusSS = SuccessStoryStatus.Published
            };

            using (var connection = _connectionFactory.CreateConnection())
            {
                var result = await connection.QueryAsync<CompanyKPI>(sql.ToString(), parameters);
                return result.ToList();
            }
        }
        public async Task<List<SectorKPI>> FetchSectorsKPIs(DateTime? startDate = null)
        {
            var yearParam = startDate.HasValue ? startDate.Value.Year : (int?)null;
            //change createdAt to publishedDate 
            var sql = @"
        WITH OppsBySector AS (
            SELECT 
                o.SectorId AS SectorId, 
                o.SectorName AS SectorName, 
                COUNT(o.Id) AS TotalOpportunitiesCount,
                0 AS TotalSuccessStoriesCount
            FROM Opportunities o (NOLOCK)
            WHERE o.[Status] = @PublishedStatusOpp
            AND (@Year IS NULL OR YEAR(o.ApprovedAt) = @Year)
            GROUP BY o.SectorId, o.SectorName
        ),
        SSBySector AS (
            SELECT 
                scs.SectorId, 
                scs.SectorName, 
                0 AS TotalOpportunitiesCount, 
                COUNT(DISTINCT ss.Id) AS TotalSuccessStoriesCount
            FROM SuccessStories ss (NOLOCK)
            INNER JOIN SynergyCompanySectors scs (NOLOCK) ON ss.CompanyId = scs.CompanyId
            WHERE ss.[Status] = @PublishedStatusSS
            AND (@Year IS NULL OR YEAR(ss.ApprovedAt) = @Year)
            GROUP BY scs.SectorId, scs.SectorName
        )
        -- Combine and finalize the results, summing counts from both sources
        SELECT 
            Combined.SectorId,
            Combined.SectorName,
            SUM(Combined.TotalOpportunitiesCount) AS TotalOpportunitiesCount,
            SUM(Combined.TotalSuccessStoriesCount) AS TotalSuccessStoriesCount
        FROM (
            SELECT * FROM OppsBySector
            UNION ALL
            SELECT * FROM SSBySector
        ) AS Combined
        GROUP BY Combined.SectorId, Combined.SectorName
        ORDER BY SUM(Combined.TotalOpportunitiesCount) + SUM(Combined.TotalSuccessStoriesCount) DESC;
    ";

            var parameters = new
            {
                Year = yearParam,
                // FIX: Convert the enum to its string name (e.g., "Published")
                // The .ToString() method on the enum will yield the string name.
                PublishedStatusOpp = OpportunityStatus.Published,
                PublishedStatusSS = SuccessStoryStatus.Published
            };

            using (var connection = _connectionFactory.CreateConnection())
            {
                var result = await connection.QueryAsync<SectorKPI>(sql, parameters);
                return result.ToList();
            }
        }

        public async Task<List<CollaborationTypeKPI>> FetchCollaborationTypeKPIs(DateTime? startDate = null)
        {
            var yearParam = startDate.HasValue ? startDate.Value.Year : (int?)null;

            // Single query to count opportunities by type and join the type name
            var sql = @"
                SELECT 
                    o.OpportunityTypeId AS CollaborationTypeId,
                    ot.[Name] AS CollaborationTypeName,
                    COUNT(o.Id) AS PublishedOpportunitiesCount
                FROM Opportunities o (NOLOCK)
                INNER JOIN OpportunityTypes ot (NOLOCK) ON o.OpportunityTypeId = ot.Id
                WHERE o.[Status] = @PublishedStatus
                AND (@Year IS NULL OR YEAR(o.ApprovedAt) = @Year)
                GROUP BY o.OpportunityTypeId, ot.[Name]
                ORDER BY PublishedOpportunitiesCount DESC;
            ";

            var parameters = new
            {
                Year = yearParam,
                PublishedStatus = OpportunityStatus.Published
            };

            using (var connection = _connectionFactory.CreateConnection())
            {
                var result = await connection.QueryAsync<CollaborationTypeKPI>(sql, parameters);
                return result.ToList();
            }
        }
        private string MapStatusToDisplay(SuccessStoryStatus status)
        {
            return status switch
            {
                SuccessStoryStatus.PendingReview => "Pending",
                SuccessStoryStatus.pending => "Pending",
                SuccessStoryStatus.Published => "Published",
                SuccessStoryStatus.AssetManagerRejected => "Rejected",
                SuccessStoryStatus.AdminRejected => "Rejected",
                _ => "Draft"
            };
        }

        private string MapSuccessStroyCollaborationToDisplay(SuccessStroyCollaborationStatus status)
        {
            return status switch
            {
                SuccessStroyCollaborationStatus.Ongoing => "Ongoing",
                SuccessStroyCollaborationStatus.Successful => "Successful"
            };
        }
    }
}
