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


namespace PartnersHub.Synergy.Application.SuccessStories.Commands;

public class UpdateSuccessStoryCommandHandler(ISuccessStoryRepository _successStoryRepository,
                                              IUnitOfWork _unitOfWork,
                                              ISynergyCompanyRepository _synergyCompanyRepository,
                                              IMiddlewareIntegrationService _middlewareService,
                                              IUserService _userService
    )
                                              : IRequestHandler<UpdateSuccessStoryCommand, Result>
{

    public async Task<Result> Handle(UpdateSuccessStoryCommand request, CancellationToken cancellationToken)
    {
        var successStory = await _successStoryRepository.GetByIdAsync(request.Id);
        if (successStory == null)
            return Result.Failure("Success story not found");

        var isTitleExist = await _successStoryRepository.CheckTitleUniqueness(request.Title, request.Id);
        if (isTitleExist)
            return Result.Failure("A success story with this title already exists.");

        var updateResult = successStory.Update(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.SuccessStoryTypeId,
            request.SuccessStoryCollaborationStatusId,
            request.IsAdmin);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        if (request.AttachmentIdsToRemove?.Count > 0)
        {
            RemoveAttachments(successStory, request.AttachmentIdsToRemove, _userService.CurrentUserId.ToString());
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ⬇ Optional attachment handling (NO effect if null or empty)
            if (request.FilesToUpload?.Count > 0)
            {
                await UploadAttachmentsAsync(
                    successStory.Id,
                    _userService.CompanyId,
                    _userService.CurrentUserId,
                    request.FilesToUpload,
                    request.AttachmentDescription,
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (ValidationException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure(ex.Message);
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure("Operation was cancelled.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure("Failed to update success story with attachments.");
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

        await MapUploadToAttachmentRequests(uploadResult, files, successStoryId, cancellationToken);
    }

    private async Task MapUploadToAttachmentRequests(
        FileUploadResult uploadResult,
        IReadOnlyCollection<FileUploadContent> originalFiles, Guid successStoryId, CancellationToken cancellationToken)
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
            successStory.AddAttachment(original.FileName, uploaded.SharePointUrl, size, "");

        }

        // Save changes
        _successStoryRepository.Update(successStory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }



    private static void RemoveAttachments(SuccessStory successStory, IEnumerable<Guid> attachmentIds, string userId)
    {
        foreach (var attachmentId in attachmentIds.Distinct())
        {
            if (attachmentId == Guid.Empty)
            {
                continue;
            }

            var removeResult = successStory.RemoveAttachment(attachmentId);
            if (removeResult.IsFailure)
            {
                throw new ValidationException(removeResult.Error!);
            }
        }
    }


}
