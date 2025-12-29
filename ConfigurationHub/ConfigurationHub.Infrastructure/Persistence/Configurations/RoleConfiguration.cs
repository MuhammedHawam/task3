using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.IsActive)
            .IsRequired();

        builder.Property(r => r.IsSystemRole)
            .IsRequired();

        builder.HasOne(r => r.Module)
            .WithMany()
            .HasForeignKey(r => r.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.Name).IsUnique();
    }
}
