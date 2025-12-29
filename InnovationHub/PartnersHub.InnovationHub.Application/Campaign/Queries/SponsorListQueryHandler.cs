using MediatR;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;
using PartnersHub.InnovationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;


namespace PartnersHub.InnovationHub.Application.Campaign.Queries;

public class SponsorListQueryHandler(ISponsorRepository _sponsorRepository) : IRequestHandler<SponsorListQuery, List<SponsorDTO>>
{
    public async Task<List<SponsorDTO>> Handle(SponsorListQuery request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return await Task.FromCanceled<List<SponsorDTO>>(cancellationToken);

        var sponsorList = await _sponsorRepository.GetAll(cancellationToken);

        return MapToDto(sponsorList.ToList());
    }


    private List<SponsorDTO> MapToDto(List<Sponsor> SponsorList)
    {

              return SponsorList.Select(cr => new SponsorDTO
              {
                    Id = cr.Id,
                    Name = cr.NameEn

              }).ToList();
    }
}
