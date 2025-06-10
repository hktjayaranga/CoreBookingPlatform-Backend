using CoreBookingPlatform.CartService.Data.Context;
using CoreBookingPlatform.CartService.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace CoreBookingPlatform.CartService.Tests.Data.Context
{
    public class CartDbContextTests
    {
        private DbContextOptions<CartDbContext> CreateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<CartDbContext>()
                .UseInMemoryDatabase(databaseName: "CartDbContextTestDb")
                .Options;
        }

        [Fact]
        public void CanConstructCartDbContext()
        {
            var options = CreateInMemoryOptions();
            using var context = new CartDbContext(options);
            Assert.NotNull(context);
        }

        [Fact]
        public void OnModelCreating_ConfiguresEntitiesAndSeedData()
        {
            var options = CreateInMemoryOptions();
            using (var context = new CartDbContext(options))
            {
                // Ensure database is created and seed data is applied
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Check User seed data
                var users = context.Users.ToList();
                Assert.Contains(users, u => u.UserId == "abc" && u.Name == "Test User 1");
                Assert.Contains(users, u => u.UserId == "cde" && u.Name == "Test User 2");
            }
        }

        [Fact]
        public void CanAddAndRetrieveCartWithItems()
        {
            var options = CreateInMemoryOptions();
            using (var context = new CartDbContext(options))
            {
                var user = new User { UserId = "xyz", Name = "Test User" };
                context.Users.Add(user);
                context.SaveChanges();

                var cart = new Cart { UserId = user.UserId };
                context.Carts.Add(cart);
                context.SaveChanges();

                var item = new CartItem { CartId = cart.CartId, ProductId = 1, Quantity = 2, Price = 9.99m };
                context.CartItems.Add(item);
                context.SaveChanges();

                var loadedCart = context.Carts.Include(c => c.Items).FirstOrDefault(c => c.CartId == cart.CartId);
                Assert.NotNull(loadedCart);
                Assert.Single(loadedCart.Items);
                Assert.Equal(9.99m, loadedCart.Items[0].Price);
            }
        }
    }
}