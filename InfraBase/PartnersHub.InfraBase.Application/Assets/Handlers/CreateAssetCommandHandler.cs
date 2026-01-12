using MediatR;
using Microsoft.Extensions.Logging;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Common.Exceptions;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;
using PartnersHub.InfraBase.Domain.Enums;

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
      //  var companyId = ResolveCompanyId(command);

        ValidateYearRows(command.CapexDetails.Select(x => x.Year), "CAPEX");
        ValidateYearRows(command.OpexDetails.Select(x => x.Year), "OPEX");

        if (command.CapexEntryMode == FinancialEntryMode.SingleYear && command.CapexDetails.Count > 1)
        {
            throw new ValidationException("CAPEX Single-Year mode supports only one year row.");
        }

        if (command.OpexEntryMode == FinancialEntryMode.SingleYear && command.OpexDetails.Count > 1)
        {
            throw new ValidationException("OPEX Single-Year mode supports only one year row.");
        }

        string? companyName = null;
        try
        {
            var company = await _middlewareService.GetCompanyByIdAsync(command.CompanyId);
            if (company != null)
            {
                companyName = company.Name;
                _logger.LogInformation("Fetched company name: {CompanyName}", companyName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching company name for {CompanyId}", command.CompanyId);
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
            command.CompanyId,
            command.CompanyName);

        if (assetResult.IsFailure)
        {
            throw new ValidationException(assetResult.Error ?? "Asset creation failed.");
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

        if (command.CapexEntryMode.HasValue)
        {
            var modeResult = asset.SetCapexEntryMode(command.CapexEntryMode.Value, userName);
            if (modeResult.IsFailure)
            {
                throw new ValidationException(modeResult.Error!);
            }
        }

        if (command.OpexEntryMode.HasValue)
        {
            var modeResult = asset.SetOpexEntryMode(command.OpexEntryMode.Value, userName);
            if (modeResult.IsFailure)
            {
                throw new ValidationException(modeResult.Error!);
            }
        }

        await _repository.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return asset.Id;
    }

    private static void ValidateYearRows(IEnumerable<int> years, string label)
    {
        var yearList = years.ToList();
        var duplicates = yearList
            .GroupBy(y => y)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new ValidationException($"{label} has duplicate years: {string.Join(", ", duplicates)}");
        }
    }

    private Guid ResolveCompanyId(CreateAssetCommand command)
    {
        var tokenCompanyId = _tokenService.GetCompanyId();

        // InfraBase Admin flow: allow creating on behalf of a selected portfolio company.
        if (_tokenService.IsInfrabaseAdmin())
        {
            if (command.PortfolioCompanyId.HasValue && command.PortfolioCompanyId.Value != Guid.Empty)
            {
                return command.PortfolioCompanyId.Value;
            }

            // If admin didn't pass an override, fall back to token company id (if any).
            if (tokenCompanyId.HasValue && tokenCompanyId.Value != Guid.Empty)
            {
                return tokenCompanyId.Value;
            }

            throw new ValidationException("Portfolio company is required to create an asset on behalf of a company.");
        }

        // Non-admin flow: company must come from token.
        if (!tokenCompanyId.HasValue || tokenCompanyId.Value == Guid.Empty)
        {
            throw new ValidationException("Company ID is required to create an asset.");
        }

        return tokenCompanyId.Value;
    }
}
