using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.DTOs;

public record AssetDto
{
    public Guid Id { get; init; }
    public string? AssetCode { get; init; }
    public string AssetName { get; init; } = string.Empty;
    public string LocationCity { get; init; } = string.Empty;
    public Guid? SectorId { get; init; }
    public string? SectorName { get; init; }
    public Guid? SubSectorId { get; init; }
    public string? SubSectorName { get; init; }
    public Guid? AssetTypeId { get; init; }
    public string? AssetTypeName { get; init; }
    public string? AssetTypeOther { get; init; }
    public decimal? QuantityOfAsset { get; init; }
    public decimal CapacityPerAsset { get; init; }
    public decimal? TotalCapacity { get; init; }
    public Guid? UnitOfMeasurementId { get; init; }
    public string? UnitOfMeasurementName { get; init; }
    public string? UnitOfMeasurementOther { get; init; }
    public string? Description { get; init; }
    public int? ConstructionStartingQuarter { get; init; }
    public int? ConstructionStartingYear { get; init; }
    public int? ConstructionCompletionQuarter { get; init; }
    public int? ConstructionCompletionYear { get; init; }
    public TenderingStages? TenderingStage { get; init; }
    public string TenderingStageDisplayName => TenderingStage?.GetDisplayName() ?? "N/A";
    public DevelopmentTypes? DevelopmentType { get; init; }
    public string DevelopmentTypeDisplayName => DevelopmentType?.GetDisplayName() ?? "N/A";
    public decimal TotalCapex { get; init; }
    public decimal TotalOpex { get; init; }
    public FundingModels? FundingModel { get; init; }
    public string FundingModelDisplayName => FundingModel?.GetDisplayName() ?? "N/A";
    public decimal? ExpectedDebt { get; init; }
    public decimal? ExpectedEquity { get; init; }
    public bool? IsRevenueGenerating { get; init; }
    public decimal? IRR { get; init; }
    public bool? IsPifGuaranteesRequired { get; init; }
    public AssetStatuses Status { get; init; }
    public string? SubmittedBy { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public string? RejectionReason { get; init; }
    public string? RejectedBy { get; init; }
    public DateTime? RejectedAt { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public Guid CompanyId { get; init; }  // Changed back to non-nullable
    public string? CompanyName { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? UpdatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<AssetCapexDto> CapexDetails { get; init; } = new();
    public List<AssetOpexDto> OpexDetails { get; init; } = new();
    public List<AssetHistoryDto> History { get; init; } = new();
    public List<AssetAttachmentDto> Attachments { get; init; } = new();
}

public record AssetCapexDto
{
    public Guid Id { get; init; }
    public int Year { get; init; }
    public decimal Amount { get; init; }
}

public record AssetOpexDto
{
    public Guid Id { get; init; }
    public int Year { get; init; }
    public decimal Amount { get; init; }
}

public record AssetHistoryDto
{
    public Guid Id { get; init; }
    public AssetStatuses Status { get; init; }
    public string Action { get; init; } = string.Empty;
    public string PerformedBy { get; init; } = string.Empty;
    public string? PerformedByName { get; init; }
    public DateTime PerformedAt { get; init; }
    public string? Comments { get; init; }
}

public record AssetAttachmentDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
    public string UploadedBy { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
}

/// <summary>
/// Asset list item for grid display
/// Includes columns as per user story requirements
/// </summary>
public record AssetListDto
{
    public Guid Id { get; init; }
    
    /// <summary>Asset code (e.g., Infra-000001)</summary>
    public string? AssetCode { get; init; }
    
    /// <summary>Asset name</summary>
    public string AssetName { get; init; } = string.Empty;
    
    /// <summary>Sector name - Required for grid display</summary>
    public string? SectorName { get; init; }
    
    /// <summary>Sub-sector name - Required for grid display</summary>
    public string? SubSectorName { get; init; }
    
    /// <summary>Asset type name - Required for grid display</summary>
    public string? AssetTypeName { get; init; }
    
    /// <summary>Total CAPEX - Required for grid display</summary>
    public decimal TotalCapex { get; init; }
    
    /// <summary>Total OPEX - Required for grid display</summary>
    public decimal TotalOpex { get; init; }
    
    /// <summary>Asset status enum value</summary>
    public AssetStatuses Status { get; init; }
    
    /// <summary>
    /// User-friendly status display name matching user story terminology
    /// </summary>
    public string StatusDisplayName => Status switch
    {
        AssetStatuses.Draft => "Draft",
        AssetStatuses.Submitted => "Pending on PC Admin Checking",
        AssetStatuses.AcceptedByPcAdmin => "Pending on Infrabase Admin Checking",
        AssetStatuses.RejectedByPcAdmin => "Return for correction",
        AssetStatuses.AcceptedByInfrabase => "Checked",
        AssetStatuses.RejectedByInfrabase => "Request for updates",
        _ => Status.ToString()
    };
    
    /// <summary>Submission date - Required for grid display</summary>
    public DateTime? SubmittedAt { get; init; }

    public string? SubmittedBy { get; init; }
    
    /// <summary>Company name</summary>
    public string? CompanyName { get; init; }
    
    /// <summary>Creation date for sorting</summary>
    public DateTime CreatedAt { get; init; }
}

public record AssetSummaryDto
{
    public int TotalAssets { get; init; }
    public int DraftAssets { get; init; }
    public int SubmittedAssets { get; init; }
    public int PcAdminApprovedAssets { get; init; }
    public int RejectedAssets { get; init; }
    public int CheckedAssets { get; init; }
    public int ReturnedForCorrectionAssets { get; init; }
}

/// <summary>
/// Helper class for getting user-friendly status display names
/// </summary>
public static class AssetStatusExtensions
{
    /// <summary>
    /// Gets the user-story-compliant display name for an asset status
    /// </summary>
    public static string GetDisplayName(this AssetStatuses status)
    {
        return status switch
        {
            AssetStatuses.Draft => "Draft",
            AssetStatuses.Submitted => "Pending on PC Admin Checking",
            AssetStatuses.AcceptedByPcAdmin => "Pending on Infrabase Admin Checking",
            AssetStatuses.RejectedByPcAdmin => "Return for correction",
            AssetStatuses.AcceptedByInfrabase => "Checked",
            AssetStatuses.RejectedByInfrabase => "Request for updates",
            _ => status.ToString()
        };
    }
    
    /// <summary>
    /// Gets the short display name for dashboard cards
    /// </summary>
    public static string GetShortDisplayName(this AssetStatuses status)
    {
        return status switch
        {
            AssetStatuses.Draft => "Draft",
            AssetStatuses.Submitted => "Pending on PC Admin",
            AssetStatuses.AcceptedByPcAdmin => "Pending on Infrabase Admin",
            AssetStatuses.RejectedByPcAdmin => "Return for correction",
            AssetStatuses.AcceptedByInfrabase => "Checked",
            AssetStatuses.RejectedByInfrabase => "Request for updates",
            _ => status.ToString()
        };
    }
}
