using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

namespace PartnersHub.InfraBase.Infrastructure.Persistence.EntityConfigurations;

public class AssetCapexConfiguration : IEntityTypeConfiguration<AssetCapex>
{
    public void Configure(EntityTypeBuilder<AssetCapex> builder)
    {
        builder.ToTable("AssetCapexDetails");

        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.AssetId)
            .IsRequired();

        builder.Property(c => c.Year)
            .IsRequired();

        builder.Property(c => c.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        // Configure the relationship from the child side
        builder.HasOne<Asset>()
            .WithMany(a => a.CapexDetails)
            .HasForeignKey(c => c.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.AssetId, c.Year })
            .IsUnique();
    }
}
