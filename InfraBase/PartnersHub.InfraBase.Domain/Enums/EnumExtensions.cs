namespace PartnersHub.InfraBase.Domain.Enums;

public static class EnumExtensions
{
    public static string GetDisplayName(this FundingModels fundingModel)
    {
        return fundingModel switch
        {
            FundingModels.FullySelfFunded => "Fully Self Funded",
            FundingModels.PublicPrivatePartnership => "Public Private Partnership",
            FundingModels.FullyGovernmentFunded => "Fully Government Funded",
            FundingModels.JointVenture => "Joint Venture",
            _ => fundingModel.ToString()
        };
    }

    public static string GetDisplayName(this TenderingStages tenderingStage)
    {
        return tenderingStage switch
        {
            TenderingStages.PreTender => "Pre Tender",
            TenderingStages.Tendered => "Tendered",
            TenderingStages.Award => "Award",
            TenderingStages.Execution => "Execution",
            TenderingStages.Delivered => "Delivered",
            _ => tenderingStage.ToString()
        };
    }

    public static string GetDisplayName(this DevelopmentTypes developmentType)
    {
        return developmentType switch
        {
            DevelopmentTypes.Greenfield => "Greenfield",
            DevelopmentTypes.Brownfield => "Brownfield",
            _ => developmentType.ToString()
        };
    }
}
