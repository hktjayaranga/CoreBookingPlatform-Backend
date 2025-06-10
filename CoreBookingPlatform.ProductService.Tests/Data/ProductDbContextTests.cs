using CoreBookingPlatform.ProductService.Data.Context;
using CoreBookingPlatform.ProductService.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoreBookingPlatform.ProductService.Tests.Data.Context
{
    public class ProductDbContextTests
    {
        private DbContextOptions<ProductDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void CanInstantiateDbContext()
        {
            var options = CreateOptions();
            using var context = new ProductDbContext(options);
            Assert.NotNull(context);
        }

        [Fact]
        public void DbSets_AreAccessible()
        {
            var options = CreateOptions();
            using var context = new ProductDbContext(options);

            Assert.NotNull(context.Products);
            Assert.NotNull(context.Categories);
            Assert.NotNull(context.ProductCategories);
            Assert.NotNull(context.ProductAttributes);
            Assert.NotNull(context.ProductContent);
        }

        [Fact]
        public void OnModelCreating_ConfiguresCategoryEntity()
        {
            var options = CreateOptions();
            using var context = new ProductDbContext(options);

            var entity = context.Model.FindEntityType(typeof(Category));
            Assert.NotNull(entity);
            Assert.True(entity.FindPrimaryKey().Properties.Any(p => p.Name == "CategoryId"));
        }

        [Fact]
        public void OnModelCreating_ConfiguresProductEntity()
        {
            var options = CreateOptions();
            using var context = new ProductDbContext(options);

            var entity = context.Model.FindEntityType(typeof(Product));
            Assert.NotNull(entity);
            Assert.True(entity.FindPrimaryKey().Properties.Any(p => p.Name == "ProductId"));
            Assert.Contains(entity.GetIndexes(), idx => idx.Properties.Any(p => p.Name == "ExternalId" || p.Name == "ExternalSystemName"));
        }

        [Fact]
        public void OnModelCreating_ConfiguresProductCategoryEntity()
        {
            var options = CreateOptions();
            using var context = new ProductDbContext(options);

            var entity = context.Model.FindEntityType(typeof(ProductCategory));
            Assert.NotNull(entity);
            Assert.True(entity.FindPrimaryKey().Properties.Any(p => p.Name == "ProductCategoryId"));
            Assert.Contains(entity.GetIndexes(), idx => idx.IsUnique && idx.Properties.Any(p => p.Name == "ProductId" || p.Name == "CategoryId"));
        }

        [Fact]
        public void OnModelCreating_ConfiguresProductAttributeEntity()
        {
            var options = CreateOptions();
            using var context = new ProductDbContext(options);

            var entity = context.Model.FindEntityType(typeof(ProductAttribute));
            Assert.NotNull(entity);
            Assert.True(entity.FindPrimaryKey().Properties.Any(p => p.Name == "ProductAttributeId"));
        }

        [Fact]
        public void OnModelCreating_ConfiguresProductContentEntity()
        {
            var options = CreateOptions();
            using var context = new ProductDbContext(options);

            var entity = context.Model.FindEntityType(typeof(ProductContent));
            Assert.NotNull(entity);
            Assert.True(entity.FindPrimaryKey().Properties.Any(p => p.Name == "ProductContentId"));
        }
    }
}