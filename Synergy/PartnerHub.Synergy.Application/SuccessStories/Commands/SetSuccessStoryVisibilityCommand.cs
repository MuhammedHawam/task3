using MediatR;
using PartnersHub.Synergy.Domain.Common;


namespace PartnersHub.Synergy.Application.SuccessStories.Commands;

public record SetSuccessStoryVisibilityCommand(Guid SuccessStoryId,bool Hide) : IRequest<Result>;
