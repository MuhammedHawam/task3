using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

namespace PartnersHub.InfraBase.Infrastructure.Persistence.EntityConfigurations;

public class AssetHistoryConfiguration : IEntityTypeConfiguration<AssetHistory>
{
    public void Configure(EntityTypeBuilder<AssetHistory> builder)
    {
        builder.ToTable("AssetHistories");

        builder.HasKey(h => h.Id);
        
        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.AssetId)
            .IsRequired();

        builder.Property(h => h.Status)
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(h => h.Action)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.PerformedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(h => h.PerformedAt)
            .IsRequired();

        builder.Property(h => h.Comments)
            .HasMaxLength(3000)
            .IsRequired(false);

        builder.Property(h => h.FieldsChanged)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(h => h.OldValues)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(h => h.NewValues)
            .HasMaxLength(2000)
            .IsRequired(false);

        // Configure the relationship from the child side
        builder.HasOne<Asset>()
            .WithMany(a => a.History)
            .HasForeignKey(h => h.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => new { h.AssetId, h.PerformedAt });
    }
}
