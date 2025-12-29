using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;


namespace PartnersHub.InnovationHub.Infrastructure.Presistence.Repositories;



public class AttachmentRepository(InnovationHubDbContext dbContext) : IAttachmentRepository
{
    public async Task AddListAsync(List<ChallengeRequestAttachment> attachmentList, CancellationToken cancellationToken)
    {
        await dbContext.attachments.AddRangeAsync(attachmentList, cancellationToken);
    }


}
