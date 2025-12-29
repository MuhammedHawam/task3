using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates.CampaignRequest;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations;

public class ChallengeTrackingHistoryConfiguration : IEntityTypeConfiguration<ChallengeTrackingHistory>
{
    public void Configure(EntityTypeBuilder<ChallengeTrackingHistory> builder)
    {

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.ChallengeId)
            .IsRequired();


        builder.Property(cr => cr.Status)
               .HasColumnName("Status")
               .HasConversion(
                              name => (int)name,
                              value => (ChallengeStatus)value);

        builder.Property(h => h.Description)
            .HasMaxLength(3000);

        builder.Property(h => h.ChangedBy);


        // Index for querying history by request
        builder.HasIndex(h => h.ChallengeId);

        builder.ToTable("challengeTrackingHistories");
    }

}
