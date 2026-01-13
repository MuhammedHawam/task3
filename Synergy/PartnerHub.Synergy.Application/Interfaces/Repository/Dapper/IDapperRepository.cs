using PartnersHub.Synergy.Application.Dashboard.DTOs;
using PartnersHub.Synergy.Application.SuccessStories.DTOs;
using PartnersHub.Synergy.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.Interfaces.Repository.Dapper
{
    public interface IDapperRepository
    {
        Task<SuccessStoryResponseDto?> GetByIdAsync(Guid id);

        Task<List<SuccessStoryResponseDto>> GetByStatusAsync(SuccessStoryStatus status);
        Task<(List<SuccessStoryResponseDto> Items, int TotalCount)> Search(
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
          bool asNoTracking = false);
        Task<List<EngagementTrend>> FetchYTDMonthlyEngagementTrends();
        Task<List<CompanyKPI>> FetchCompanyKPIsAsync(DateTime? startDate = null, int? topCount = null);
        Task<List<SectorKPI>> FetchSectorsKPIs(DateTime? startDate = null);
        Task<List<CollaborationTypeKPI>> FetchCollaborationTypeKPIs(DateTime? startDate = null);
    }
}
