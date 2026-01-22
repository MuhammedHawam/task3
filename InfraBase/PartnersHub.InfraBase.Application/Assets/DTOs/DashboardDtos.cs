using PartnersHub.InfraBase.Application.Common.Models;

namespace PartnersHub.InfraBase.Application.Assets.DTOs;

// ========== PC Contributor Dashboard ==========

/// <summary>
/// Dashboard data for PC Contributors
/// User Story: "As PC contributor, I want to view home page – landing page"
/// </summary>
public record ContributorDashboardDto
{
    public ContributorStatusCardsDto StatusCards { get; init; } = new();
    public PaginatedList<AssetListDto> Assets { get; init; } = null!;
}

/// <summary>
/// Status cards for PC Contributor dashboard
/// </summary>
public record ContributorStatusCardsDto
{
    /// <summary>
    /// Total assets created by this contributor
    /// </summary>
    public int TotalAssets { get; init; }

    /// <summary>
    /// Assets with status "AcceptedByInfrabase" (Checked assets)
    /// </summary>
    public int CheckedAssets { get; init; }

    /// <summary>
    /// Assets with status "Submitted" (Pending on PC admin checking)
    /// </summary>
    public int PendingOnPcAdmin { get; init; }

    /// <summary>
    /// Assets with status "AcceptedByPcAdmin" (Pending on infrabase admin checking)
    /// </summary>
    public int PendingOnInfrabaseAdmin { get; init; }

    /// <summary>
    /// Assets with status "Draft"
    /// </summary>
    public int Draft { get; init; }

    /// <summary>
    /// Assets with status "RejectedByPcAdmin" or "RejectedByInfrabase" (Return for correction)
    /// </summary>
    public int ReturnForCorrection { get; init; }
}

// ========== PC Admin Dashboard ==========

/// <summary>
/// Dashboard data for PC Admin (own assets)
/// User Story: "As PC admin, I want to view home page – landing page"
/// </summary>
public record PcAdminDashboardDto
{
    public PcAdminStatusCardsDto MyAssetsStatusCards { get; init; } = new();
    public PaginatedList<AssetListDto> MyAssets { get; init; } = null!;
}

/// <summary>
/// Status cards for PC Admin's own assets
/// </summary>
public record PcAdminStatusCardsDto
{
    public int TotalAssets { get; init; }
    public int Draft { get; init; }
    public int CheckedAssets { get; init; }
    public int PendingOnInfrabaseAdmin { get; init; }
    public int ReturnForCorrection { get; init; }
}

// ========== Team Assets Dashboard ==========

/// <summary>
/// Dashboard data for team assets (assets by contributors in same company)
/// User Story: "User will be able to view 'Team assets'"
/// </summary>
public record TeamAssetsDashboardDto
{
    public TeamAssetsStatusCardsDto StatusCards { get; init; } = new();
    public PaginatedList<AssetListDto> Assets { get; init; } = null!;
}

/// <summary>
/// Status cards for team assets
/// </summary>
public record TeamAssetsStatusCardsDto
{
    public int TotalAssets { get; init; }
    public int CheckedAssets { get; init; }
    public int ReturnForCorrection { get; init; }
    
    /// <summary>
    /// Assets with status "Submitted" (waiting for PC admin approval)
    /// </summary>
    public int PendingAssets { get; init; }
}

// ========== Infrabase Admin Dashboard ==========

/// <summary>
/// Dashboard data for Infrabase Admin
/// User Story: "As infrabase admin, I want to view home page – landing page"
/// </summary>
public record InfrabaseAdminDashboardDto
{
    public InfrabaseAdminStatusCardsDto StatusCards { get; init; } = new();
    public PaginatedList<AssetListDto> Assets { get; init; } = null!;
}

/// <summary>
/// Status cards for Infrabase Admin dashboard
/// </summary>
public record InfrabaseAdminStatusCardsDto
{
    public int TotalAssets { get; init; }
    public int Draft { get; init; }
    public int Submitted { get; init; }
    public int AcceptedByPcAdmin { get; init; }
    public int RejectedByPcAdmin { get; init; }
    public int AcceptedByInfrabase { get; init; }
    public int RejectedByInfrabase { get; init; }
}
