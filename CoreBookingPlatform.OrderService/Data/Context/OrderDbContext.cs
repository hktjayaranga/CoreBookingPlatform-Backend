using CoreBookingPlatform.OrderService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBookingPlatform.OrderService.Data.Context
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }
        public DbSet<Order> Orders {  get; set; }
        public DbSet<OrderItem> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasKey(o => o.OrderId);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>().HasKey(oi => oi.OrderItemId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}
