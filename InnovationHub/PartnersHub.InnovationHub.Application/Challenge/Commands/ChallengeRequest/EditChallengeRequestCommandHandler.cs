using MediatR;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Integration;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Application.Models;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using PartnersHub.InnovationHub.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;


namespace PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest
{
    /// <summary>
    /// Handles the Edit of a new challenge request.
    /// </summary>
    public class EditChallengeRequestCommandHandler(
          IChallengeRequestRepository _challengeRepository,
          IUnitOfWork _unitOfWork,
          IAssociatedProviderRepository _associatedProviderRepository,
          IAssociatedSectorRepository _associatedSectorRepository,
          IAttachmentRepository _attachmentRepository,
          ICurrentUserService _userService,
          IMiddlewareIntegrationService _middlewareService
            ) : IRequestHandler<EditChallengeRequestCommand, Result<bool>>
    {


        public async Task<Result<bool>> Handle(EditChallengeRequestCommand request, CancellationToken cancellationToken)
        {
            var challenge = await _challengeRepository.GetById(request.ChallengeRequestId, cancellationToken);

            if (challenge == null)
                return Result<bool>.Failure("This Challenge not found");

            if (challenge != null && (challenge.ChallengeStatus != ChallengeStatus.Draft && challenge.ChallengeStatus != ChallengeStatus.RevisionsRequest))
                return Result<bool>.Failure("Challenge status must be Draft or Revision Requested");

            // Check for duplicate name
            if (await _challengeRepository.ExistsByNameAsync(request.Name, request.ChallengeRequestId, cancellationToken))
                return Result<bool>.Failure("A challenge with this name already exists");


            var existingSector = request.AssociatedSector.id != challenge.AssociatedSectorId ? await _associatedSectorRepository.GetById(request.AssociatedSector.id, cancellationToken): null;
            var associatedSectorId = existingSector?.Id ?? request.AssociatedSector.id;
            var associatedSectorName = existingSector?.Name ?? request.AssociatedSector.name;



            challenge.Update(request.Name,
                             request.Description,
                             (int)request.PriorityLevel,
                             associatedSectorId,
                             associatedSectorName,
                             existingSector,
                             (request.IsDraft ? ChallengeStatus.Draft : ChallengeStatus.Pending));

            var attachments = request.Attachments.Select(attachment =>
                                      {
                                          var attachmentResult = Attachment.Create(attachment.fileName,
                                                                                   attachment.fileSizeInBytes,
                                                                                   attachment.attachmentMetadata.Format,
                                                                                   attachment.SharePointUrl);

                                          return attachmentResult.IsSuccess ? ChallengeRequestAttachment.Create(challenge.Id,
                                                                                                                attachmentResult.Value,
                                                                                                                attachment.sharePointFileId,
                                                                                                                attachment.SharePointUrl,
                                                                                                                attachment.sharePointLibrary,
                                                                                                                request.UserId).Value
                                                                                                               : null;
                                      })
                                                                                   .Where(attachment => attachment != null)
                                                                                   .ToList();



            if (request.Attachments.Count > 0 &&  !attachments.Any())
                return Result<bool>.Failure("missed data in Attachment");

            if (request.AttachmentIdsToRemove?.Count > 0)
            {
                RemoveAttachments(challenge, request.AttachmentIdsToRemove, _userService.CurrentUserId.ToString());
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // Persist to database
                await _attachmentRepository.AddListAsync(attachments, cancellationToken);
                await _challengeRepository.Update(challenge, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (request.FilesToUpload?.Count > 0)
                {
                    await UploadAttachmentsAsync(
                    challenge,
                    _userService.CompanyId,
                    _userService.CurrentUserId,
                    request.FilesToUpload,
                    request.AttachmentDescription,
                    cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {

                await transaction.RollbackAsync(cancellationToken);


                return Result<bool>.Failure(
                    ex is ValidationException ? ex.Message : "Failed to save Challenge with attachments.");
            }






        }


        private async Task UploadAttachmentsAsync(Domain.Aggregates.ChallengeRequest.ChallengeRequest challenge,
                                       Guid companyId,
                                       Guid contactId,
                                       IReadOnlyCollection<FileUploadContent> files,
                                       string? description,
                                       CancellationToken cancellationToken)
        {


            var uploadRequest = new FileUploadRequest(
                challenge.Id.ToString(),
                companyId,
                contactId,
                string.IsNullOrWhiteSpace(description) ? "challenge attachment" : description,
                files);

            var uploadResult = await _middlewareService.UploadFilesAsync(uploadRequest, cancellationToken);
            if (!uploadResult.Success)
            {
                throw new ValidationException(uploadResult.Message ?? "Attachment upload failed.");
            }

            await MapUploadToAttachmentRequests(uploadResult, files, challenge, cancellationToken);
        }

        private async Task MapUploadToAttachmentRequests(FileUploadResult uploadResult,
                                                         IReadOnlyCollection<FileUploadContent> originalFiles,
                                                         Domain.Aggregates.ChallengeRequest.ChallengeRequest challenge,
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
                var result = ChallengeRequestAttachment.Create(challenge.Id,
                 Attachment.Create(uploaded.FileName, size, Format.Documents, uploaded.SharePointUrl).Value, "",
                 uploaded.SharePointUrl, "", _userService.CurrentUserId);

                challenge.AddAttachment(result.Value);

            }

            // Save changes
            _challengeRepository.Update(challenge , cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        }


        private static void RemoveAttachments(Domain.Aggregates.ChallengeRequest.ChallengeRequest challenge, IEnumerable<Guid> attachmentIds, string userId)
        {
            foreach (var attachmentId in attachmentIds.Distinct())
            {
                if (attachmentId == Guid.Empty)
                {
                    continue;
                }

                var removeResult = challenge.RemoveAttachment(attachmentId,userId);
                if (removeResult.IsFailure)
                {
                    throw new ValidationException(removeResult.Error!);
                }
            }
        }


    }
}
