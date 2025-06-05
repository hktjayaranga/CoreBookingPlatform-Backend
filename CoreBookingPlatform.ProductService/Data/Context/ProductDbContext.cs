using CoreBookingPlatform.ProductService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreBookingPlatform.ProductService.Data.Context
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        
        public DbSet<ProductContent> ProductContent { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<Category>()
                .HasKey(c => c.CategoryId);
            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryId)
                .ValueGeneratedOnAdd(); 

       
            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductId);
            modelBuilder.Entity<Product>()
                .Property(p => p.ProductId)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<Product>()
                .Property(p => p.BasePrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => pc.ProductCategoryId);
            modelBuilder.Entity<ProductCategory>()
                .Property(pc => pc.ProductCategoryId)
                .ValueGeneratedOnAdd(); 
            modelBuilder.Entity<ProductCategory>()
                .HasIndex(pc => new { pc.ProductId, pc.CategoryId })
                .IsUnique(); 
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

           
            modelBuilder.Entity<ProductAttribute>()
                .HasKey(pa => pa.ProductAttributeId);
            modelBuilder.Entity<ProductAttribute>()
                .Property(pa => pa.ProductAttributeId)
                .ValueGeneratedOnAdd(); 
            modelBuilder.Entity<ProductAttribute>()
                .HasOne(pa => pa.Product)
                .WithMany(p => p.Attributes)
                .HasForeignKey(pa => pa.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

     
            modelBuilder.Entity<ProductContent>()
                .HasKey(pc => pc.ProductContentId);
            modelBuilder.Entity<ProductContent>()
                .Property(pc => pc.ProductContentId)
                .ValueGeneratedOnAdd(); 
            modelBuilder.Entity<ProductContent>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.Contents)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                 .HasIndex(p => new { p.ExternalId, p.ExternalSystemName })
                 .IsUnique();

            

        }


    }
}
