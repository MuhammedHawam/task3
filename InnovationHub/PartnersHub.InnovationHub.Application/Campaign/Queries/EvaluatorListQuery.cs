using MediatR;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;


namespace PartnersHub.InnovationHub.Application.Campaign.Queries;

public class EvaluatorListQuery : IRequest<List<EvaluatorDTO>>
{
}
