using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Presistence.EntityConfigurations
{
    public class PermissionConfiguration : EntityConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(128); 

            builder.Property(p => p.Description)
                .HasMaxLength(512);

            builder.HasIndex(p => p.Name).IsUnique();
        }
    }
}