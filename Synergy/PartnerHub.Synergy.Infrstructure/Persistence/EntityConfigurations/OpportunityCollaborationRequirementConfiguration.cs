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
    public class OpportunityCollaborationRequirementConfiguration : IEntityTypeConfiguration<OpportunityCollaborationRequirement>
    {
        public void Configure(EntityTypeBuilder<OpportunityCollaborationRequirement> builder)
        {
            builder.ToTable("OpportunityCollaborationRequirements");
            builder.HasKey(ocr => ocr.Id);
            builder.Property(ocr => ocr.Id).ValueGeneratedNever();

            builder.Property(ocr => ocr.OpportunityId).IsRequired();
            builder.Property(ocr => ocr.CollaborationRequirementId).IsRequired();

            builder.HasIndex(ocr => ocr.OpportunityId);
            builder.HasIndex(ocr => ocr.CollaborationRequirementId);
            builder.HasIndex(ocr => new { ocr.OpportunityId, ocr.CollaborationRequirementId }).IsUnique();

        }
    }
}
