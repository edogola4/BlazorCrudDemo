using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorCrudDemo.Data.Seeders
{
    public class SampleDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            
            var logger = services.GetRequiredService<ILogger<SampleDataSeeder>>();
            var dbContext = services.GetRequiredService<ApplicationDbContext>();

            try
            {
                logger.LogInformation("Starting to seed sample data...");
                
                // Check if we already have categories
                if (await dbContext.Categories.AnyAsync())
                {
                    logger.LogInformation("Database already contains data. Seeding skipped.");
                    return;
                }

                // Add sample categories
                var categories = new List<Category>
                {
                    new() { Name = "Electronics", Description = "Electronic devices and accessories" },
                    new() { Name = "Clothing", Description = "Apparel and fashion items" },
                    new() { Name = "Books", Description = "Books and reading materials" },
                    new() { Name = "Home & Kitchen", Description = "Home appliances and kitchenware" },
                    new() { Name = "Sports & Outdoors", Description = "Sports equipment and outdoor gear" }
                };

                await dbContext.Categories.AddRangeAsync(categories);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Added {Count} categories", categories.Count);

                // Add sample products
                var random = new Random();
                var products = new List<Product>();
                
                // Electronics
                products.Add(new Product
                {
                    Name = "Wireless Earbuds",
                    Description = "High-quality wireless earbuds with noise cancellation",
                    Price = 129.99m,
                    Stock = 50,
                    SKU = "ELEC-001",
                    CategoryId = categories[0].Id
                });

                products.Add(new Product
                {
                    Name = "Smart Watch",
                    Description = "Feature-rich smartwatch with health monitoring",
                    Price = 249.99m,
                    Stock = 30,
                    SKU = "ELEC-002",
                    CategoryId = categories[0].Id
                });

                // Clothing
                products.Add(new Product
                {
                    Name = "Cotton T-Shirt",
                    Description = "Comfortable 100% cotton t-shirt",
                    Price = 24.99m,
                    Stock = 100,
                    SKU = "CLOTH-001",
                    CategoryId = categories[1].Id
                });

                // Books
                products.Add(new Product
                {
                    Name = "The Great Novel",
                    Description = "Bestselling fiction novel",
                    Price = 19.99m,
                    Stock = 75,
                    SKU = "BOOK-001",
                    CategoryId = categories[2].Id
                });

                // Home & Kitchen
                products.Add(new Product
                {
                    Name = "Air Fryer",
                    Description = "Digital air fryer with multiple cooking functions",
                    Price = 89.99m,
                    Stock = 40,
                    SKU = "HOME-001",
                    CategoryId = categories[3].Id
                });

                // Sports & Outdoors
                products.Add(new Product
                {
                    Name = "Yoga Mat",
                    Description = "Eco-friendly non-slip yoga mat",
                    Price = 34.99m,
                    Stock = 60,
                    SKU = "SPORT-001",
                    CategoryId = categories[4].Id
                });

                await dbContext.Products.AddRangeAsync(products);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Added {Count} products", products.Count);
                
                logger.LogInformation("Sample data seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }
    }
}
