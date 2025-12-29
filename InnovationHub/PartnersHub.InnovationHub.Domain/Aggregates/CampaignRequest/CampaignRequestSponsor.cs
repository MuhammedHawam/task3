using PartnersHub.InnovationHub.Domain.Common;
using PartnersHub.InnovationHub.Domain.Enums;
using System.Xml.Linq;



namespace PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;

public class CampaignRequestSponsor : Entity
{
    public Guid CampaignRequestId { get; private set; }
    public Guid SponsorId { get; private set; }
    public string SponserName { get; private set; }
    public bool IsDeleted { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private CampaignRequestSponsor() { }
    private CampaignRequestSponsor(Guid campaignRequestId, Guid sponsorId, string sponserName)
    {
        

        CampaignRequestId = campaignRequestId;
        SponsorId = sponsorId;
        SponserName = sponserName;

    }

    public static Result<CampaignRequestSponsor> Create(Guid campaignRequestId, Guid sponsorId, string sponserName)
    {
        if (campaignRequestId == Guid.Empty) 
                  return Result<CampaignRequestSponsor>.Failure("CampaignRequestId is required.");

        if (sponsorId == Guid.Empty)
                  return Result<CampaignRequestSponsor>.Failure("SponsorId is required.");
        

        var campaignRequestSponsor = new CampaignRequestSponsor(campaignRequestId,
                                                                sponsorId,
                                                                sponserName);

        return Result<CampaignRequestSponsor>.Success(campaignRequestSponsor);
    }

    public Result MarkAsDeleted(Guid deletedBy)
    {
        if (IsDeleted)
            return Result.Failure("Sponser is already deleted");

        if (deletedBy == Guid.Empty)
            return Result.Failure("Deleted by user is required");

        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
