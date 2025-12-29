using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates.Campaigns;
using PartnersHub.InnovationHub.Domain.Aggregates.ChallengeRequest;
using PartnersHub.InnovationHub.Domain.Enums;


namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations.CampaignConfiguration;

public class CampaignRequestConfiguration : AuditableEntityConfiguration<CampaignRequest>
{

    public override void Configure(EntityTypeBuilder<CampaignRequest> builder)
    {
        builder.ToTable("CampaignRequests");

        builder.Property(cr => cr.Name)
            .HasColumnName("Name")
            .IsRequired();

        builder.Property(cr => cr.Description)
            .HasColumnName("Description")
            .HasMaxLength(3000);

        builder.Property(cr => cr.ProblemStatement)
            .HasColumnName("ProblemStatement")
            .HasMaxLength(3000);

        builder.Property(cr => cr.CampaignRequestStatus)
            .HasColumnName("CampaignRequestStatus")
            .HasConversion(
                name => (int)name,
                value => (CampaignRequestStatus)value);

        builder.Property(cr => cr.Type)
            .HasColumnName("Type")
            .HasConversion(
                name => (int)name,
                value => (CampaignType)value);

        builder.Property(cr => cr.LaunchDate)
            .HasColumnName("LaunchDate")
            .HasColumnType("datetime");

        builder.Property(cr => cr.SubmissionDeadLine)
            .HasColumnName("SubmissionDeadLine")
            .HasColumnType("datetime");


        builder.Property(cr => cr.OwnerName)
            .HasColumnName("OwnerName")
            .IsRequired();

        builder.Property(cr => cr.Comments)
            .HasColumnName("Comments")
            .HasMaxLength(1000);

        builder.Property(cr => cr.RowVersion)
            .HasColumnName("RowVersion")
            .IsRowVersion();

        builder.Property(cr => cr.ShortId)
               .HasColumnName("ShortId").ValueGeneratedOnAdd();
        builder.Property(p => p.ShortId)
          .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasMany(cr => cr.Sponsors)
            .WithOne()
            .HasForeignKey(s => s.CampaignRequestId);

        builder.HasMany(cr => cr.Evaluators)
            .WithOne()
            .HasForeignKey(e => e.CampaignRequestId);

        builder.HasMany(cr => cr.EvaluationCriterias)
            .WithOne()
            .HasForeignKey(ec => ec.CampaignRequestId);

        builder.HasMany(cr => cr.LinkedChallenges)
            .WithOne()
            .HasForeignKey(lc => lc.CampaignRequestId);

        builder.HasMany(cr => cr.TermsAndCondition)
            .WithOne()
            .HasForeignKey(tac => tac.CampaignRequestId);

        builder.HasMany(cr => cr.TrackingHistory)
            .WithOne()
            .HasForeignKey(th => th.CampaignRequestId)
            .OnDelete(DeleteBehavior.Cascade); 
    }
}
