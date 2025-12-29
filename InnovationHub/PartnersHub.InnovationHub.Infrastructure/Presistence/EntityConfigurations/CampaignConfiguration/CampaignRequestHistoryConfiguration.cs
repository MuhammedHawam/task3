using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates.CampaignRequest;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations.CampaignConfiguration;

public class CampaignRequestHistoryConfiguration : IEntityTypeConfiguration<CampaignTrackingHistory>
{
    public  void Configure(EntityTypeBuilder<CampaignTrackingHistory> builder)
    {

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.CampaignRequestId)
            .IsRequired();


        builder.Property(cr => cr.Status)
               .HasColumnName("Status")
               .HasConversion(
                              name => (int)name,
                              value => (CampaignRequestStatus)value);




        builder.Property(h => h.Action)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.PerformedBy)
            .IsRequired();

        builder.Property(h => h.PerformedAt)
            .IsRequired();

        builder.Property(h => h.Comments)
            .HasMaxLength(3000);

        builder.Property(h => h.FieldsChanged)
            .HasMaxLength(1000);

        builder.Property(h => h.OldValues)
            .HasMaxLength(2000);

        builder.Property(h => h.NewValues)
            .HasMaxLength(2000);


        // Index for querying history by request
        builder.HasIndex(h => h.CampaignRequestId);

        builder.ToTable("CampaignTrackingHistory");
    }



}
