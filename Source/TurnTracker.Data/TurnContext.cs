using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TurnTracker.Data.Entities;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace TurnTracker.Data
{
    public class TurnContext : DbContext
    {
        public TurnContext(DbContextOptions<TurnContext> options) : base(options)
        {
        }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Turn> Turns { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<NotificationSetting> NotificationSettings { get; set; }
        public DbSet<PushSubscriptionDevice> PushSubscriptionDevices { get; set; }
        public DbSet<DeviceAuthorization> DeviceAuthorizations { get; set; }
        public DbSet<Login> Logins { get; set; }

        private void UpdateDates()
        {
            var now = DateTimeOffset.Now;
            foreach (var entry in ChangeTracker.Entries()
                .Where(entry => entry.Entity is Entity && (entry.State == EntityState.Added || entry.State == EntityState.Modified)))
            {
                var entity = (Entity)entry.Entity;
                entity.ModifiedDate = now;
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedDate = now;
                }
            }
        }

        public override int SaveChanges()
        {
            UpdateDates();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateDates();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            UpdateDates();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            UpdateDates();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Participant>()
                .HasOne(participant => participant.User)
                .WithMany(user => user.Participants)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Participant>()
                .HasMany(participant => participant.NotificationSettings)
                .WithOne(notificationSetting => notificationSetting.Participant)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Turn>()
                .HasOne(turn => turn.User)
                .WithMany(user => user.TurnsTaken)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Turn>()
                .HasOne(turn => turn.Creator)
                .WithMany(user => user.TurnsCreated)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Activity>()
                .Property(x => x.PeriodUnit)
                .HasConversion<string>();

            modelBuilder.Entity<Activity>()
                .Property(x => x.Period)
                .HasConversion(
                    v => v.Value.Ticks,
                    t => TimeSpan.FromTicks(t));

            modelBuilder.Entity<PushSubscriptionDevice>()
                .HasKey(x => new {x.UserId, x.Endpoint});

            modelBuilder.Entity<PushSubscriptionDevice>()
                .Property(x => x.Keys)
                .HasJsonConversion();

            modelBuilder.Entity<Login>()
                .HasOne(login => login.DeviceUsedForLogin)
                .WithMany(device => device.Logins)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
