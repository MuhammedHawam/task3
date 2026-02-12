using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Integration;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;
using PartnersHub.Synergy.Domain.Common;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace PartnersHub.Synergy.Application.Opportunities.Commands;

public class UpdateOpportunityCommandHandler : IRequestHandler<UpdateOpportunityCommand, Result>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOpportunityTypeRepository _opportunityTypeRepository;
    private readonly IThematicAreaRepository _thematicAreaRepository;
    private readonly IExpectedOutcomesRepository _expectedOutcomesRepository;
    private readonly ICollaborationRequirementRepository _collaborationRequirementRepository;
    private readonly ISynergyCompanyRepository _synergyCompanyRepository;
    private readonly IUserService _userService;
    private readonly IMiddlewareIntegrationService _middlewareService;

    public UpdateOpportunityCommandHandler(
        IOpportunityRepository opportunityRepository,
        IUnitOfWork unitOfWork,
        IOpportunityTypeRepository opportunityTypeRepository,
        IThematicAreaRepository thematicAreaRepository,
        IExpectedOutcomesRepository expectedOutcomesRepository,
        ICollaborationRequirementRepository collaborationRequirementRepository,
        ISynergyCompanyRepository synergyCompanyRepository,
        IUserService userService,
        IMiddlewareIntegrationService middlewareService
        )
    {
        _opportunityRepository = opportunityRepository;
        _unitOfWork = unitOfWork;
        _opportunityTypeRepository = opportunityTypeRepository;
        _thematicAreaRepository = thematicAreaRepository;
        _expectedOutcomesRepository = expectedOutcomesRepository;
        _collaborationRequirementRepository = collaborationRequirementRepository;
        _synergyCompanyRepository = synergyCompanyRepository;
        _userService = userService;
        _middlewareService = middlewareService;
    }

    public async Task<Result> Handle(UpdateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(request.OpportunityId, false,
        o => o.OpportunityType,
        o => o.ThematicArea,
        o => o.Sector,
        o => o.ExpectedOutcomes,
        o => o.CollaborationRequirements,
        o => o.OpportunityType,
        o => o.CollaboratedCompanies,
        o => o.RepresentativeInformation,
        o => o.Attachments);
        if (opportunity == null)
            return Result.Failure("Opportunity doesn't exist");

        if (!await _opportunityTypeRepository.ExistsAsync(request.TypeId))
            return Result.Failure("Opportunity Type does not exist");

        if (!await _thematicAreaRepository.ExistsAsync(request.ThematicAreaId))
            return Result.Failure("Thematic Area does not exist");


        var opportunityType = await _opportunityTypeRepository.GetById(request.TypeId);
        var thematicArea = await _thematicAreaRepository.GetById(request.ThematicAreaId);


        var collaboratedIds = request.CollaboratedProfiles ?? new List<Guid>();
        if (collaboratedIds.Count > 0)
        {
            var companies = await _synergyCompanyRepository.GetByIdsAsync(collaboratedIds);
            if (companies == null || companies.Count != collaboratedIds.Count)
                return Result.Failure("Compaines not found");
        }


        var expectedIds = request.ExpectedOutcomes ?? new List<int>();
        var expectedOutcomes = await _expectedOutcomesRepository.GetByIdsAsync(expectedIds);
        if (expectedIds.Count > 0 && (expectedOutcomes == null || expectedOutcomes.Count != expectedIds.Count))
            return Result.Failure("expectedoutcomes not found");


        var collabReqIds = request.CollaborationRequirements ?? new List<int>();
        var collaborationRequirements = await _collaborationRequirementRepository.GetByIdsAsync(collabReqIds);
        if (collabReqIds.Count > 0 && (collaborationRequirements == null || collaborationRequirements.Count != collabReqIds.Count))
            return Result.Failure("collaboration Requirements not found");


        var result = opportunity.Update(
            _userService.CurrentUserId,
            request.Title,
            request.Description,
            request.SectorName,
            request.SectorId,
            request.TypeId,
            request.ThematicAreaId,
            request.CollaborationRationale,
            request.StartDate,
            request.EndDate,
            request.ContactName,
            request.ContactAddress,
            request.ContactMobile,
            request.IsAdmin);

        if (result.IsFailure)
            return Result.Failure(result.Error);


        result = opportunity.SetOpportunityType(opportunityType);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        result = opportunity.SetThematicArea(thematicArea);
        if (result.IsFailure)
            return Result.Failure(result.Error);


        result = opportunity.ReplaceCollaboratedCompanies(collaboratedIds, _userService.CurrentUserId);
        if (result.IsFailure)
            return Result.Failure(result.Error);


        result = opportunity.ReplaceCollaborationRequirements(
            collaborationRequirements,
            request.CollaborationRequirementOther,
            _userService.CurrentUserId);
        if (result.IsFailure)
            return Result.Failure(result.Error);


        result = opportunity.ReplaceExpectedOutcomes(
            expectedOutcomes,
            request.ExpectedOutcomeOther,
            _userService.CurrentUserId);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        if (request.AttachmentIdsToRemove?.Count > 0)
        {
            RemoveAttachments(opportunity, request.AttachmentIdsToRemove, _userService.CurrentUserId.ToString());
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            _opportunityRepository.Update(opportunity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (request.FilesToUpload?.Count > 0)
            {
                await UploadAttachmentsAsync(
                opportunity,
                _userService.CompanyId,
                _userService.CurrentUserId,
                request.FilesToUpload,
                request.AttachmentDescription,
                cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {

            await transaction.RollbackAsync(cancellationToken);


            return Result.Failure(
                ex is ValidationException ? ex.Message : "Failed to save Partnership with attachments.");
        }
    }



    private async Task UploadAttachmentsAsync(Domain.Aggregates.OpportunityAggregate.Opportunity opportunity,
                                           Guid companyId,
                                           Guid contactId,
                                           IReadOnlyCollection<FileUploadContent> files,
                                           string? description,
                                           CancellationToken cancellationToken)
    {


        var uploadRequest = new FileUploadRequest(
            opportunity.Id.ToString(),
            companyId,
            contactId,
            string.IsNullOrWhiteSpace(description) ? "opportunity attachment" : description,
            files);

        var uploadResult = await _middlewareService.UploadFilesAsync(uploadRequest, cancellationToken);
        if (!uploadResult.Success)
        {
            throw new ValidationException(uploadResult.Message ?? "Attachment upload failed.");
        }

        await MapUploadToAttachmentRequests(uploadResult, files, opportunity, cancellationToken);
    }

    private async Task MapUploadToAttachmentRequests(FileUploadResult uploadResult,
                                                     IReadOnlyCollection<FileUploadContent> originalFiles,
                                                     Domain.Aggregates.OpportunityAggregate.Opportunity opportunity,
                                                     CancellationToken cancellationToken)
    {

        var fileLookup = originalFiles
            .GroupBy(f => f.FileName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var uploaded in uploadResult.UploadedFiles.Where(f => f.Uploaded))
        {
            if (!fileLookup.TryGetValue(uploaded.FileName, out var original))
            {
                continue;
            }

            var size = uploaded.FileSize > 0 ? uploaded.FileSize : original.Length;
            opportunity.AddAttachment(original.FileName, uploaded.SharePointUrl, size, "");

        }

        // Save changes
        _opportunityRepository.Update(opportunity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }


    private static void RemoveAttachments(Domain.Aggregates.OpportunityAggregate.Opportunity opportunity, IEnumerable<Guid> attachmentIds, string userId)
    {
        foreach (var attachmentId in attachmentIds.Distinct())
        {
            if (attachmentId == Guid.Empty)
            {
                continue;
            }

            var removeResult = opportunity.RemoveAttachment(attachmentId);
            if (removeResult.IsFailure)
            {
                throw new ValidationException(removeResult.Error!);
            }
        }
    }


}

