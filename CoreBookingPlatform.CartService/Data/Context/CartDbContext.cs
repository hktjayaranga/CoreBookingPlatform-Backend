using CoreBookingPlatform.CartService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBookingPlatform.CartService.Data.Context
{
    public class CartDbContext : DbContext
    {
        public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
        {
        }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cart>()
                .HasKey(c => c.CartId);
            modelBuilder.Entity<Cart>()
                .Property(c => c.CartId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)         
                .WithMany(u => u.Carts)       
                .HasForeignKey(c => c.UserId) 
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasKey(ci => ci.CartItemId);
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.CartItemId)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>().HasData(
                new User { UserId = "abc", Name = "Test User 1"},
                new User { UserId = "cde", Name = "Test User 2" }
                );

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}
