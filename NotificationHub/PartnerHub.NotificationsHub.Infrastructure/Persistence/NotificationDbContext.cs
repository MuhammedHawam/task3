using Microsoft.EntityFrameworkCore;
using PartnerHub.NotificationsHub.Domain.Entities;

namespace PartnerHub.NotificationsHub.Infrastructure.Persistence;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<NotificationEntity> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Channel).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
        });
    }
}