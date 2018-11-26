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
            modelBuilder.Entity<Setting>().HasData(
                new {Key = "registration.open", Name = "Registration Open", Type = "bool", BoolValue = true});
        }
    }
}
