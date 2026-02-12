using MediatR;
using Microsoft.Extensions.Options;
using PartnersHub.Synergy.Application.Common.Options;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Common;
using PartnersHub.Synergy.Application.Interfaces.Integration;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Application.Models;
using PartnersHub.Synergy.Application.SuccessStories.Commands;
using PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate;
using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;
using PartnersHub.Synergy.Domain.Aggregates.SynergyCompanyAggregate;
using PartnersHub.Synergy.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.Threading;

public class CreateSuccessStoryCommandHandler : IRequestHandler<CreateSuccessStoryCommand, Result<Guid>>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ISynergyCompanyRepository _synergyCompanyRepository;
    private readonly ISuccessStoryRepository _successStoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly RequestCodeSettings _requestCodeSettings;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly IMiddlewareIntegrationService _middlewareService;

    public CreateSuccessStoryCommandHandler(IOpportunityRepository opportunityRepository, 
        ISynergyCompanyRepository synergyCompanyRepository,
        ISuccessStoryRepository successStoryRepository,
        IUserService userService,
        IUnitOfWork unitOfWork,
        IOptions<RequestCodeSettings> requestCodeSettings,
        IMiddlewareIntegrationService middlewareService
         )
    {

        _synergyCompanyRepository = synergyCompanyRepository;
        _opportunityRepository = opportunityRepository;
        _successStoryRepository = successStoryRepository;
        _unitOfWork = unitOfWork;
        _userService = userService;
        _requestCodeSettings = requestCodeSettings.Value;
        _middlewareService = middlewareService;
    }

    public async Task<Result<Guid>> Handle(CreateSuccessStoryCommand request, CancellationToken cancellationToken)
    {

      var HasNoCollaboratedProfile = request.CollaboratedProfiles == null || request.CollaboratedProfiles.Count == 0;
        List<SynergyCompany> synergyCompanies = await _synergyCompanyRepository.GetByIdsAsync(request.CollaboratedProfiles);
        if (!HasNoCollaboratedProfile)
        {
            if (synergyCompanies == null || synergyCompanies.Count <= default(int) || request.CollaboratedProfiles.Count > synergyCompanies.Count)
                return Result<Guid>.Failure("Synergy company doesn't exist");
        }


    
        
            await _lock.WaitAsync(cancellationToken);
          

       
        string nextCode;

        var isTitleExist = await _successStoryRepository.CheckTitleUniqueness(request.Title, null);
        if (isTitleExist)
            return Result<Guid>.Failure("A success story with this title already exists.");
        try
        {
            int nextRequestId = await _successStoryRepository.GetNextRequestIdAsync();

            nextCode = _requestCodeSettings.GenerateCode(nextRequestId);
        }
        catch (OperationCanceledException)
        {
            return Result<Guid>.Failure("Operation was cancelled");
        }
        finally
        {
            _lock.Release();
        }
        var successStoryResult = SuccessStory.Create(( request.IsAdmin == true ? request.UserCompanyId.Value : _userService.CompanyId),
            request.Title, request.Description,
            request.SuccessStoryTypeId,
            nextCode,
            request.StartDate,
            request.EndDate,
            request.SuccessStoryCollaborationStatusId,
            Guid.NewGuid(),
            _userService.CurrentUserId,
            request.SectorId,
            request.SectorName,
            request.UserEmail,request.IsAdmin);
        if (successStoryResult.IsFailure)
            return Result<Guid>.Failure(successStoryResult.Error);

        var result = successStoryResult.Value.AddCollaboratedCompanies(request.CollaboratedProfiles);
        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        result = successStoryResult.Value.AddAssociatedOpportunities(request.AssociatedOpportunities != null ? (new List<Guid> { request.AssociatedOpportunities.Value  }) : null);
        if (result.IsFailure)
            return Result<Guid>.Failure(result.Error);

        successStoryResult.Value.Submit(_userService.CurrentUserId, request.Title,request.CompanyName);

        if(request.IsAdmin.HasValue && request.IsAdmin.Value) successStoryResult.Value.Publish(_userService.CurrentUserId, successStoryResult.Value.Title.Value, successStoryResult.Value.CompanyName, successStoryResult.Value.UserEmail, request.IsAdmin.Value);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _successStoryRepository.AddAsync(successStoryResult.Value);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (request.FilesToUpload?.Count > 0)
            {
                await UploadAttachmentsAsync(
                successStoryResult.Value.Id,
                request.UserCompanyId ?? _userService.CompanyId,
                _userService.CurrentUserId,
                request.FilesToUpload,
                request.AttachmentDescription,
                cancellationToken);


            }

            await transaction.CommitAsync(cancellationToken);

            return Result<Guid>.Success(successStoryResult.Value.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);


            return Result<Guid>.Failure(
                ex is ValidationException ? ex.Message : "Failed to save success story with attachments.");
        }


        

    }

    private async Task UploadAttachmentsAsync(
    Guid successStoryId,
    Guid companyId,
    Guid contactId,
    IReadOnlyCollection<FileUploadContent> files,
    string? description,
    CancellationToken cancellationToken)
    {


        var uploadRequest = new FileUploadRequest(
            successStoryId.ToString(),
            companyId,
            contactId,
            string.IsNullOrWhiteSpace(description) ? "SuccessStory attachment" : description,
            files);

        var uploadResult = await _middlewareService.UploadFilesAsync(uploadRequest, cancellationToken);
        if (!uploadResult.Success)
        {
            throw new ValidationException(uploadResult.Message ?? "Attachment upload failed.");
        }

        await MapUploadToAttachmentRequests(uploadResult, files , successStoryId , cancellationToken);
    }

    private async Task MapUploadToAttachmentRequests(
        FileUploadResult uploadResult,
        IReadOnlyCollection<FileUploadContent> originalFiles , Guid successStoryId, CancellationToken cancellationToken)
    {
        var successStory = await _successStoryRepository.GetByIdAsync(
           successStoryId,
           asNoTracking: false,
           s => s.Attachments);

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
            successStory.AddAttachment(original.FileName,uploaded.SharePointUrl,size,"");

        }

        // Save changes
        _successStoryRepository.Update(successStory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }


}