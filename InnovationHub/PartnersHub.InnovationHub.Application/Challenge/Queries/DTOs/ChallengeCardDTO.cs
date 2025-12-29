using PartnersHub.InnovationHub.Application.Common.Paging;
using PartnersHub.InnovationHub.Domain.Enums;

namespace PartnersHub.InnovationHub.Application.Challenge.Queries.DTOs;

public class ChallengeCardDTO
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DevCoName { get; init; } = string.Empty;
    public string DevCoLogoUrl { get; init; } = string.Empty;
    public string SectorName { get; init; } = string.Empty;
    public PriorityLevel PriorityLevel { get; init; }
    public DateTime CreatedAt { get; init; }
    public ChallengeStatus Status { get; init; }
    public bool IsArchived { get; init; }

    public string ShortId { get; init; }    
}

public class ChallengeStatusCount
{
    public ChallengeStatus ChallengeStatus { get; set; }
    public int Count { get; set; }
}

public class PriorityLevelCount
{
    public string PriorityName { get; set; }
    public int Count { get; set; }
}

public class SectorCount
{
    public string AssociatedSectorName { get; set; }
    public Guid AssociatedSectorId { get; set; }
    public int Count { get; set; }
}




