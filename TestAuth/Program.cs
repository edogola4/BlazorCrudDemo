using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Models;
using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Web.Services;
using System.Text.Json;

namespace TestAuth
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing BlazorCrudDemo Authentication...");

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Setup DI
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuditService, AuditService>();

            // Add required services for AuthenticationService
            services.AddHttpContextAccessor();
            services.AddLogging();

            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

                // Test login with admin credentials
                var loginDto = new LoginDto
                {
                    Email = "admin@blazorcrud.com",
                    Password = "Admin123!",
                    RememberMe = false
                };

                Console.WriteLine($"Attempting login with: {loginDto.Email}");

                var result = await authService.LoginAsync(loginDto);

                Console.WriteLine($"Login result: Success={result.Success}");
                Console.WriteLine($"Message: {result.Message}");

                if (result.Errors?.Any() == true)
                {
                    Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");
                }

                if (result.Success)
                {
                    Console.WriteLine("✅ Authentication test PASSED!");
                    Console.WriteLine($"User: {result.User?.FirstName} {result.User?.LastName}");
                    Console.WriteLine($"Token expires: {result.ExpiresAt}");
                }
                else
                {
                    Console.WriteLine("❌ Authentication test FAILED!");
                }
            }
        }
    }
}
