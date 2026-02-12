using MediatR;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Integration;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Application.Models;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest;

public class CreateChallengeRequestCommandHandler : IRequestHandler<CreateChallengeRequestCommand, Results<Guid>>
{
    private readonly IChallengeRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAssociatedProviderRepository _associatedProviderRepository;
    private readonly IAssociatedSectorRepository _associatedSectorRepository;
    private readonly INotificationService _NotificationService;
    private readonly ICurrentUserService _userService;
    private readonly IMiddlewareIntegrationService _middlewareService;

    public CreateChallengeRequestCommandHandler(
        IChallengeRequestRepository repository,
        IUnitOfWork unitOfWork,
        IAssociatedProviderRepository associatedProviderRepository,
        IAssociatedSectorRepository associatedSectorRepository,
        INotificationService notificationService,
        ICurrentUserService userService,
        IMiddlewareIntegrationService middlewareService
        )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _associatedProviderRepository = associatedProviderRepository;
        _associatedSectorRepository = associatedSectorRepository;
        _NotificationService = notificationService;
        _userService = userService;
        _middlewareService = middlewareService;
    }

    public async Task<Results<Guid>> Handle(CreateChallengeRequestCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate name
        if (await _repository.ExistsByNameAsync(request.Name,null, cancellationToken))
            return Results<Guid>.Failure("A challenge with this name already exists");

        var existingProvider = await _associatedProviderRepository.GetById(request.SourceCompany.id, cancellationToken);
        var existingSector = await _associatedSectorRepository.GetById(request.AssociatedSector.id, cancellationToken);

        var sourceCompanyId = existingProvider?.Id ?? request.SourceCompany.id;
        var sourceCompanyName = existingProvider?.Name ?? request.SourceCompany.name;

        var associatedSectorId = existingSector?.Id ?? request.AssociatedSector.id;
        var associatedSectorName = existingSector?.Name ?? request.AssociatedSector.name;

        // Create the challenge request using factory method
        var createResult = Domain.Aggregates.ChallengeRequest.ChallengeRequest.Create(
            request.UserId,
            request.Name,
            request.Description,
            sourceCompanyId,
            sourceCompanyName,
            associatedSectorId,
            associatedSectorName,
            request.SubmitterName,
            (int)request.PriorityLevel,
            existingProvider,
            existingSector,
            request.IsDraft,
            string.IsNullOrWhiteSpace(request.SubmitterEmail)? "con-mabdelkareem@pif.gov.sa": request.SubmitterEmail); // make front end send Email 

        // Return early if creation failed
        if (createResult.IsFailure)
            return Results<Guid>.Failure(createResult.Error!);

     


        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Persist to database
            await _repository.AddAsync(createResult.Value!, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

          





            if (request.FilesToUpload?.Count > 0)
            {
                await UploadAttachmentsAsync(
                createResult.Value!.Id,
                _userService.CompanyId,
                _userService.CurrentUserId,
                request.FilesToUpload,
                request.AttachmentDescription,
                cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            _NotificationService.SendChallengeSubmittedNotificationAsync(createResult.Value!.Id, request.Name);
            return Results<Guid>.Success(createResult.Value!.Id);
        }
        catch (Exception ex)
        {

            await transaction.RollbackAsync(cancellationToken);


            return Results<Guid>.Failure(
                ex is ValidationException ? ex.Message : "Failed to save Challenge with attachments.");
        }
    }


    private async Task UploadAttachmentsAsync(Guid challengeId,
                                              Guid companyId,
                                              Guid contactId,
                                              IReadOnlyCollection<FileUploadContent> files,
                                              string? description,
                                              CancellationToken cancellationToken)
    {


        var uploadRequest = new FileUploadRequest(
            challengeId.ToString(),
            companyId,
            contactId,
            string.IsNullOrWhiteSpace(description) ? "challenge attachment" : description,
            files);

        var uploadResult = await _middlewareService.UploadFilesAsync(uploadRequest, cancellationToken);
        if (!uploadResult.Success)
        {
            throw new ValidationException(uploadResult.Message ?? "Attachment upload failed.");
        }

        await MapUploadToAttachmentRequests(uploadResult, files, challengeId, cancellationToken);
    }

    private async Task MapUploadToAttachmentRequests(FileUploadResult uploadResult,
                                                     IReadOnlyCollection<FileUploadContent> originalFiles,
                                                     Guid challengeId,
                                                     CancellationToken cancellationToken)
    {
        var challenge = await _repository.GetById(challengeId, cancellationToken);

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
                  Domain.ValueObjects.Attachment.Create(uploaded.FileName, size, Format.Documents, uploaded.SharePointUrl).Value, "",
                  uploaded.SharePointUrl, "", _userService.CurrentUserId);

            challenge.AddAttachment(result.Value);

        }

        // Save changes
        _repository.Update(challenge, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }
}
