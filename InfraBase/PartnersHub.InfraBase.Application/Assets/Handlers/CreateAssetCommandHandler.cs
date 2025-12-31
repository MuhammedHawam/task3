using MediatR;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Common.Exceptions;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, Guid>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IMiddlewareIntegrationService _middlewareService;
    private readonly ILogger<CreateAssetCommandHandler> _logger;

    public CreateAssetCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IMiddlewareIntegrationService middlewareService,
        ILogger<CreateAssetCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _middlewareService = middlewareService;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateAssetCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history
        var userEmail = _tokenService.GetUserEmail(); // Use email for CreatedBy field
        var companyId = _tokenService.GetCompanyId();

        if (!companyId.HasValue || companyId.Value == Guid.Empty)
        {
            throw new ValidationException("Company ID is required to create an asset.");
        }

        string? companyName = null;
        try
        {
            var company = await _middlewareService.GetCompanyByIdAsync(companyId.Value);
            if (company != null)
            {
                companyName = company.Name;
                _logger.LogInformation("Fetched company name: {CompanyName}", companyName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching company name for {CompanyId}", companyId.Value);
        }

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
            userName, // Use username for history
            companyId.Value,
            companyName);

        if (assetResult.IsFailure)
        {
            throw new ValidationException(assetResult.Error!);
        }

        var asset = assetResult.Value!;

        foreach (var capex in command.CapexDetails)
        {
            var capexResult = asset.AddCapexDetail(capex.Year, capex.Amount, userName);
            if (capexResult.IsFailure)
            {
                throw new ValidationException(capexResult.Error!);
            }
        }

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

        return asset.Id;
    }
}
