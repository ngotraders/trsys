using Microsoft.EntityFrameworkCore;
using System;
using Trsys.Web.Models.Events;
using Trsys.Web.Models.Orders;

namespace Trsys.Web.Data
{
    public class TrsysContext : DbContext
    {
        public TrsysContext(DbContextOptions<TrsysContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Event>()
                .Property(e => e.Timestamp)
                .HasConversion(e => e.UtcDateTime, e => new DateTimeOffset(e, TimeSpan.Zero));
            modelBuilder.Entity<Event>()
                .HasIndex(e => e.Timestamp);
        }
    }
}