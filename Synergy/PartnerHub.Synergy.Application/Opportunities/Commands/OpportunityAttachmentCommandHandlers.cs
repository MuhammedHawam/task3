using MediatR;
using PartnersHub.Synergy.Application.Interfaces;
using PartnersHub.Synergy.Application.Interfaces.Repository;
using PartnersHub.Synergy.Domain.Common;

namespace PartnersHub.Synergy.Application.Opportunities.Commands;

/// <summary>
/// Handler for adding an attachment to an opportunity
/// </summary>
public class AddOpportunityAttachmentCommandHandler 
    : IRequestHandler<AddOpportunityAttachmentCommand, Result<Guid>>
{
    private readonly IOpportunityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddOpportunityAttachmentCommandHandler(
        IOpportunityRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddOpportunityAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the opportunity
        var opportunity = await _repository.GetByIdAsync(
            request.OpportunityId,
            asNoTracking: false,
            o => o.Attachments);

        if (opportunity == null)
        {
            return Result<Guid>.Failure("Opportunity not found");
        }

        // Add the attachment
        var result = opportunity.AddAttachment(
            request.FileName,
            request.SharePointUrl,
            request.FileSizeInBytes,
            request.UploadedBy);

        if (result.IsFailure)
        {
            return Result<Guid>.Failure(result.Error!);
        }

        // Save changes
        _repository.Update(opportunity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the attachment ID (last added)
        var attachment = opportunity.Attachments.OrderByDescending(a => a.UploadedAt).First();
        return Result<Guid>.Success(attachment.Id);
    }
}

/// <summary>
/// Handler for removing an attachment from an opportunity
/// </summary>
public class RemoveOpportunityAttachmentCommandHandler 
    : IRequestHandler<RemoveOpportunityAttachmentCommand, Result<bool>>
{
    private readonly IOpportunityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveOpportunityAttachmentCommandHandler(
        IOpportunityRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(
        RemoveOpportunityAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the opportunity
        var opportunity = await _repository.GetByIdAsync(
            request.OpportunityId,
            asNoTracking: false,
            o => o.Attachments);

        if (opportunity == null)
        {
            return Result<bool>.Failure("Opportunity not found");
        }

        // Remove the attachment
        var result = opportunity.RemoveAttachment(request.AttachmentId);

        if (result.IsFailure)
        {
            return Result<bool>.Failure(result.Error!);
        }

        // Save changes
        _repository.Update(opportunity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

/// <summary>
/// Handler for getting all attachments for an opportunity
/// </summary>
public class GetOpportunityAttachmentsQueryHandler 
    : IRequestHandler<GetOpportunityAttachmentsQuery, Result<List<OpportunityAttachmentDto>>>
{
    private readonly IOpportunityRepository _repository;

    public GetOpportunityAttachmentsQueryHandler(IOpportunityRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<OpportunityAttachmentDto>>> Handle(
        GetOpportunityAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        // Get the opportunity with attachments
        var opportunity = await _repository.GetByIdAsync(
            request.OpportunityId,
            asNoTracking: true,
            o => o.Attachments);

        if (opportunity == null)
        {
            return Result<List<OpportunityAttachmentDto>>.Failure("Opportunity not found");
        }

        // Map to DTOs
        var attachmentDtos = opportunity.Attachments
            .Select(a => new OpportunityAttachmentDto
            {
                Id = a.Id,
                OpportunityId = a.OpportunityId,
                FileName = a.FileName,
                SharePointUrl = a.SharePointUrl,
                FileExtension = a.FileExtension,
                FileSizeInBytes = a.FileSizeInBytes,
                UploadedAt = a.UploadedAt,
                UploadedBy = a.UploadedBy
            })
            .OrderByDescending(a => a.UploadedAt)
            .ToList();

        return Result<List<OpportunityAttachmentDto>>.Success(attachmentDtos);
    }
}
