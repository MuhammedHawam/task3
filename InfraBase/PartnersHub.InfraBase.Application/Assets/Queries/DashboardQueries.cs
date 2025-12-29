using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Queries;

/// <summary>
/// Query to get PC Contributor dashboard data
/// User Story: "As PC contributor, I want to view home page – landing page"
/// </summary>
public record GetContributorDashboardQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    AssetStatuses? StatusFilter = null) : IRequest<ContributorDashboardDto>;

/// <summary>
/// Query to get PC Admin dashboard data (own assets)
/// User Story: "As PC admin, I want to view home page – landing page"
/// </summary>
public record GetPcAdminDashboardQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    AssetStatuses? StatusFilter = null) : IRequest<PcAdminDashboardDto>;

/// <summary>
/// Query to get team assets dashboard data
/// User Story: "User will be able to view 'Team assets'"
/// </summary>
public record GetTeamAssetsDashboardQuery(
    Guid CompanyId,
    Guid UserId,  // PC Admin user ID to exclude their assets
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    AssetStatuses? StatusFilter = null) : IRequest<TeamAssetsDashboardDto>;

/// <summary>
/// Query to get Infrabase Admin dashboard data
/// User Story: "As infrabase admin, I want to view home page – landing page"
/// </summary>
public record GetInfrabaseAdminDashboardQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    AssetStatuses? StatusFilter = null) : IRequest<InfrabaseAdminDashboardDto>;
