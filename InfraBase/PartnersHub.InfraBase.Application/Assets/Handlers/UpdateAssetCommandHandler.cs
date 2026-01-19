using MediatR;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Common.Exceptions;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;
using PartnersHub.InfraBase.Domain.Enums;

namespace PartnersHub.InfraBase.Application.Assets.Handlers;

public class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public UpdateAssetCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<bool> Handle(UpdateAssetCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdWithFinancialsAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var updateResult = asset.UpdateAssetInformation(
            command.AssetName, 
            command.LocationCity, 
            command.SectorId,
            command.SectorOther,
            command.SubSectorId,
            command.SubSectorOther,
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
            userName);

        if (updateResult.IsFailure)
        {
            throw new ValidationException(updateResult.Error!);
        }

        if (command.CapexDetails != null)
        {
            ValidateYearRows(command.CapexDetails.Select(x => x.Year), "CAPEX");
            if (command.CapexEntryMode == FinancialEntryMode.SingleYear && command.CapexDetails.Count > 1)
            {
                throw new ValidationException("CAPEX Single-Year mode supports only one year row.");
            }
        }

        if (command.OpexDetails != null)
        {
            ValidateYearRows(command.OpexDetails.Select(x => x.Year), "OPEX");
            if (command.OpexEntryMode == FinancialEntryMode.SingleYear && command.OpexDetails.Count > 1)
            {
                throw new ValidationException("OPEX Single-Year mode supports only one year row.");
            }
        }

        UpdateCapexDetails(asset, command, userName);
        UpdateOpexDetails(asset, command, userName);

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

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
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

    private static void UpdateCapexDetails(Asset asset, UpdateAssetCommand command, string userId)
    {
        if (command.CapexDetails == null) return;

        var existingYears = asset.CapexDetails.Select(c => c.Year).ToList();
        var newYears = command.CapexDetails.Select(c => c.Year).ToList();

        // Remove CAPEX that are no longer in the command
        foreach (var year in existingYears.Where(y => !newYears.Contains(y)))
        {
            asset.RemoveCapexDetail(year, userId);
        }

        // Add or update CAPEX
        foreach (var capexDto in command.CapexDetails)
        {
            var existing = asset.CapexDetails.FirstOrDefault(c => c.Year == capexDto.Year);
            if (existing == null)
            {
                var addResult = asset.AddCapexDetail(capexDto.Year, capexDto.Amount, userId);
                if (addResult.IsFailure)
                {
                    throw new ValidationException(addResult.Error!);
                }
            }
            else if (existing.Amount != capexDto.Amount)
            {
                var updateResult = asset.UpdateCapexDetail(capexDto.Year, capexDto.Amount, userId);
                if (updateResult.IsFailure)
                {
                    throw new ValidationException(updateResult.Error!);
                }
            }
        }
    }

    private static void UpdateOpexDetails(Asset asset, UpdateAssetCommand command, string userId)
    {
        if (command.OpexDetails == null) return;

        var existingYears = asset.OpexDetails.Select(o => o.Year).ToList();
        var newYears = command.OpexDetails.Select(o => o.Year).ToList();

        // Remove OPEX that are no longer in the command
        foreach (var year in existingYears.Where(y => !newYears.Contains(y)))
        {
            asset.RemoveOpexDetail(year, userId);
        }

        // Add or update OPEX
        foreach (var opexDto in command.OpexDetails)
        {
            var existing = asset.OpexDetails.FirstOrDefault(o => o.Year == opexDto.Year);
            if (existing == null)
            {
                var addResult = asset.AddOpexDetail(opexDto.Year, opexDto.Amount, userId);
                if (addResult.IsFailure)
                {
                    throw new ValidationException(addResult.Error!);
                }
            }
            else if (existing.Amount != opexDto.Amount)
            {
                var updateResult = asset.UpdateOpexDetail(opexDto.Year, opexDto.Amount, userId);
                if (updateResult.IsFailure)
                {
                    throw new ValidationException(updateResult.Error!);
                }
            }
        }
    }
}
