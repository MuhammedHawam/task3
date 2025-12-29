using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;


namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations.CampaignConfiguration;

public class CampaignRequestLinkedChallengeConfiguration : IEntityTypeConfiguration<CampaignRequestLinkedChallenge>
{
    public void Configure(EntityTypeBuilder<CampaignRequestLinkedChallenge> builder)
    {
        builder.ToTable("CampaignRequestLinkedChallenges");
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.HasKey(lc => new { lc.CampaignRequestId,lc.ChallengeRequestId });

        builder.Property(lc => lc.CampaignRequestId)
            .IsRequired();

        builder.Property(lc => lc.ChallengeRequestId)
            .IsRequired();

    }
}
