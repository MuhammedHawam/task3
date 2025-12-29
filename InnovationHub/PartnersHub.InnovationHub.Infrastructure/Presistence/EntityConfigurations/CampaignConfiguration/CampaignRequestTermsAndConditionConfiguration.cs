using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations.CampaignConfiguration;


public class CampaignRequestTermsAndConditionConfiguration : EntityConfiguration<CampaignRequestTermsAndCondition>
{
    public override void Configure(EntityTypeBuilder<CampaignRequestTermsAndCondition> builder)
    {
        base.Configure(builder);

        builder.ToTable("CampaignRequestTermsAndConditions");
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(a => a.CampaignRequestId)
            .IsRequired();

        // Configure Attachment as owned type (value object)
        builder.OwnsOne(a => a.Metadata, metadata => {
            metadata.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("FileName");

            metadata.Property(m => m.Extension)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("FileExtension");

            metadata.Property(m => m.SizeInBytes)
                .IsRequired()
                .HasColumnName("FileSizeInBytes");

            metadata.Property(m => m.Format)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("FileFormat");
        });

        // SharePoint properties
        builder.Property(a => a.SharePointFileId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.SharePointUrl)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.SharePointLibrary)
            .IsRequired()
            .HasMaxLength(200);

        // Audit properties
        builder.Property(a => a.UploadedBy)
            .IsRequired();

        builder.Property(a => a.UploadedAt)
            .IsRequired();

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(a => a.DeletedBy);

        builder.Property(a => a.DeletedAt);

        // Indexes for performance
        builder.HasIndex(a => a.CampaignRequestId)
            .HasDatabaseName("IX_CampaignRequestCampaignRequestTermsAndConditions_CampaignRequestId");

        builder.HasIndex(a => a.UploadedAt)
            .HasDatabaseName("IX_CampaignRequestCampaignRequestTermsAndConditions_UploadedAt");

        builder.HasIndex(a => a.IsDeleted)
            .HasDatabaseName("IX_CampaignRequestCampaignRequestTermsAndConditions_IsDeleted");

        builder.HasIndex(a => new { a.CampaignRequestId, a.IsDeleted })
            .HasDatabaseName("IX_CampaignRequestCampaignRequestTermsAndConditions_CampaignRequestId_IsDeleted");


    }
}

