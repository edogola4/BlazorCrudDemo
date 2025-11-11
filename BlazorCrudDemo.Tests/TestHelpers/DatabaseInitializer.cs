using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace BlazorCrudDemo.Tests.TestHelpers
{
    public static class DatabaseInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Add test users
            var hasher = new PasswordHasher<ApplicationUser>();
            
            var adminUser = new ApplicationUser
            {
                Id = "1",
                UserName = "admin@test.com",
                NormalizedUserName = "ADMIN@TEST.COM",
                Email = "admin@test.com",
                NormalizedEmail = "ADMIN@TEST.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                FirstName = "Admin",
                LastName = "User",
                IsActive = true
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

            var regularUser = new ApplicationUser
            {
                Id = "2",
                UserName = "user@test.com",
                NormalizedUserName = "USER@TEST.COM",
                Email = "user@test.com",
                NormalizedEmail = "USER@TEST.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                FirstName = "Regular",
                LastName = "User",
                IsActive = true
            };
            regularUser.PasswordHash = hasher.HashPassword(regularUser, "User123!");

            // Add roles
            var adminRole = new IdentityRole
            {
                Id = "1",
                Name = "Admin",
                NormalizedName = "ADMIN"
            };

            var userRole = new IdentityRole
            {
                Id = "2",
                Name = "User",
                NormalizedName = "USER"
            };

            // Add user roles
            var adminUserRole = new IdentityUserRole<string>
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            };

            var regularUserRole = new IdentityUserRole<string>
            {
                UserId = regularUser.Id,
                RoleId = userRole.Id
            };

            // Add test categories
            var category1 = new Category
            {
                Id = 1,
                Name = "Electronics",
                Description = "Electronic items",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var category2 = new Category
            {
                Id = 2,
                Name = "Books",
                Description = "Books and publications",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add test products
            var product1 = new Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "High performance laptop",
                Price = 999.99m,
                CategoryId = 1,
                StockQuantity = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var product2 = new Product
            {
                Id = 2,
                Name = "Smartphone",
                Description = "Latest smartphone model",
                Price = 699.99m,
                CategoryId = 1,
                StockQuantity = 15,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add all to context
            context.Users.Add(adminUser);
            context.Users.Add(regularUser);
            context.Roles.Add(adminRole);
            context.Roles.Add(userRole);
            context.UserRoles.Add(adminUserRole);
            context.UserRoles.Add(regularUserRole);
            context.Categories.Add(category1);
            context.Categories.Add(category2);
            context.Products.Add(product1);
            context.Products.Add(product2);

            context.SaveChanges();
        }
    }
}
