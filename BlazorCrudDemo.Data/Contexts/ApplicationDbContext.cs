using Microsoft.EntityFrameworkCore;
using BlazorCrudDemo.Shared.Models;
using BlazorCrudDemo.Data.Configurations;
using BlazorCrudDemo.Data.Contexts.Interceptors;

namespace BlazorCrudDemo.Data.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        private readonly AuditInterceptor _auditInterceptor;
        private readonly SoftDeleteInterceptor _softDeleteInterceptor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            AuditInterceptor auditInterceptor,
            SoftDeleteInterceptor softDeleteInterceptor)
            : base(options)
        {
            _auditInterceptor = auditInterceptor;
            _softDeleteInterceptor = softDeleteInterceptor;
        }

        public DbSet<Product> Products { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Add interceptors for audit fields and soft delete
            optionsBuilder.AddInterceptors(_auditInterceptor, _softDeleteInterceptor);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull); // Set to null instead of cascade delete

            // Global query filter for soft delete (only active records by default)
            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => p.IsActive);

            modelBuilder.Entity<Category>()
                .HasQueryFilter(c => c.IsActive);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed categories
            var categories = new[]
            {
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Electronic devices and accessories",
                    Icon = "fas fa-laptop",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                },
                new Category
                {
                    Id = 2,
                    Name = "Books",
                    Description = "Books and educational materials",
                    Icon = "fas fa-book",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                },
                new Category
                {
                    Id = 3,
                    Name = "Clothing",
                    Description = "Fashion and apparel items",
                    Icon = "fas fa-tshirt",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                }
            };

            modelBuilder.Entity<Category>().HasData(categories);

            // Seed products
            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Name = "MacBook Pro 16-inch",
                    Description = "High-performance laptop for professionals",
                    Price = 2399.99m,
                    Stock = 15,
                    SKU = "MBP16-001",
                    ImageUrl = "https://example.com/macbook-pro.jpg",
                    CategoryId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                },
                new Product
                {
                    Id = 2,
                    Name = "iPhone 15 Pro",
                    Description = "Latest smartphone with advanced camera system",
                    Price = 999.99m,
                    Stock = 25,
                    SKU = "IPH15P-001",
                    ImageUrl = "https://example.com/iphone-15-pro.jpg",
                    CategoryId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                },
                new Product
                {
                    Id = 3,
                    Name = "Clean Code",
                    Description = "A Handbook of Agile Software Craftsmanship",
                    Price = 39.99m,
                    Stock = 50,
                    SKU = "CC-BOOK-001",
                    ImageUrl = "https://example.com/clean-code.jpg",
                    CategoryId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                },
                new Product
                {
                    Id = 4,
                    Name = "Design Patterns",
                    Description = "Elements of Reusable Object-Oriented Software",
                    Price = 49.99m,
                    Stock = 30,
                    SKU = "DP-BOOK-001",
                    ImageUrl = "https://example.com/design-patterns.jpg",
                    CategoryId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                },
                new Product
                {
                    Id = 5,
                    Name = "Cotton T-Shirt",
                    Description = "Comfortable 100% cotton t-shirt",
                    Price = 19.99m,
                    Stock = 100,
                    SKU = "TSHIRT-COT-001",
                    ImageUrl = "https://example.com/cotton-tshirt.jpg",
                    CategoryId = 3,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                }
            };

            modelBuilder.Entity<Product>().HasData(products);
        }
    }
}
