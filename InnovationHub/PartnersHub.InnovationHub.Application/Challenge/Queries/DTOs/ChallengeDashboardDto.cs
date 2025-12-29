using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;

public class ChallengeDashboardDto
{
    public List<ChallengeStatusCount> StatusCountList { get; set; }
    public List<PriorityLevelCount> PriorityCountList { get; set; }
    public List<SectorCount> SectorCountList { get; set; }
    public int TotalCount { get; set; } 


}