using PartnersHub.InnovationHub.Domain.Common;
using System.Data.Common;


namespace PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;


public class CampaignRequestEvaluationCriteria : Entity
{
    public Guid CampaignRequestId { get; private set; }
    public string CriteriaName { get; private set; }
    public int CriteriaValue { get; private set; }
    public bool IsDeleted { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private CampaignRequestEvaluationCriteria() { }
    public CampaignRequestEvaluationCriteria(Guid campaignId, string name , int value)
    {
        if (campaignId == Guid.Empty) throw new ArgumentNullException("CampaignId is required.", nameof(campaignId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("CriteriaName is required.", nameof(name));

        CampaignRequestId = campaignId;
        CriteriaName = name;
        CriteriaValue = value;

    }

    public static Result<CampaignRequestEvaluationCriteria> Create(Guid campaignRequestId, string CriteriaName, int CriteriaValue)
    {
        if (campaignRequestId == Guid.Empty)
            return Result<CampaignRequestEvaluationCriteria>.Failure("CampaignRequestId is required.");

        var campaignRequestSponsor = new CampaignRequestEvaluationCriteria(campaignRequestId,
                                                                CriteriaName,
                                                                CriteriaValue);

        return Result<CampaignRequestEvaluationCriteria>.Success(campaignRequestSponsor);
    }

    public void Update(string name, int value)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Evaluation Criteria is required.", nameof(name));
        if (value < 0 || value > 100) throw new ArgumentOutOfRangeException(nameof(value), "0..100");

        CriteriaValue = value;
        CriteriaName = name.Trim();
    }

    public Result MarkAsDeleted(Guid deletedBy)
    {
        if (IsDeleted)
            return Result.Failure("Attachment is already deleted");

        if (deletedBy == Guid.Empty)
            return Result.Failure("Deleted by user is required");

        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
