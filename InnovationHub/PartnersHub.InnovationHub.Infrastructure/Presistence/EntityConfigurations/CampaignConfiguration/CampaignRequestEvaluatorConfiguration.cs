using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations.CampaignConfiguration;

public class CampaignRequestEvaluatorConfiguration : IEntityTypeConfiguration<CampaignRequestEvaluator>
{
    public void Configure(EntityTypeBuilder<CampaignRequestEvaluator> builder)
    {
        builder.ToTable("CampaignRequestEvaluator");
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.CampaignRequestId)
            .IsRequired();

        builder.Property(c => c.EvaluatorId)
           .IsRequired();

       // builder
       // .HasOne(e => e.Evaluator)
       //.WithMany(ev => ev.CampaignRequestEvaluators)
       // .HasForeignKey(e => e.EvaluatorId)
       // .OnDelete(DeleteBehavior.Cascade);
    }
}
