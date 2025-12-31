using MediatR;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Common.Exceptions;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

/// <summary>
/// Handler for creating an asset on behalf of a Portfolio Company by InfraBases Admin
/// </summary>
public class CreateAssetOnBehalfOfPcCompanyCommandHandler : IRequestHandler<CreateAssetOnBehalfOfPcCompanyCommand, Guid>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly ILogger<CreateAssetOnBehalfOfPcCompanyCommandHandler> _logger;

    public CreateAssetOnBehalfOfPcCompanyCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IMiddlewareIntegrationService middlewareService,
        ILogger<CreateAssetOnBehalfOfPcCompanyCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _middlewareService = middlewareService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateAssetOnBehalfOfPcCompanyCommand command, CancellationToken cancellationToken)
    {
        // Validate that the user is an InfraBases Admin
        if (!_tokenService.IsInfrabaseAdmin())
        {
            throw new UnauthorizedAccessException("Only InfraBases Admins can create assets on behalf of Portfolio Companies.");
        }

        var userName = _tokenService.GetUserName();
        var userEmail = _tokenService.GetUserEmail();

        // Validate Portfolio Company ID
        if (command.PortfolioCompanyId == Guid.Empty)
        {
            throw new ValidationException("Portfolio Company ID is required.");
        }

        // Fetch Portfolio Company details from middleware
        var portfolioCompany = await _middlewareService.GetCompanyByIdAsync(command.PortfolioCompanyId);
        if (portfolioCompany == null)
        {
            throw new NotFoundException("Portfolio Company", command.PortfolioCompanyId);
        }

        _logger.LogInformation(
            "InfraBases Admin {AdminName} ({AdminEmail}) creating asset on behalf of Portfolio Company {CompanyId} ({CompanyName})",
            userName, userEmail, command.PortfolioCompanyId, portfolioCompany.Name);

        // Create asset with Portfolio Company information
        var assetResult = Asset.Create(
            command.AssetName, 
            command.LocationCity, 
            command.SectorId, 
            command.SubSectorId, 
            command.AssetTypeId, 
            command.AssetTypeOther, 
            command.QuantityOfAsset, 
            command.CapacityPerAsset, 
            command.UnitOfMeasurementId, 
            command.UnitOfMeasurementOther,
            command.Description, 
            command.ConstructionStartingQuarter, 
            command.ConstructionStartingYear, 
            command.ConstructionCompletionQuarter, 
            command.ConstructionCompletionYear, 
            command.TenderingStage, 
            command.DevelopmentType, 
            command.FundingModel, 
            command.ExpectedDebt, 
            command.ExpectedEquity, 
            command.IsRevenueGenerating, 
            command.IRR, 
            command.IsPifGuaranteesRequired, 
            userName, // Use admin username for history
            command.PortfolioCompanyId, // Use Portfolio Company ID
            portfolioCompany.Name); // Use Portfolio Company name

        if (assetResult.IsFailure)
        {
            throw new ValidationException(assetResult.Error!);
        }

        var asset = assetResult.Value!;

        // Add CAPEX details
        foreach (var capex in command.CapexDetails)
        {
            var capexResult = asset.AddCapexDetail(capex.Year, capex.Amount, userName);
            if (capexResult.IsFailure)
            {
                throw new ValidationException(capexResult.Error!);
            }
        }

        // Add OPEX details
        foreach (var opex in command.OpexDetails)
        {
            var opexResult = asset.AddOpexDetail(opex.Year, opex.Amount, userName);
            if (opexResult.IsFailure)
            {
                throw new ValidationException(opexResult.Error!);
            }
        }

        await _repository.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Asset {AssetId} created successfully on behalf of Portfolio Company {CompanyId} ({CompanyName}) by InfraBases Admin {AdminName}",
            asset.Id, command.PortfolioCompanyId, portfolioCompany.Name, userName);

        return asset.Id;
    }
}
