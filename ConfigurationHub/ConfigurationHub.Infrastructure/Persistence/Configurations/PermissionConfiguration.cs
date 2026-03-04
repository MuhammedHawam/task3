using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : EntityConfiguration<Permission>
{
    public override void Configure(EntityTypeBuilder<Permission> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(p => p.Description)
            .HasMaxLength(512);

        builder.HasIndex(p => p.Name).IsUnique();
    }
}