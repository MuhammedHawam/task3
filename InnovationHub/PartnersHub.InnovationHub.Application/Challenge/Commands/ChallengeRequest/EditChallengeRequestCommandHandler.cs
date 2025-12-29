using MediatR;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using PartnersHub.InnovationHub.Domain.ValueObjects;


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
          IAttachmentRepository _attachmentRepository) : IRequestHandler<EditChallengeRequestCommand, Result<bool>>
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

            // Persist to database
            await _attachmentRepository.AddListAsync(attachments, cancellationToken);
            await _challengeRepository.Update(challenge, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);

        }


    }
}
