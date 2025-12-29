using MediatR;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;


namespace PartnersHub.InnovationHub.Application.Campaign.Queries;

public class EvaluatorListQueryHandler(IEvaluatorRepository _evaluatorRepository) : IRequestHandler<EvaluatorListQuery, List<EvaluatorDTO>>
{
    public async Task<List<EvaluatorDTO>> Handle(EvaluatorListQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return await Task.FromCanceled<List<EvaluatorDTO>>(cancellationToken);

        var evaluatorList = await _evaluatorRepository.GetAll(cancellationToken);

        return MapToDto(evaluatorList.ToList());
    }


    private List<EvaluatorDTO> MapToDto(List<Evaluator> SponsorList)
    {

        return SponsorList.Select(cr => new EvaluatorDTO
        {
            Id = cr.Id,
            Name = cr.NameEn

        }).ToList();
    }
}
