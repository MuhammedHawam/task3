using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;
using PartnersHub.InnovationHub.Domain.Common;


namespace PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;

public class CampaignRequestEvaluator : Entity
{
    public Guid CampaignRequestId { get; private set; }
    public Guid EvaluatorId { get; private set; }
    public bool IsDeleted { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }


    private CampaignRequestEvaluator() { }
    public CampaignRequestEvaluator(Guid campaignRequestId, Guid evaluatorId)
    {
        if (campaignRequestId == Guid.Empty) throw new ArgumentNullException("CampaignRequestId is required.", nameof(campaignRequestId));
        if (evaluatorId == Guid.Empty) throw new ArgumentNullException("evaluatorId is required.", nameof(evaluatorId));

        CampaignRequestId = campaignRequestId;
        EvaluatorId = evaluatorId;
      

    }

    public static Result<CampaignRequestEvaluator> Create(Guid campaignRequestId, Guid EvaluatorId)
    {
        if (campaignRequestId == Guid.Empty)
            return Result<CampaignRequestEvaluator>.Failure("CampaignRequestId is required.");

        if (EvaluatorId == Guid.Empty)
            return Result<CampaignRequestEvaluator>.Failure("Evaluator is required.");


        var campaignRequestSponsor = new CampaignRequestEvaluator(campaignRequestId, EvaluatorId);

        return Result<CampaignRequestEvaluator>.Success(campaignRequestSponsor);
    }

    public Result MarkAsDeleted(Guid deletedBy)
    {
        if (IsDeleted)
            return Result.Failure("Evaluator is already deleted");

        if (deletedBy == Guid.Empty)
            return Result.Failure("Deleted by user is required");

        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
