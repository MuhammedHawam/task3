using MediatR;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Enums;

namespace PartnersHub.InnovationHub.Application.Challenge.Queries.ChallengeRequest;

public class ChallengeDetailsHandler : IRequestHandler<ChallengeDetailsQuery, ChallengeDetailsDTO?>
{
    private readonly IChallengeRequestRepository _repository;

    public ChallengeDetailsHandler(IChallengeRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ChallengeDetailsDTO?> Handle(ChallengeDetailsQuery query, CancellationToken cancellationToken)
    {
        var request = await _repository.GetById(query.ChallengeId, cancellationToken);

        if (request == null)
            return null;

        return new ChallengeDetailsDTO
        {
            Name = request.Name,
            SubmitterName = request.SubmitterName,
            Description = request.Description,
            SourceCompany = request.SourceCompany != null
                ? new AssociatedProviderModel
                {
                    Id = request.SourceCompany.Id,
                    Name = request.SourceCompany.Name,
                }
                : null,
            AssociatedSector = request.AssociatedSector != null
                ? new AssociatedSectorModel
                {
                    Id = request.AssociatedSector.Id,
                    Name = request.AssociatedSector.Name,
                    LogoUrl = ""
                }
                : null,
            PriorityLevel = (PriorityLevel)request.PriorityLevelId,
            DateAdded = request.CreatedAt,
            ChallengeStatus = request.IsArchived == true ? ChallengeStatus.Archived : request.ChallengeStatus,
            IsDraft = request.IsDraft,
            Attachments = request.Attachments.Select(a => new AttachmentDto(){
                   Id = a.Id,
                   FileName = a.Metadata.Name,
                   FileSizeInBytes  = a.Metadata.SizeInBytes,
                   ContentType = "",
                   SharePointUrl= a.SharePointUrl,
                   UploadedAt = a.UploadedAt
                   }).ToList(),
            Technologies = request.Technologies.Select(t => new TechnologyDTO(t.TechnologyId,t.LinkedTechnology.Name,t.JustificationForLinking,t.RequestStatus,t.CreatedAt , t.LinkedTechnology.TechnologyStage, t.LinkedTechnology.Sector,"")).ToList(),
            Comment = request.RevisionComments?.OrderByDescending(e => e.CommentedAt).FirstOrDefault(e => e.IsCurrent == true)?.Content
        };
    }
}
