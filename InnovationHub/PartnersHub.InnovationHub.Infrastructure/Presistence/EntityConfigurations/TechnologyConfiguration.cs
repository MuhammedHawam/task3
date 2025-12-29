using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations;

public class TechnologyConfiguration : EntityConfiguration<Technology>
{ 
    public void Configure(EntityTypeBuilder<Technology> builder) 
    {
        builder.ToTable("Technologies"); 

        builder.Property(t => t.Id).IsRequired(); 
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100); 
        // Set maximum length as needed
        builder.Property(t => t.TechnologyStage).IsRequired();   
        builder.Property(t => t.Sector).HasMaxLength(150); 
    }
}