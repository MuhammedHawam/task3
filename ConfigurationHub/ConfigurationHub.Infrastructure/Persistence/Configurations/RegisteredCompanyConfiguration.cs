using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace YourNamespace.Data.Configurations
{
    public class RegisteredCompanyConfiguration : AuditableEntityConfiguration<RegisteredCompany>
    {
        public override void Configure(EntityTypeBuilder<RegisteredCompany> builder)
        {
            builder.ToTable("RegisteredCompanies");
            builder.Property(rc => rc.CompanyId)
                   .IsRequired()
                   .HasMaxLength(36);            

            builder.HasIndex(rc => rc.CompanyId)
                   .IsUnique()
                   .HasDatabaseName("IX_RegisteredCompany_CompanyId_Unique");

            builder.Property(rc => rc.Name)
                   .IsRequired()
                   .HasMaxLength(256);               


            builder.HasIndex(rc => rc.Name)
                   .HasDatabaseName("IX_RegisteredCompany_Name");

            builder.Property(rc => rc.ModuleId)
                   .IsRequired();

            builder.HasOne(rc => rc.Module)               
                   .WithMany(m => m.RegisteredCompanies) 
                   .HasForeignKey(rc => rc.ModuleId)
                   .OnDelete(DeleteBehavior.Restrict)  
                   .HasConstraintName("FK_RegisteredCompany_Module");

            builder.Property(rc => rc.SectorId)
                   .HasMaxLength(64);

            builder.Property(rc => rc.SectorName)
                 .HasMaxLength(250);

            builder.Property(rc => rc.Description)
                 .HasMaxLength(500);
        }
    }
}
