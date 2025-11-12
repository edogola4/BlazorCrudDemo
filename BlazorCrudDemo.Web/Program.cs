using System.Reflection;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using FluentValidation;
using AutoMapper;
using Blazored.LocalStorage;
using Blazored.Toast;
using Blazored.Modal;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Interfaces;
using BlazorCrudDemo.Data.Repositories;
using BlazorCrudDemo.Data.Contexts.Interceptors;
using BlazorCrudDemo.Data.UnitOfWork;
using BlazorCrudDemo.Data.Models;
using BlazorCrudDemo.Data.Seeders;
using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Web.Middleware;
using BlazorCrudDemo.Web.Services;
using BlazorCrudDemo.Web.Hubs;
using BlazorCrudDemo.Web.BackgroundServices;
using BlazorCrudDemo.Web.Mapping;
using BlazorCrudDemo.Web.Configuration;
using BlazorCrudDemo.Web.HealthChecks;

// Create the builder
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    
    // Add services to the container
    builder.Host.UseSerilog();
    
    // Configure Kestrel
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.AddServerHeader = false;
        serverOptions.ConfigureHttpsDefaults(httpsOptions =>
        {
            httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | 
                                      System.Security.Authentication.SslProtocols.Tls13;
        });
    });

    // Add CORS policy for WebSockets
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:5120", "https://localhost:5120")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials(); // This is important for WebSockets
        });
});

// Add SignalR services with enhanced configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromMinutes(1);
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
});

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("api-version");
})
.AddMvc()
.AddApiExplorer(options =>
{
    // Add the versioned API explorer, which also adds IApiVersionDescriptionProvider service
    // Note: the specified format code will format the version as "v{0}" (e.g., v1, v2)
    options.GroupNameFormat = "'v'VVV";
    
    // Note: this option is only necessary when versioning by URL segment. The SubstitutionFormat
    // can also be used to control the format of the API version in route templates
    options.SubstituteApiVersionInUrl = true;
});

// Add API Explorer for Swagger
try
{
    builder.Services.AddVersionedApiExplorer(options =>
    {
        // Add the versioned API explorer, which also adds IApiVersionDescriptionProvider service
        // Note: the specified format code will format the version as "'v'major[.minor][-status]"
        options.GroupNameFormat = "'v'VVV";
        
        // Note: this option is only necessary when versioning by URL segment. The SubstitutionFormat
        // can also be used to control the format of the API version in route templates
        options.SubstituteApiVersionInUrl = true;
    });
}
catch (Exception ex)
{
    Log.Error(ex, "Error adding versioned API explorer");
    throw;
}

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddDbContextCheck<ApplicationDbContext>("database-context");

// Add Razor Pages and Server-Side Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5);
    options.DisconnectedCircuitMaxRetained = 100;
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    options.MaxBufferedUnacknowledgedRenderBatches = 10;
});

// Add Response Caching
builder.Services.AddResponseCaching(options =>
{
    options.SizeLimit = 100 * 1024 * 1024; // 100 MB
    options.MaximumBodySize = 64 * 1024 * 1024; // 64 MB
    options.UseCaseSensitivePaths = false;
});

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// Configure Compression
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddSqlite(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        healthQuery: "SELECT 1;",
        name: "database",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "db", "sqlite", "ready" })
    .AddDiskStorageHealthCheck(settings =>
    {
        settings.AddDrive("/", 500); // 500MB minimum free disk space
    }, name: "disk", tags: new[] { "storage", "ready" })
    .AddMemoryHealthCheck(1024 * 1024 * 100); // 100MB threshold

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "BlazorCrudDemo API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.\r\n\r\nEnter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\""
    });
    
    options.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Include XML comments for API documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

// Configure Blazor Server with detailed logging
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(5);
    options.DisconnectedCircuitMaxRetained = 100;
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    options.MaxBufferedUnacknowledgedRenderBatches = 10;
});

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Add nonce script tag helper
builder.Services.AddScoped<NonceScriptTagHelper>();

// Configure JSON serialization for SignalR
builder.Services.Configure<JsonHubProtocolOptions>(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null; // Use exact property names
});

// Add controllers
builder.Services.AddControllers();

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Add 2FA service
builder.Services.AddScoped<TwoFactorService>();

// Add cascading authentication state for Blazor Server
builder.Services.AddCascadingAuthenticationState();

