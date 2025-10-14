using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BlazorCrudDemo.Data.Interfaces;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Repositories;
using BlazorCrudDemo.Data.Exceptions;

namespace BlazorCrudDemo.Data.UnitOfWork;

/// <summary>
/// Unit of Work implementation for managing database transactions and repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly IServiceProvider _serviceProvider;

    private IProductRepository? _productRepository;
    private ICategoryRepository? _categoryRepository;

    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger, IServiceProvider serviceProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public IProductRepository Products => _productRepository ??= _serviceProvider.GetRequiredService<IProductRepository>();

    /// <inheritdoc />
    public ICategoryRepository Categories => _categoryRepository ??= _serviceProvider.GetRequiredService<ICategoryRepository>();

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync()
    {
        try
        {
            _logger.LogDebug("Saving changes to database");

            var result = await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully saved {ChangeCount} changes to database", result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving changes to database");
            throw new RepositoryException("Failed to save changes to database", ex);
        }
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Saving changes to database with cancellation token");

            var result = await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully saved {ChangeCount} changes to database", result);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Save changes operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving changes to database");
            throw new RepositoryException("Failed to save changes to database", ex);
        }
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync()
    {
        try
        {
            _logger.LogDebug("Beginning database transaction");

            await _context.Database.BeginTransactionAsync();

            _logger.LogInformation("Database transaction started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while beginning database transaction");
            throw new RepositoryException("Failed to begin database transaction", ex);
        }
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel)
    {
        try
        {
            _logger.LogDebug("Beginning database transaction with isolation level {IsolationLevel}", isolationLevel);

            await _context.Database.BeginTransactionAsync();

            _logger.LogInformation("Database transaction started with isolation level {IsolationLevel}", isolationLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while beginning database transaction with isolation level {IsolationLevel}", isolationLevel);
            throw new RepositoryException($"Failed to begin database transaction with isolation level {isolationLevel}", ex);
        }
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync()
    {
        try
        {
            _logger.LogDebug("Committing database transaction");

            await _context.Database.CommitTransactionAsync();

            _logger.LogInformation("Database transaction committed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while committing database transaction");
            throw new RepositoryException("Failed to commit database transaction", ex);
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync()
    {
        try
        {
            _logger.LogDebug("Rolling back database transaction");

            await _context.Database.RollbackTransactionAsync();

            _logger.LogInformation("Database transaction rolled back successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rolling back database transaction");
            throw new RepositoryException("Failed to rollback database transaction", ex);
        }
    }

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await ExecuteInTransactionAsync(action, System.Data.IsolationLevel.ReadCommitted);
    }

    /// <inheritdoc />
    public async Task ExecuteInTransactionAsync(Func<Task> action, System.Data.IsolationLevel isolationLevel)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        try
        {
            _logger.LogDebug("Executing action in transaction with isolation level {IsolationLevel}", isolationLevel);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await action();

                await transaction.CommitAsync();

                _logger.LogInformation("Transaction completed successfully");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing action in transaction");
            throw new RepositoryException("Failed to execute action in transaction", ex);
        }
    }

    /// <inheritdoc />
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func)
    {
        return await ExecuteInTransactionAsync(func, System.Data.IsolationLevel.ReadCommitted);
    }

    /// <inheritdoc />
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func, System.Data.IsolationLevel isolationLevel)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        try
        {
            _logger.LogDebug("Executing function in transaction with isolation level {IsolationLevel}", isolationLevel);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var result = await func();

                await transaction.CommitAsync();

                _logger.LogInformation("Transaction completed successfully with result of type {ResultType}", typeof(TResult).Name);

                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing function in transaction");
            throw new RepositoryException("Failed to execute function in transaction", ex);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the UnitOfWork and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _context?.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer for the UnitOfWork class.
    /// </summary>
    ~UnitOfWork()
    {
        Dispose(false);
    }
}
