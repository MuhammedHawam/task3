
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;


namespace PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;

public interface IAttachmentRepository
{
    Task AddListAsync(List<ChallengeRequestAttachment> attachmentList, CancellationToken cancellationToken);
}
