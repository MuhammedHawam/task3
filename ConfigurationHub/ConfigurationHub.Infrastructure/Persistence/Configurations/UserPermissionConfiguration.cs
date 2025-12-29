using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Presistence.EntityConfigurations
{
    public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
    {
        public void Configure(EntityTypeBuilder<UserPermission> builder)
        {
            builder.HasKey(rp => new { rp.UserId, rp.PermissionId, rp.ModuleId });

            builder.Property(rp => rp.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(rp => rp.Module)
                .WithMany()
                .HasForeignKey(rp => rp.ModuleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
