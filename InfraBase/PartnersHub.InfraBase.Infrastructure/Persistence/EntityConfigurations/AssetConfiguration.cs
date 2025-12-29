using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

namespace PartnersHub.InfraBase.Infrastructure.Persistence.EntityConfigurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AssetCode)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.OwnsOne(a => a.AssetName, an =>
        {
            an.Property(n => n.Value)
                .HasColumnName("AssetName")
                .HasMaxLength(300) 
                .IsRequired();
        });

        builder.OwnsOne(a => a.LocationCity, lc =>
        {
            lc.Property(c => c.Value)
                .HasColumnName("LocationCity")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(a => a.SectorId).IsRequired(false);
        builder.Property(a => a.SubSectorId).IsRequired(false);
        builder.Property(a => a.AssetTypeId).IsRequired(false);
        builder.Property(a => a.AssetTypeOther).HasMaxLength(200).IsRequired(false);
        builder.Property(a => a.QuantityOfAsset).HasColumnType("decimal(18,2)").IsRequired(false);
        builder.Property(a => a.CapacityPerAsset).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(a => a.UnitOfMeasurementId).IsRequired(false);
        builder.Property(a => a.UnitOfMeasurementOther).HasMaxLength(200).IsRequired(false);

        builder.OwnsOne(a => a.Description, d =>
        {
            d.Property(desc => desc.Value)
                .HasColumnName("Description")
                .HasMaxLength(3000) 
                .IsRequired(false);
        });

        builder.Property(a => a.ConstructionStartingQuarter).IsRequired(false);
        builder.Property(a => a.ConstructionStartingYear).IsRequired(false);
        builder.Property(a => a.ConstructionCompletionQuarter).IsRequired(false);
        builder.Property(a => a.ConstructionCompletionYear).IsRequired(false);

        builder.Property(a => a.TenderingStage)
            .IsRequired(false)
            .HasConversion<string>();

        builder.Property(a => a.DevelopmentType)
            .IsRequired(false)
            .HasConversion<string>();

        builder.Property(a => a.FundingModel)
            .IsRequired(false)
            .HasConversion<string>();

        builder.Property(a => a.ExpectedDebt).HasColumnType("decimal(18,2)").IsRequired(false);
        builder.Property(a => a.ExpectedEquity).HasColumnType("decimal(18,2)").IsRequired(false);
        builder.Property(a => a.IsRevenueGenerating).IsRequired(false);
        builder.Property(a => a.IRR).HasColumnType("decimal(18,2)").IsRequired(false);
        builder.Property(a => a.IsPifGuaranteesRequired).IsRequired(false);

        builder.Property(a => a.Status).IsRequired();
        builder.Property(a => a.SubmittedBy).HasMaxLength(255).IsRequired(false);
        builder.Property(a => a.SubmittedAt).IsRequired(false);

        builder.OwnsOne(a => a.RejectionReason, rr =>
        {
            rr.Property(r => r.Value)
                .HasColumnName("RejectionReason")
                .HasMaxLength(3000)
                .IsRequired(false);
        });

        builder.Property(a => a.RejectedBy).HasMaxLength(255).IsRequired(false);
        builder.Property(a => a.RejectedAt).IsRequired(false);
        builder.Property(a => a.ApprovedBy).HasMaxLength(255).IsRequired(false);
        builder.Property(a => a.ApprovedAt).IsRequired(false);
        builder.Property(a => a.CompanyId).IsRequired();
        builder.Property(a => a.CompanyName).HasMaxLength(500).IsRequired(false);

        builder.Property(a => a.CreatedBy).HasMaxLength(255).IsRequired(false);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedBy).HasMaxLength(255).IsRequired(false);
        builder.Property(a => a.UpdatedAt).IsRequired(false);

        builder.HasMany<AssetCapex>()
            .WithOne()
            .HasForeignKey(c => c.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<AssetOpex>()
            .WithOne()
            .HasForeignKey(o => o.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<AssetHistory>()
            .WithOne()
            .HasForeignKey(h => h.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<AssetAttachment>()
            .WithOne()
            .HasForeignKey(a => a.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore calculated properties and domain events
        builder.Ignore(a => a.DomainEvents);
        builder.Ignore(a => a.TotalCapacity);
        builder.Ignore(a => a.TotalCapex);
        builder.Ignore(a => a.TotalOpex);
    }
}
