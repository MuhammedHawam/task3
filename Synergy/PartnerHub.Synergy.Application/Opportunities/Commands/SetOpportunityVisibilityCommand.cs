using MediatR;
using PartnersHub.Synergy.Domain.Common;
using System.ComponentModel.DataAnnotations;


namespace PartnersHub.Synergy.Application.Opportunities.Commands;

public record SetOpportunityVisibilityCommand(
    Guid OpportunityId,
    bool Hide 
) : IRequest<Result>;
