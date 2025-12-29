using MediatR;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Challenge.Commands.ChallengeRequest;

/// <summary>
/// Handler for adding an attachment to a Challenge
/// </summary>
public class AddChallengeAttachmentCommandHandler
    : IRequestHandler<AddChallengeAttachmentCommand, Result<Guid>>
{
    private readonly IChallengeRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddChallengeAttachmentCommandHandler(
        IChallengeRequestRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddChallengeAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the challenge
        var challenge = await _repository.GetById(request.ChallengeId, cancellationToken);

        if (challenge == null)
        {
            return Result<Guid>.Failure("challenge not found");
        }

        var result = ChallengeRequestAttachment.Create(request.ChallengeId,
              Domain.ValueObjects.Attachment.Create(request.FileName,request.FileSizeInBytes,Domain.Enums.Format.Documents,request.SharePointUrl).Value,"",
              request.SharePointUrl, "", request.UploadedBy);


      

        if (result.IsFailure)
        {
            return Result<Guid>.Failure(result.Error!);
        }

        challenge.AddAttachment(result.Value);

        // Save changes
        _repository.Update(challenge, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the attachment ID (last added)
        var attachment = challenge.Attachments.OrderByDescending(a => a.UploadedAt).First();
        return Result<Guid>.Success(attachment.Id);
    }
}

/// <summary>
/// Handler for removing an attachment from a Challenge
/// </summary>
public class RemoveChallengeAttachmentCommandHandler
    : IRequestHandler<RemoveChallengeAttachmentCommand, Result<bool>>
{
    private readonly IChallengeRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveChallengeAttachmentCommandHandler(
        IChallengeRequestRepository repository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(
        RemoveChallengeAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the Challenge
        var challenge = await _repository.GetById(request.ChallengeId, cancellationToken);


        if (challenge == null)
        {
            return Result<bool>.Failure("Challenge not found");
        }

        // Remove the attachment
        var result = challenge.RemoveAttachment(request.AttachmentId, _currentUserService.UserId);

        if (result.IsFailure)
        {
            return Result<bool>.Failure(result.Error!);
        }

        // Save changes
        _repository.Update(challenge, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

/// <summary>
/// Handler for getting all attachments for a success story
/// </summary>
public class GetChallengeAttachmentsQueryHandler
    : IRequestHandler<GetChallengeAttachmentsQuery, Result<List<ChallengeAttachmentDto>>>
{
    private readonly IChallengeRequestRepository _repository;

    public GetChallengeAttachmentsQueryHandler(IChallengeRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ChallengeAttachmentDto>>> Handle(
        GetChallengeAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        // Get the Challenge with attachments
        var challenge = await _repository.GetById(request.ChallengeId, cancellationToken);


        if (challenge == null)
        {
            return Result<List<ChallengeAttachmentDto>>.Failure("Challenge not found");
        }

        // Map to DTOs
        var attachmentDtos = challenge.Attachments
            .Select(a => new ChallengeAttachmentDto
            {
                Id = a.Id,
                ChallengeId = a.ChallengeRequestId,
                FileName = a.Metadata.Name,
                SharePointUrl = a.SharePointUrl,
                FileExtension = a.Metadata.Extension.ToString(),
                FileSizeInBytes = a.Metadata.SizeInBytes,
                UploadedAt = a.UploadedAt,
                UploadedBy = a.UploadedBy.ToString(),
            })
            .OrderByDescending(a => a.UploadedAt)
            .ToList();

        return Result<List<ChallengeAttachmentDto>>.Success(attachmentDtos);
    }
}
