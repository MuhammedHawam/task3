using MediatR;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Common;


namespace PartnersHub.InnovationHub.Application.Campaign.Commands;

/// <summary>
/// Handler for adding an attachment to a Campaign
/// </summary>
public class AddCampaignAttachmentCommandHandler
    : IRequestHandler<AddCampaignAttachmentCommand, Result<Guid>>
{
    private readonly ICampaignRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCampaignAttachmentCommandHandler(
        ICampaignRequestRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddCampaignAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the Campaign
        var Campaign = await _repository.GetById(request.CampaignId, cancellationToken);

        if (Campaign == null)
        {
            return Result<Guid>.Failure("Campaign not found");
        }

        var result = CampaignRequestTermsAndCondition.Create(request.CampaignId,
              Domain.ValueObjects.Attachment.Create(request.FileName, request.FileSizeInBytes, Domain.Enums.Format.Documents, request.SharePointUrl).Value, "",
              request.SharePointUrl, "", request.UploadedBy);




        if (result.IsFailure)
        {
            return Result<Guid>.Failure(result.Error!);
        }

        Campaign.AddTermsAndCondition(result.Value);

        // Save changes
        _repository.Update(Campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the attachment ID (last added)
        var attachment = Campaign.TermsAndCondition.OrderByDescending(a => a.UploadedAt).First();
        return Result<Guid>.Success(attachment.Id);
    }
}

/// <summary>
/// Handler for removing an attachment from a Campaign
/// </summary>
public class RemoveCampaignAttachmentCommandHandler
    : IRequestHandler<RemoveCampaignAttachmentCommand, Result<bool>>
{
    private readonly ICampaignRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveCampaignAttachmentCommandHandler(
        ICampaignRequestRepository repository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(
        RemoveCampaignAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the Campaign
        var Campaign = await _repository.GetById(request.CampaignId, cancellationToken);


        if (Campaign == null)
        {
            return Result<bool>.Failure("Campaign not found");
        }

        // Remove the attachment
        var result = Campaign.RemoveTermsAndCondition(request.AttachmentId, _currentUserService.UserId);

        if (result.IsFailure)
        {
            return Result<bool>.Failure(result.Error!);
        }

        // Save changes
        _repository.Update(Campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

/// <summary>
/// Handler for getting all attachments for a success story
/// </summary>
public class GetCampaignAttachmentsQueryHandler
    : IRequestHandler<GetCampaignAttachmentsQuery, Result<List<CampaignAttachmentDto>>>
{
    private readonly ICampaignRequestRepository _repository;

    public GetCampaignAttachmentsQueryHandler(ICampaignRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<CampaignAttachmentDto>>> Handle(
        GetCampaignAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        // Get the Campaign with attachments
        var Campaign = await _repository.GetById(request.CampaignId, cancellationToken);


        if (Campaign == null)
        {
            return Result<List<CampaignAttachmentDto>>.Failure("Campaign not found");
        }

        // Map to DTOs
        var attachmentDtos = Campaign.TermsAndCondition
            .Select(a => new CampaignAttachmentDto
            {
                Id = a.Id,
                CampaignId = a.CampaignRequestId,
                FileName = a.Metadata.Name,
                SharePointUrl = a.SharePointUrl,
                FileExtension = a.Metadata.Extension.ToString(),
                FileSizeInBytes = a.Metadata.SizeInBytes,
                UploadedAt = a.UploadedAt,
                UploadedBy = a.UploadedBy.ToString(),
            })
            .OrderByDescending(a => a.UploadedAt)
            .ToList();

        return Result<List<CampaignAttachmentDto>>.Success(attachmentDtos);
    }
}
