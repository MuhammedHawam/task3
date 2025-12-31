using MediatR;
using PartnersHub.InfraBase.Application.Assets.Commands;
using PartnersHub.InfraBase.Application.Common.Exceptions;
using PartnersHub.InfraBase.Application.Common.Interfaces;
using PartnersHub.InfraBase.Application.Common.Interfaces.Repository;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

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

        var asset = await _repository.GetByIdWithDetailsAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var updateResult = asset.UpdateAssetInformation(
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
            userName);

        if (updateResult.IsFailure)
        {
            throw new ValidationException(updateResult.Error!);
        }

        UpdateCapexDetails(asset, command, userName);
        UpdateOpexDetails(asset, command, userName);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
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
