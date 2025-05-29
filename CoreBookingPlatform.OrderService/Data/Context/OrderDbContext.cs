using CoreBookingPlatform.OrderService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBookingPlatform.OrderService.Data.Context
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Order> Orders {  get; set; }
        public DbSet<OrderItem> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasKey(o => o.OrderId);
            modelBuilder.Entity<OrderItem>().HasKey(oi => oi.OrderItemId);
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);
        }
    }
}
