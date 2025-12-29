using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.Synergy.Domain.Aggregates.OpportunityAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Infrastructure.Persistence.EntityConfigurations
{
    public class OpportunitiyExpectedOutcomeConfiguration : IEntityTypeConfiguration<OpportunityExpectedOutcome>
    {
        public void Configure(EntityTypeBuilder<OpportunityExpectedOutcome> builder)
        {
            builder.ToTable("OpportunityExpectedOutcomes");
            builder.HasKey(oeo => oeo.Id);
            builder.Property(oeo => oeo.Id).ValueGeneratedNever();

            builder.Property(oeo => oeo.OpportunityId).IsRequired();
            builder.Property(oeo => oeo.ExpectedOutcomeId).IsRequired();

            builder.HasIndex(oeo => oeo.OpportunityId);
            builder.HasIndex(oeo => oeo.ExpectedOutcomeId);
            builder.HasIndex(oeo => new { oeo.OpportunityId, oeo.ExpectedOutcomeId }).IsUnique();

        }
    }

}
