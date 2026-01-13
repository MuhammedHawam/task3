using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

namespace PartnersHub.InfraBase.Infrastructure.Persistence.EntityConfigurations;

public class AssetOpexConfiguration : IEntityTypeConfiguration<AssetOpex>
{
    public void Configure(EntityTypeBuilder<AssetOpex> builder)
    {
        builder.ToTable("AssetOpexDetails");

        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.AssetId)
            .IsRequired();

        builder.Property(o => o.Year)
            .IsRequired();

        builder.Property(o => o.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        // Configure the relationship from the child side
        builder.HasOne<Asset>()
            .WithMany(a => a.OpexDetails)
            .HasForeignKey(o => o.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.AssetId, o.Year })
            .IsUnique();
    }
}
