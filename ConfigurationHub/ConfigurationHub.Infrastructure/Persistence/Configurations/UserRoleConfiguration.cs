using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(ur => new { ur.UserId, ur.RoleId, ur.ModuleId });

        builder.Property(ur => ur.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(ur => ur.UserEmail)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(ur => ur.UserName)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(ur => ur.AssignedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(ur => ur.AssignedAt)
            .IsRequired();

        builder.HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ur => ur.Module)
            .WithMany()
            .HasForeignKey(ur => ur.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
