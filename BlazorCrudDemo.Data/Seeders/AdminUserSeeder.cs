using Microsoft.AspNetCore.Identity;
using BlazorCrudDemo.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BlazorCrudDemo.Data.Seeders
{
    public static class AdminUserSeeder
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminUserSeeder");

            // Ensure the Admin role exists
            var adminRoleName = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRoleName));
                logger.LogInformation("Created Admin role");
            }

            // Check if admin user already exists
            var adminEmail = "admin@demo.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    LastLoginDate = DateTime.UtcNow
                };

                // Create the admin user with a secure password
                var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
                
                if (createResult.Succeeded)
                {
                    // Assign the Admin role to the user
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, adminRoleName);
                    
                    if (addToRoleResult.Succeeded)
                    {
                        logger.LogInformation("Created admin user successfully");
                    }
                    else
                    {
                        logger.LogError("Failed to add admin user to role");
                    }
                }
                else
                {
                    logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", createResult.Errors));
                }
            }
            else
            {
                // Ensure the admin user has the Admin role
                if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
                {
                    await userManager.AddToRoleAsync(adminUser, adminRoleName);
                    logger.LogInformation("Added Admin role to existing admin user");
                }
            }
        }
    }
}
