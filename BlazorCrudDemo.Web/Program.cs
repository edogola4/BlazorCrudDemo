using BlazorCrudDemo.Web.Middleware;
using Blazored.LocalStorage;

using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Interfaces;
using BlazorCrudDemo.Data.Repositories;
using BlazorCrudDemo.Data.Contexts.Interceptors;
using BlazorCrudDemo.Data.UnitOfWork;
using BlazorCrudDemo.Web.Services;
using BlazorCrudDemo.Web.Hubs;
using BlazorCrudDemo.Web.BackgroundServices;
using BlazorCrudDemo.Web.Mapping;
using BlazorCrudDemo.Web.Configuration;
using BlazorCrudDemo.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Blazored.Toast;
using Blazored.Modal;
using AutoMapper;
using BlazorCrudDemo.Shared.DTOs;
using FluentValidation;
using System.Reflection;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Cors.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .CreateLogger();

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set to true in production

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
builder.Services.AddIdentityServer(options =>
{
    options.IssuerUri = "https://localhost:5001";
    options.Authentication.CookieLifetime = TimeSpan.FromHours(2);
    options.Authentication.CookieSlidingExpiration = true;

    // In production, configure proper certificate
    options.KeyManagement.Enabled = false;
})
.AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
.AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
.AddInMemoryClients(IdentityServerConfig.GetClients())
.AddProfileService<ProfileService>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "https://localhost:5001";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "https://localhost:5001";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
    options.DefaultChallengeScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(5) // 5 minute tolerance
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.Principal);

            if (user == null || !user.IsActive)
            {
                context.Fail("User is inactive or not found");
                return;
            }

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        }
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Admin policy - requires Admin role
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // User policy - requires User or Admin role
    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole("User", "Admin"));

    // Product management policy
    options.AddPolicy("CanManageProducts", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("permission", "products.manage"));

    // Category management policy
    options.AddPolicy("CanManageCategories", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("permission", "categories.manage"));

    // User management policy
    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("permission", "users.manage"));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });

    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("https://localhost:5001", "https://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Register interceptors as singletons
builder.Services.AddSingleton<AuditInterceptor>();
builder.Services.AddSingleton<SoftDeleteInterceptor>();

// Add Entity Framework with interceptors
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
    var softDeleteInterceptor = serviceProvider.GetRequiredService<SoftDeleteInterceptor>();

    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(auditInterceptor, softDeleteInterceptor);
});

// Register repositories with logging
builder.Services.AddScoped<IProductRepository>(serviceProvider =>
{
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = serviceProvider.GetRequiredService<ILogger<ProductRepository>>();
    return new ProductRepository(context, logger);
});

builder.Services.AddScoped<ICategoryRepository>(serviceProvider =>
{
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = serviceProvider.GetRequiredService<ILogger<CategoryRepository>>();
    return new CategoryRepository(context, logger);
});

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register AutoMapper
var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});
var mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

// Register FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStateContainer, StateContainer>();
builder.Services.AddScoped<IBusinessValidationService, BusinessValidationService>();
builder.Services.AddScoped<ErrorNotificationService>();
builder.Services.AddScoped<ErrorRecoveryService>();
builder.Services.AddScoped<NetworkStatusService>();
builder.Services.AddScoped<OfflineModeService>();
builder.Services.AddScoped<ErrorRecoveryGuidanceService>();

// Register authentication services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Register background services
builder.Services.AddHostedService<MaintenanceBackgroundService>();
builder.Services.AddHostedService<CacheCleanupBackgroundService>();
builder.Services.AddHostedService<DataSyncBackgroundService>();

// Register SignalR
builder.Services.AddSignalR();

// Add Blazored services
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredLocalStorage();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Add CORS middleware
app.UseCors("AllowSpecificOrigins");

// Add security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "frame-ancestors 'self';");

    await next();
});

// Add request/response logging middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Add global exception handler middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseStaticFiles();
app.UseRouting();

// Configure IdentityServer middleware
app.UseIdentityServer();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Ensure database is migrated and created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Apply any pending migrations
    dbContext.Database.Migrate();

    // If no migrations exist, ensure the database is created with seed data
    if (!dbContext.Database.GetAppliedMigrations().Any())
    {
        dbContext.Database.EnsureCreated();
    }
}

app.Run();
