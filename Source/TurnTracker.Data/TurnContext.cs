using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TurnTracker.Data.Entities;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Participant>()
                .HasOne(participant => participant.User)
                .WithMany(user => user.Participants)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Turn>()
                .HasOne(turn => turn.User)
                .WithMany(user => user.TurnsTaken)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Turn>()
                .HasOne(turn => turn.Creator)
                .WithMany(user => user.TurnsCreated)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
