using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InfraBase.Domain.Aggregates.AssetAggregate;

namespace PartnersHub.InfraBase.Infrastructure.Persistence.EntityConfigurations;

public class AssetAttachmentConfiguration : IEntityTypeConfiguration<AssetAttachment>
{
    public void Configure(EntityTypeBuilder<AssetAttachment> builder)
    {
        builder.ToTable("AssetAttachments");

        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.AssetId)
            .IsRequired();

        builder.OwnsOne(a => a.Metadata, m =>
        {
            m.Property(x => x.FileName)
                .HasColumnName("FileName")
                .HasMaxLength(500)
                .IsRequired();

            m.Property(x => x.FileSizeInBytes)
                .HasColumnName("FileSizeInBytes")
                .IsRequired();

            m.Property(x => x.ContentType)
                .HasColumnName("ContentType")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.Property(a => a.SharePointUrl)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(a => a.UploadedBy)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.UploadedAt)
            .IsRequired();

        builder.Property(a => a.IsDeleted)
            .IsRequired();

        builder.Property(a => a.DeletedBy)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(a => a.DeletedAt)
            .IsRequired(false);

        // Configure the relationship from the child side
        builder.HasOne<Asset>()
            .WithMany(a => a.Attachments)
            .HasForeignKey(att => att.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.AssetId, a.IsDeleted });
    }
}
