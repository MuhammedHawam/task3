using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.Synergy.Domain.Aggregates.SuccessStoryAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Infrastructure.Persistence.EntityConfigurations
{
    public class SuccessStoryOpportunityConfiguration : IEntityTypeConfiguration<SuccessStoryOpportunity>
    {
        public void Configure(EntityTypeBuilder<SuccessStoryOpportunity> builder)
        {
            builder.ToTable("SuccessStoryOpportunities");

        }
    }
}
