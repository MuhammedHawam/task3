using MediatR;
using PartnersHub.InnovationHub.Application.Common.Interfaces;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Integration;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Application.Models;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System.ComponentModel.DataAnnotations;


namespace PartnersHub.InnovationHub.Application.Campaign.Commands;

public record CreateCampaignCommandHandler(
              IUnitOfWork _unitOfWork,
              ICurrentUserService _userService,
              ICampaignRequestRepository _campaignRequestRepository,
              ICampaignRequestLinkedChallengeRepository _campaignRequestLinkedChallengeRepository,
              ICampaignRequestSponsorRepository _campaignRequestSponsorRepository,
              ICampaignRequestTermsAndConditionRepository _campaignRequestTermsAndConditionRepository,
              INotificationService _notificationService,
              IMiddlewareIntegrationService _middlewareService
                           ) : IRequestHandler<CreateCampaignCommand, Results<Guid>>
{



    public async Task<Results<Guid>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate name
        if (await _campaignRequestRepository.ExistsByNameAsync(request.CampaignName, null, cancellationToken))
            return Results<Guid>.Failure("A Campaign with this name already exists");



        // Create the Campaign request using factory method
        var createResult = CampaignRequest.CreateCampaign(request.SubmitterId,
                                                  request.CampaignName,
                                                  request.Description,
                                                  request.ProblemStatement,
                                                  request.Type,
                                                  request.SubmitterName,
                                                  request.LaunchDate,
                                                  request.SubmissionDeadlineDate,
                                                  request.IsDraft == true ? CampaignRequestStatus.Draft :  CampaignRequestStatus.PendingReview,
                                                  request.EvaluatorList.Select(el => (Id: el.id, Name: el.name)).ToList(),
                                                  request.SponsorsList.Select(el => (Id: el.id, Name: el.name)).ToList(),
                                                  request.EvaluationCriteriaList.Select(el => (name: el.name,value: el.value)).ToList(),
                                                  request.LinkedDevCoChallenges,
                                                  string.IsNullOrWhiteSpace(request.SubmitterEmail) ? "con-mabdelkareem@pif.gov.sa" : request.SubmitterEmail);

        // Return early if creation failed
        if (createResult.IsFailure)
            return Results<Guid>.Failure(createResult.Error!);



       



        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Persist to database
            await _campaignRequestRepository.AddAsync(createResult.Value!, cancellationToken);
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

            _notificationService.SendCampaignSubmittedNotificationAsync(createResult.Value!.Id, request.CampaignName);

            return Results<Guid>.Success(createResult.Value!.Id);
        }
        catch (Exception ex)
        {

            await transaction.RollbackAsync(cancellationToken);


            return Results<Guid>.Failure(
                ex is ValidationException ? ex.Message : "Failed to save Campaign with attachments.");
        }

    }



    private async Task UploadAttachmentsAsync(Guid opportunityId,
                                          Guid companyId,
                                          Guid contactId,
                                          IReadOnlyCollection<FileUploadContent> files,
                                          string? description,
                                          CancellationToken cancellationToken)
    {


        var uploadRequest = new FileUploadRequest(
            opportunityId.ToString(),
            companyId,
            contactId,
            string.IsNullOrWhiteSpace(description) ? "opportunity attachment" : description,
            files);

        var uploadResult = await _middlewareService.UploadFilesAsync(uploadRequest, cancellationToken);
        if (!uploadResult.Success)
        {
            throw new ValidationException(uploadResult.Message ?? "Attachment upload failed.");
        }

        await MapUploadToAttachmentRequests(uploadResult, files, opportunityId, cancellationToken);
    }

    private async Task MapUploadToAttachmentRequests(FileUploadResult uploadResult,
                                                     IReadOnlyCollection<FileUploadContent> originalFiles,
                                                     Guid campaignId,
                                                     CancellationToken cancellationToken)
    {
        var campaign = await _campaignRequestRepository.GetById(campaignId, cancellationToken);

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

            var result = CampaignRequestTermsAndCondition.Create(campaignId,
              Domain.ValueObjects.Attachment.Create(uploaded.FileName, size, Format.Documents, uploaded.SharePointUrl).Value, "",
              uploaded.SharePointUrl, "", _userService.CurrentUserId);

            campaign.AddTermsAndCondition(result.Value);

        }

        // Save changes
        _campaignRequestRepository.Update(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }
}
