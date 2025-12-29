using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.SuccessStories.Commands;

/// <summary>
/// Handler for adding an attachment to a success story
/// </summary>
public class AddSuccessStoryAttachmentCommandHandler 
    : IRequestHandler<AddSuccessStoryAttachmentCommand, Result<Guid>>
{
    private readonly ISuccessStoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddSuccessStoryAttachmentCommandHandler(
        ISuccessStoryRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddSuccessStoryAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the success story
        var successStory = await _repository.GetByIdAsync(
            request.SuccessStoryId,
            asNoTracking: false,
            s => s.Attachments);

        if (successStory == null)
        {
            return Result<Guid>.Failure("Success story not found");
        }

        // Add the attachment
        var result = successStory.AddAttachment(
            request.FileName,
            request.SharePointUrl,
            request.FileSizeInBytes,
            request.UploadedBy);

        if (result.IsFailure)
        {
            return Result<Guid>.Failure(result.Error!);
        }

        // Save changes
        _repository.Update(successStory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the attachment ID (last added)
        var attachment = successStory.Attachments.OrderByDescending(a => a.UploadedAt).First();
        return Result<Guid>.Success(attachment.Id);
    }
}

/// <summary>
/// Handler for removing an attachment from a success story
/// </summary>
public class RemoveSuccessStoryAttachmentCommandHandler 
    : IRequestHandler<RemoveSuccessStoryAttachmentCommand, Result<bool>>
{
    private readonly ISuccessStoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveSuccessStoryAttachmentCommandHandler(
        ISuccessStoryRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        RemoveSuccessStoryAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the success story
        var successStory = await _repository.GetByIdAsync(
            request.SuccessStoryId,
            asNoTracking: false,
            s => s.Attachments);

        if (successStory == null)
        {
            return Result<bool>.Failure("Success story not found");
        }

        // Remove the attachment
        var result = successStory.RemoveAttachment(request.AttachmentId);

        if (result.IsFailure)
        {
            return Result<bool>.Failure(result.Error!);
        }

        // Save changes
        _repository.Update(successStory);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

/// <summary>
/// Handler for getting all attachments for a success story
/// </summary>
public class GetSuccessStoryAttachmentsQueryHandler 
    : IRequestHandler<GetSuccessStoryAttachmentsQuery, Result<List<SuccessStoryAttachmentDto>>>
{
    private readonly ISuccessStoryRepository _repository;

    public GetSuccessStoryAttachmentsQueryHandler(ISuccessStoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<SuccessStoryAttachmentDto>>> Handle(
        GetSuccessStoryAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        // Get the success story with attachments
        var successStory = await _repository.GetByIdAsync(
            request.SuccessStoryId,
            asNoTracking: true,
            s => s.Attachments);

        if (successStory == null)
        {
            return Result<List<SuccessStoryAttachmentDto>>.Failure("Success story not found");
        }

        // Map to DTOs
        var attachmentDtos = successStory.Attachments
            .Select(a => new SuccessStoryAttachmentDto
            {
                Id = a.Id,
                SuccessStoryId = a.SuccessStoryId,
                FileName = a.FileName,
                SharePointUrl = a.SharePointUrl,
                FileExtension = a.FileExtension,
                FileSizeInBytes = a.FileSizeInBytes,
                UploadedAt = a.UploadedAt,
                UploadedBy = a.UploadedBy
            })
            .OrderByDescending(a => a.UploadedAt)
            .ToList();

        return Result<List<SuccessStoryAttachmentDto>>.Success(attachmentDtos);
    }
}
