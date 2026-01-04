using MediatR;
using PartnersHub.InfraBase.Application.Assets.DTOs;
using PartnersHub.InfraBase.Application.Common.DTOs;
using PartnersHub.InfraBase.Application.Common.Models;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Queries;

public record GetAssetByIdQuery(Guid Id) : IRequest<AssetDto?>;

public record GetAssetListQuery : IRequest<PaginatedList<AssetListDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public AssetStatuses? Status { get; init; }
    public Guid? CompanyId { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = false;
}

public record GetAssetSummaryQuery(Guid? CompanyId = null) : IRequest<AssetSummaryDto>;

public record GetAssetsByStatusQuery(AssetStatuses Status, Guid? CompanyId = null) 
    : IRequest<List<AssetListDto>>;

public record GetAssetHistoryQuery(Guid AssetId) : IRequest<List<AssetHistoryDto>>;

public record GetAssetAttachmentsQuery(Guid AssetId) : IRequest<List<AssetAttachmentDto>>;

public record GetNextAssetCodeQuery() : IRequest<string>;

public record GetPortfolioCompaniesQuery(string? SearchTerm = null) : IRequest<List<PortfolioCompanyDto>>;
