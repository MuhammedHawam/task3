using MediatR;
using PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;


namespace PartnersHub.InnovationHub.Application.Challenge.Queries;

public record ChallengeDashboardQuery : IRequest<ChallengeDashboardDto>;
