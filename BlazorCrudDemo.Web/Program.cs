using BlazorCrudDemo.Web.Middleware;
using Blazored.LocalStorage;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BlazorCrudDemo.Data.Seeders;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

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
builder.Services.AddControllers();

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

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
});
// builder.Services.AddIdentityServer(options =>
// {
//     options.IssuerUri = "https://localhost:5001";
//     options.Authentication.CookieLifetime = TimeSpan.FromHours(2);
//     options.Authentication.CookieSlidingExpiration = true;

//     // In production, configure proper certificate
//     options.KeyManagement.Enabled = false;
// })
// .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources())
// .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
// .AddInMemoryClients(IdentityServerConfig.GetClients())
// .AddProfileService<ProfileService>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "https://localhost:5120";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "https://localhost:5120";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Authority = jwtIssuer;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        NameClaimType = "name",
        RoleClaimType = "role",
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<ApplicationUser>>();
            var signInManager = context.HttpContext.RequestServices.GetService<SignInManager<ApplicationUser>>();

            if (userManager == null || signInManager == null) return;

            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userId)) return;

            var user = await userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive) return;

            user.LastLoginDate = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", $"{user.FirstName} {user.LastName}".Trim()),
                new Claim("first_name", user.FirstName ?? ""),
                new Claim("last_name", user.LastName ?? "")
            };

            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var appIdentity = new ClaimsIdentity(claims, "Identity.Application");
            context.Principal = new ClaimsPrincipal(appIdentity);
        },

        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Configure Identity to use the same JWT claims
// builder.Services.Configure<IdentityOptions>(options =>
// {
//     options.ClaimsIdentity.UserIdClaimType = JwtRegisteredClaimNames.Sub;
//     options.ClaimsIdentity.UserNameClaimType = JwtRegisteredClaimNames.Email;
//     options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
//     options.ClaimsIdentity.EmailClaimType = JwtRegisteredClaimNames.Email;
// });

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
builder.Services.AddScoped<BlazorCrudDemo.Web.Services.IAuthenticationService, BlazorCrudDemo.Web.Services.AuthenticationService>();
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
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors 'self';");

    await next();
});

// Seed admin user after database is ready but before starting the server
using (var scope = app.Services.CreateScope())
{
    try
    {
        await AdminUserSeeder.SeedAdminUser(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the admin user.");
    }
}

// Add request/response logging middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Add global exception handler middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseStaticFiles();
app.UseRouting();

// Configure IdentityServer middleware
// app.UseIdentityServer();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllers();
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
