using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations.CampaignConfiguration;

public class CampaignRequestEvaluationCriteriaConfiguration : IEntityTypeConfiguration<CampaignRequestEvaluationCriteria>
{
    public void Configure(EntityTypeBuilder<CampaignRequestEvaluationCriteria> builder)
    {
        builder.ToTable("CampaignRequestEvaluationCriteria");
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.CampaignRequestId)
            .IsRequired();

        builder.Property(c => c.CriteriaName)
            .HasMaxLength(3000);

        builder.Property(c => c.CriteriaValue)
           .IsRequired();
    }
}
