using MediatR;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Commands;

public record CreateAssetCommand : IRequest<Guid>
{
    /// <summary>
    /// Used by InfraBase Admins to create an asset on behalf of a selected portfolio company.
    /// For non-admin users, company is always resolved from the access token.
    /// </summary>
    public Guid? PortfolioCompanyId { get; init; }

    public string? AssetName { get; init; }
    public string LocationCity { get; init; } = string.Empty;
    public Guid? SectorId { get; init; }
    public Guid? SubSectorId { get; init; }
    public Guid? AssetTypeId { get; init; }
    public string? AssetTypeOther { get; init; }
    public decimal? QuantityOfAsset { get; init; }
    public decimal CapacityPerAsset { get; init; }
    public Guid? UnitOfMeasurementId { get; init; }
    public string? UnitOfMeasurementOther { get; init; }
    public string? Description { get; init; }
    public int? ConstructionStartingQuarter { get; init; }
    public int? ConstructionStartingYear { get; init; }
    public int? ConstructionCompletionQuarter { get; init; }
    public int? ConstructionCompletionYear { get; init; }
    public TenderingStages? TenderingStage { get; init; }
    public DevelopmentTypes? DevelopmentType { get; init; }
    public FinancialEntryMode? CapexEntryMode { get; init; }
    public FinancialEntryMode? OpexEntryMode { get; init; }
    public FundingModels? FundingModel { get; init; }
    public decimal? ExpectedDebt { get; init; }
    public decimal? ExpectedEquity { get; init; }
    public bool? IsRevenueGenerating { get; init; }
    public decimal? IRR { get; init; }
    public bool? IsPifGuaranteesRequired { get; init; }
    public List<CapexDetailDto> CapexDetails { get; init; } = new();
    public List<OpexDetailDto> OpexDetails { get; init; } = new();

    public Guid CompanyId { get; init; }
    public string CompanyName { get; init; }
}

public record UpdateAssetCommand : IRequest<bool>
{
    public Guid Id { get; init; }
    public string? AssetName { get; init; }
    public string? LocationCity { get; init; }
    public Guid? SectorId { get; init; }
    public Guid? SubSectorId { get; init; }
    public Guid? AssetTypeId { get; init; }
    public string? AssetTypeOther { get; init; }
    public decimal? QuantityOfAsset { get; init; }
    public decimal? CapacityPerAsset { get; init; }
    public Guid? UnitOfMeasurementId { get; init; }
    public string? UnitOfMeasurementOther { get; init; }
    public string? Description { get; init; }
    public int? ConstructionStartingQuarter { get; init; }
    public int? ConstructionStartingYear { get; init; }
    public int? ConstructionCompletionQuarter { get; init; }
    public int? ConstructionCompletionYear { get; init; }
    public TenderingStages? TenderingStage { get; init; }
    public DevelopmentTypes? DevelopmentType { get; init; }
    public FinancialEntryMode? CapexEntryMode { get; init; }
    public FinancialEntryMode? OpexEntryMode { get; init; }
    public FundingModels? FundingModel { get; init; }
    public decimal? ExpectedDebt { get; init; }
    public decimal? ExpectedEquity { get; init; }
    public bool? IsRevenueGenerating { get; init; }
    public decimal? IRR { get; init; }
    public bool? IsPifGuaranteesRequired { get; init; }
    public List<CapexDetailDto>? CapexDetails { get; init; }
    public List<OpexDetailDto>? OpexDetails { get; init; }
}

public record SaveAssetAsDraftCommand(Guid Id) : IRequest<bool>;

public record SubmitAssetCommand(Guid Id, bool IsPcAdmin = false) : IRequest<string>;

public record AcceptAssetByPcAdminCommand(Guid Id) : IRequest<bool>;

public record RejectAssetByPcAdminCommand(Guid Id, string RejectionReason) : IRequest<bool>;

public record CheckAssetByInfrabaseAdminCommand(Guid Id) : IRequest<bool>;

public record ReturnAssetForCorrectionCommand(Guid Id, string CorrectionReason) : IRequest<bool>;

public record DeleteAssetCommand(Guid Id) : IRequest<bool>;

public record AddAssetAttachmentCommand : IRequest<Guid>
{
    public Guid AssetId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSizeInBytes { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public string SharePointUrl { get; init; } = string.Empty;
}

public record RemoveAssetAttachmentCommand(Guid AssetId, Guid AttachmentId) : IRequest<bool>;

public record CapexDetailDto(int Year, decimal Amount);

public record OpexDetailDto(int Year, decimal Amount);
