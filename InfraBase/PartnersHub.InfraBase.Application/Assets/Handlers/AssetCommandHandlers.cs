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

public class SaveAssetAsDraftCommandHandler : IRequestHandler<SaveAssetAsDraftCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public SaveAssetAsDraftCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<bool> Handle(SaveAssetAsDraftCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var result = asset.SaveAsDraft(userName);
        if (result.IsFailure)
        {
            throw new ValidationException(result.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class SubmitAssetCommandHandler : IRequestHandler<SubmitAssetCommand, string>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public SubmitAssetCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<string> Handle(SubmitAssetCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history
        var isPcAdmin = command.IsPcAdmin || _tokenService.IsPcAdmin();

        var asset = await _repository.GetByIdWithDetailsAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var nextNumber = await _repository.GetNextAssetNumberAsync(cancellationToken);
        var assetCode = $"Infra-{nextNumber:D6}";

        var submitResult = asset.Submit(userName, assetCode, isPcAdmin);
        if (submitResult.IsFailure)
        {
            throw new ValidationException(submitResult.Error!);
        }

        if (isPcAdmin)
        {
            var approveResult = asset.AcceptByPcAdmin(userName);
            if (approveResult.IsFailure)
            {
                throw new ValidationException(approveResult.Error!);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return asset.AssetCode!;
    }
}

public class AcceptAssetByPcAdminCommandHandler : IRequestHandler<AcceptAssetByPcAdminCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public AcceptAssetByPcAdminCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<bool> Handle(AcceptAssetByPcAdminCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var result = asset.AcceptByPcAdmin(userName);
        if (result.IsFailure)
        {
            throw new ValidationException(result.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class RejectAssetByPcAdminCommandHandler : IRequestHandler<RejectAssetByPcAdminCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RejectAssetByPcAdminCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<bool> Handle(RejectAssetByPcAdminCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var result = asset.RejectByPcAdmin(userName, command.RejectionReason);
        if (result.IsFailure)
        {
            throw new ValidationException(result.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class CheckAssetByInfrabaseAdminCommandHandler : IRequestHandler<CheckAssetByInfrabaseAdminCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public CheckAssetByInfrabaseAdminCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<bool> Handle(CheckAssetByInfrabaseAdminCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var result = asset.CheckByInfrabaseAdmin(userName);
        if (result.IsFailure)
        {
            throw new ValidationException(result.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class ReturnAssetForCorrectionCommandHandler : IRequestHandler<ReturnAssetForCorrectionCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public ReturnAssetForCorrectionCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<bool> Handle(ReturnAssetForCorrectionCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        var result = asset.ReturnForCorrectionByInfrabaseAdmin(userName, command.CorrectionReason);
        if (result.IsFailure)
        {
            throw new ValidationException(result.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class DeleteAssetCommandHandler : IRequestHandler<DeleteAssetCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAssetCommandHandler(IAssetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteAssetCommand command, CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.Id);
        }

        _repository.Delete(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class AddAssetAttachmentCommandHandler : IRequestHandler<AddAssetAttachmentCommand, Guid>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public AddAssetAttachmentCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<Guid> Handle(AddAssetAttachmentCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdWithDetailsAsync(command.AssetId, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.AssetId);
        }

        var attachmentResult = asset.AddAttachment(
            command.FileName, 
            command.FileSizeInBytes, 
            command.ContentType, 
            command.SharePointUrl, 
            userName);

        if (attachmentResult.IsFailure)
        {
            throw new ValidationException(attachmentResult.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return attachmentResult.Value!.Id;
    }
}

public class RemoveAssetAttachmentCommandHandler : IRequestHandler<RemoveAssetAttachmentCommand, bool>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RemoveAssetAttachmentCommandHandler(
        IAssetRepository repository, 
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<bool> Handle(RemoveAssetAttachmentCommand command, CancellationToken cancellationToken)
    {
        var userName = _tokenService.GetUserName(); // Use username for readable history

        var asset = await _repository.GetByIdWithDetailsAsync(command.AssetId, cancellationToken);
        if (asset == null)
        {
            throw new NotFoundException("Asset", command.AssetId);
        }

        var result = asset.RemoveAttachment(command.AttachmentId, userName);
        if (result.IsFailure)
        {
            throw new ValidationException(result.Error!);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
