using MediatR;
using PartnersHub.InnovationHub.Application.Campaign.Queries.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Campaign.Queries;

public class SponsorListQuery : IRequest<List<SponsorDTO>>
{
}
