using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;


namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations.CampaignConfiguration;


public class CampaignRequestSponsorConfiguration : IEntityTypeConfiguration<CampaignRequestSponsor>
{
    public void Configure(EntityTypeBuilder<CampaignRequestSponsor> builder)
    {
        builder.ToTable("CampaignRequestSponsors");

        builder.HasKey(lc => new { lc.Id });

        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(lc => lc.CampaignRequestId)
            .IsRequired();

        builder.Property(lc => lc.SponsorId)
            .IsRequired();

        builder.Property(asct => asct.SponserName)
             .HasColumnName("Name")
             .HasMaxLength(255)
             .IsRequired();
    }
}