// Configure Identity with enhanced security settings
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Enhanced password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true; // Require email confirmation
    options.SignIn.RequireConfirmedAccount = true;

    // Account lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Token providers
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    
    // Session settings
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/access-denied";
    options.ReturnUrlParameter = "returnUrl";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api") ||
                (context.Request.Headers.ContainsKey("X-Requested-With") && 
                 context.Request.Headers["X-Requested-With"] == "XMLHttpRequest"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api") ||
                (context.Request.Headers.ContainsKey("X-Requested-With") && 
                 context.Request.Headers["X-Requested-With"] == "XMLHttpRequest"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

// Register CustomAuthenticationStateProvider as the implementation for AuthenticationStateProvider
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Explicitly register CustomAuthenticationStateProvider
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
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
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "https://localhost:5120";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "https://localhost:5120";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

// Configure JWT Bearer authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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
// Application services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStateContainer, StateContainer>();
builder.Services.AddScoped<IBusinessValidationService, BusinessValidationService>();

// Add HTTP context accessor if not already added
builder.Services.AddHttpContextAccessor();

// Add session support for storing temporary data
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
});
builder.Services.AddScoped<ErrorNotificationService>();
builder.Services.AddScoped<ErrorRecoveryService>();
builder.Services.AddScoped<NetworkStatusService>();
builder.Services.AddScoped<OfflineModeService>();
builder.Services.AddScoped<ErrorRecoveryGuidanceService>();

// Register authentication services
builder.Services.AddScoped<BlazorCrudDemo.Web.Services.IAuthenticationService, BlazorCrudDemo.Web.Services.AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Register modal service
builder.Services.AddScoped<ModalService>();

// Register background services
builder.Services.AddHostedService<MaintenanceBackgroundService>();
builder.Services.AddHostedService<CacheCleanupBackgroundService>();
builder.Services.AddHostedService<DataSyncBackgroundService>();

// Register SignalR with enhanced configuration
builder.Services.AddSignalR(hubOptions => 
{
    // Enable detailed error messages in development
    hubOptions.EnableDetailedErrors = builder.Environment.IsDevelopment();
    
    // Configure keep-alive and handshake timeouts
    hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(15);
    hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(15);
    
    // Configure client timeout and reconnection settings
    hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    
    // Enable automatic reconnection
    hubOptions.MaximumReceiveMessageSize = 32 * 1024; // 32KB
    
}).AddHubOptions<NotificationHub>(options =>
{
    // Configure notification hub options
    options.EnableDetailedErrors = true;
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// Configure CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(';') ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Blazored services
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredLocalStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorCrudDemo API V1");
        options.RoutePrefix = "api";
        options.DocumentTitle = "BlazorCrudDemo API Documentation";
        options.EnableTryItOutByDefault();
        
        // Add custom CSS for better UI
        options.InjectStylesheet("/css/swagger.css");
    });
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Configure WebSockets for Blazor Server
// Enable WebSockets
app.UseWebSockets(new Microsoft.AspNetCore.Builder.WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

// Configure CORS to allow WebSocket connections
app.UseCors("AllowSpecificOrigins");

// Map the Blazor hub with WebSocket and LongPolling transports
app.MapBlazorHub(options =>
{
    options.Transports = HttpTransportType.WebSockets | 
                        HttpTransportType.LongPolling;
});

// Add a redirect from /auth/login to /api/auth/login
app.MapGet("/auth/login", async context =>
{
    context.Response.Redirect("/api/auth/login", permanent: true);
    await Task.CompletedTask;
});

// Map the fallback route for client-side routing
app.MapFallbackToPage("/_Host");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

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

// Add security headers middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

// Add nonce to script tags
app.Use(async (context, next) =>
{
    var nonce = context.Items["CspNonce"]?.ToString();
    if (!string.IsNullOrEmpty(nonce))
    {
        context.Response.Headers["Content-Security-Policy"] = context.Response.Headers["Content-Security-Policy"]
            .ToString()
            .Replace("{nonce}", nonce);
    }
    await next();
});

// Enable rate limiting
app.UseRateLimiter();

// Add global exception handler middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseStaticFiles();

// Add health check endpoints
app.MapHealthChecks("/health", new()
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data,
                duration = e.Value.Duration
            }),
            totalDuration = report.TotalDuration
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false
});

app.UseRouting();

// Configure IdentityServer middleware
// app.UseIdentityServer();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers and endpoints
app.MapControllers();
app.MapRazorPages();

// Configure Blazor Hub with enhanced WebSocket settings
app.MapBlazorHub(options => 
{
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | 
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
    options.CloseOnAuthenticationExpiration = true;
    options.ApplicationMaxBufferSize = 64 * 1024; // 64KB
    options.TransportMaxBufferSize = 64 * 1024;   // 64KB
    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(30);
    options.TransportSendTimeout = TimeSpan.FromSeconds(30);
});
app.MapHub<NotificationHub>("/notificationHub");
app.MapFallbackToPage("/_Host");

// Ensure database is migrated and created
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // Apply any pending migrations
            logger.LogInformation("Applying database migrations...");
            dbContext.Database.Migrate();

            // Seed sample data if the database is empty
            logger.LogInformation("Seeding sample data...");
            await SampleDataSeeder.SeedAsync(scope.ServiceProvider);
            
            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

app.Run();
