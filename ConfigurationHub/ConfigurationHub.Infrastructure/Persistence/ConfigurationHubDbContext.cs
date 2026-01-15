using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PartnersHub.ConfigurationHub.Domain.Aggregates.Configuration;
using PartnersHub.ConfigurationHub.Domain.Aggregates.Lookups;
using PartnersHub.ConfigurationHub.Domain.Common;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using System.Reflection;
using DomainModule = PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission.Module;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence;

public class ConfigurationHubDbContext : DbContext {
    public ConfigurationHubDbContext(DbContextOptions<ConfigurationHubDbContext> options)
        : base(options) {
    }

    public DbSet<WhiteListIP> WhiteListIPs => Set<WhiteListIP>();
    public DbSet<TermsAndCondition> TermsAndConditions => Set<TermsAndCondition>();

    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<SubSector> SubSectors => Set<SubSector>();
    public DbSet<AssetType> AssetTypes => Set<AssetType>();
    public DbSet<UnitOfMeasurement> UnitsOfMeasurement => Set<UnitOfMeasurement>();

    public DbSet<DomainModule> Modules => Set<DomainModule>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        var result = await base.SaveChangesAsync(cancellationToken);

        var entitiesWithEvents = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        foreach (var entity in entitiesWithEvents) {
            entity.ClearDomainEvents();
        }

        return result;
    }
}