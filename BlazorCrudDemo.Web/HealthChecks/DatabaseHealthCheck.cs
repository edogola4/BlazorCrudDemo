using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BlazorCrudDemo.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BlazorCrudDemo.Web.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(
            IDbContextFactory<ApplicationDbContext> dbContextFactory,
            ILogger<DatabaseHealthCheck> logger)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Starting database health check");
                
                await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
                
                // Test connection
                var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
                
                if (!canConnect)
                {
                    _logger.LogWarning("Database health check failed: Cannot connect to database");
                    return HealthCheckResult.Unhealthy("Cannot connect to database");
                }
                
                // Test a simple query
                try
                {
                    _ = await dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
                }
                catch (DbException ex)
                {
                    _logger.LogError(ex, "Database health check failed: Error executing test query");
                    return HealthCheckResult.Unhealthy("Error executing test query", ex);
                }

                _logger.LogDebug("Database health check completed successfully");
                return HealthCheckResult.Healthy("Database is available and responsive");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed with exception");
                return HealthCheckResult.Unhealthy("Database check failed", ex);
            }
        }
    }
}
