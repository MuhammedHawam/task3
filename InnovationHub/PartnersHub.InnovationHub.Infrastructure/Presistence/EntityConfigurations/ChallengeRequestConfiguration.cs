using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations;


public class ChallengeRequestConfiguration : AuditableEntityConfiguration<ChallengeRequest> 
{
    public override void Configure(EntityTypeBuilder<ChallengeRequest> builder)
    {

        builder.ToTable("ChallengeRequests");

        builder.Property(cr => cr.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(cr => cr.Name)
            .HasColumnName("Name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(cr => cr.Description)
            .HasColumnName("Description")
            .HasMaxLength(3000)
            .IsRequired();

        builder.Property(cr => cr.SourceCompanyId)
            .HasColumnName("SourceCompanyId")
            .IsRequired();

        builder.Property(cr => cr.AssociatedSectorId)
            .HasColumnName("AssociatedSectorId")
            .IsRequired();

        builder.Property(cr => cr.SubmitterName)
            .HasColumnName("SubmitterName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(cr => cr.PriorityLevelId)
            .HasColumnName("PriorityLevelId")
            .IsRequired();

        builder.Property(cr => cr.ChallengeStatus)
            .HasColumnName("ChallengeStatus")
            .HasConversion(
                name => (int)name,
                value => (ChallengeStatus)value)
            .IsRequired();


        builder.Property(cr => cr.ShortId)
               .HasColumnName("ShortId").ValueGeneratedOnAdd();
        builder.Property(p => p.ShortId)
          .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);




        builder.HasOne(cr => cr.SourceCompany)
           .WithMany()
           .HasForeignKey(cr => cr.SourceCompanyId);

        

            builder.HasOne(cr => cr.AssociatedSector)
           .WithMany()
           .HasForeignKey(cr => cr.AssociatedSectorId);

        builder.HasMany(cr => cr.TrackingHistory)
            .WithOne()
            .HasForeignKey(cth => cth.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cr => cr.Technologies)
        .WithOne()
        .HasForeignKey(cth => cth.ChallengeRequestId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}

