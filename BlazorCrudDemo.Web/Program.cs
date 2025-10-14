using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Interfaces;
using BlazorCrudDemo.Data.Repositories;
using BlazorCrudDemo.Data.Contexts.Interceptors;
using BlazorCrudDemo.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Blazored.Toast;
using Blazored.Modal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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

// Add Blazored services
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

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
