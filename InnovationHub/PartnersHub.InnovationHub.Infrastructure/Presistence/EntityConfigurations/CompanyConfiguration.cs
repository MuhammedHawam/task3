using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PartnersHub.InnovationHub.Domain.Aggregates.Lookups;
using PartnersHub.InnovationHub.Domain.ValueObjects;


namespace PartnersHub.InnovationHub.Infrastructure.Presistence.EntityConfigurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.HeadquarterCountry).IsRequired().HasMaxLength(100);
        builder.Property(c => c.HeadquarterCity).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Logo).HasColumnType("varbinary(max)");
        builder.Property(c => c.IsActive).HasDefaultValue(true);

        builder.OwnsOne(c => c.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("Name")
                .IsRequired()
                .HasMaxLength(CompanyName.MaxLength);
        });

        builder.OwnsOne(c => c.Description, description =>
        {
            description.Property(d => d.Value)
                .HasColumnName("Description")
                .HasMaxLength(Description.MaxLength);
        });

        builder.OwnsOne(c => c.RepresentativeInformation, rep =>
        {
            rep.Property(r => r.Name).HasColumnName("RepresentativeName").IsRequired().HasMaxLength(200);
            rep.Property(r => r.Position).HasColumnName("Position").IsRequired().HasMaxLength(100);
            rep.Property(r => r.Email).HasColumnName("Email").IsRequired().HasMaxLength(256);
            rep.Property(r => r.Phone).HasColumnName("Phone").IsRequired().HasMaxLength(20);
        });

        builder.HasMany(c => c.Sectors)
            .WithOne()
            .HasForeignKey(cs => cs.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Sectors)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
