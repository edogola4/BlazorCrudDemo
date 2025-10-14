using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BlazorCrudDemo.Data.Interfaces;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Exceptions;

namespace BlazorCrudDemo.Data.Repositories;

/// <summary>
/// Generic base repository implementation for CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<BaseRepository<T>> _logger;

    /// <summary>
    /// Initializes a new instance of the BaseRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    protected BaseRepository(ApplicationDbContext context, ILogger<BaseRepository<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Getting all {EntityType} entities", typeof(T).Name);
            var entities = await _dbSet.AsNoTracking().ToListAsync();
            _logger.LogDebug("Retrieved {Count} {EntityType} entities", entities.Count, typeof(T).Name);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all {EntityType} entities", typeof(T).Name);
            throw new RepositoryException($"Failed to get all {typeof(T).Name} entities", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        try
        {
            _logger.LogDebug("Getting all {EntityType} entities with includes", typeof(T).Name);
            var query = _dbSet.AsNoTracking();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var entities = await query.ToListAsync();
            _logger.LogDebug("Retrieved {Count} {EntityType} entities with includes", entities.Count, typeof(T).Name);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all {EntityType} entities with includes", typeof(T).Name);
            throw new RepositoryException($"Failed to get all {typeof(T).Name} entities with includes", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entity with ID {Id}", typeof(T).Name, id);
            var entity = await _dbSet.FindAsync(id);

            if (entity != null)
            {
                _logger.LogDebug("Found {EntityType} entity with ID {Id}", typeof(T).Name, id);
            }
            else
            {
                _logger.LogWarning("No {EntityType} entity found with ID {Id}", typeof(T).Name, id);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting {EntityType} entity with ID {Id}", typeof(T).Name, id);
            throw new RepositoryException($"Failed to get {typeof(T).Name} entity with ID {id}", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entity with ID {Id} and includes", typeof(T).Name, id);
            var query = _dbSet.AsNoTracking().Where(CreateIdExpression(id));

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var entity = await query.FirstOrDefaultAsync();

            if (entity != null)
            {
                _logger.LogDebug("Found {EntityType} entity with ID {Id} and includes", typeof(T).Name, id);
            }
            else
            {
                _logger.LogWarning("No {EntityType} entity found with ID {Id}", typeof(T).Name, id);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting {EntityType} entity with ID {Id} and includes", typeof(T).Name, id);
            throw new RepositoryException($"Failed to get {typeof(T).Name} entity with ID {id} and includes", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            _logger.LogDebug("Finding {EntityType} entities with predicate", typeof(T).Name);
            var entities = await _dbSet.AsNoTracking().Where(predicate).ToListAsync();
            _logger.LogDebug("Found {Count} {EntityType} entities matching predicate", entities.Count, typeof(T).Name);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while finding {EntityType} entities with predicate", typeof(T).Name);
            throw new RepositoryException($"Failed to find {typeof(T).Name} entities with predicate", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        try
        {
            _logger.LogDebug("Finding {EntityType} entities with predicate and includes", typeof(T).Name);
            var query = _dbSet.AsNoTracking().Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var entities = await query.ToListAsync();
            _logger.LogDebug("Found {Count} {EntityType} entities matching predicate with includes", entities.Count, typeof(T).Name);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while finding {EntityType} entities with predicate and includes", typeof(T).Name);
            throw new RepositoryException($"Failed to find {typeof(T).Name} entities with predicate and includes", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            _logger.LogDebug("Adding new {EntityType} entity", typeof(T).Name);
            var entry = await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully added {EntityType} entity with ID {Id}", typeof(T).Name, GetEntityId(entity));
            return entry.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding {EntityType} entity", typeof(T).Name);
            throw new RepositoryException($"Failed to add {typeof(T).Name} entity", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(T entity)
    {
        try
        {
            _logger.LogDebug("Updating {EntityType} entity with ID {Id}", typeof(T).Name, GetEntityId(entity));
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated {EntityType} entity with ID {Id}", typeof(T).Name, GetEntityId(entity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating {EntityType} entity with ID {Id}", typeof(T).Name, GetEntityId(entity));
            throw new RepositoryException($"Failed to update {typeof(T).Name} entity", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(int id)
    {
        try
        {
            _logger.LogDebug("Deleting {EntityType} entity with ID {Id}", typeof(T).Name, id);
            var entity = await GetByIdAsync(id);

            if (entity == null)
            {
                throw new EntityNotFoundException($"{typeof(T).Name} with ID {id} not found");
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted {EntityType} entity with ID {Id}", typeof(T).Name, id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting {EntityType} entity with ID {Id}", typeof(T).Name, id);
            throw new RepositoryException($"Failed to delete {typeof(T).Name} entity with ID {id}", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<bool> ExistsAsync(int id)
    {
        try
        {
            var exists = await _dbSet.AnyAsync(CreateIdExpression(id));
            _logger.LogDebug("{EntityType} entity with ID {Id} exists: {Exists}", typeof(T).Name, id, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if {EntityType} entity with ID {Id} exists", typeof(T).Name, id);
            throw new RepositoryException($"Failed to check if {typeof(T).Name} entity with ID {id} exists", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync()
    {
        try
        {
            var count = await _dbSet.AsNoTracking().CountAsync();
            _logger.LogDebug("Counted {Count} {EntityType} entities", count, typeof(T).Name);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while counting {EntityType} entities", typeof(T).Name);
            throw new RepositoryException($"Failed to count {typeof(T).Name} entities", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var count = await _dbSet.AsNoTracking().CountAsync(predicate);
            _logger.LogDebug("Counted {Count} {EntityType} entities matching predicate", count, typeof(T).Name);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while counting {EntityType} entities with predicate", typeof(T).Name);
            throw new RepositoryException($"Failed to count {typeof(T).Name} entities with predicate", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var any = await _dbSet.AsNoTracking().AnyAsync(predicate);
            _logger.LogDebug("Any {EntityType} entities matching predicate: {Any}", typeof(T).Name, any);
            return any;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if any {EntityType} entities match predicate", typeof(T).Name);
            throw new RepositoryException($"Failed to check if any {typeof(T).Name} entities match predicate", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var entity = await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);

            if (entity != null)
            {
                _logger.LogDebug("Found first {EntityType} entity matching predicate", typeof(T).Name);
            }
            else
            {
                _logger.LogDebug("No {EntityType} entity found matching predicate", typeof(T).Name);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting first {EntityType} entity matching predicate", typeof(T).Name);
            throw new RepositoryException($"Failed to get first {typeof(T).Name} entity matching predicate", ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        try
        {
            var query = _dbSet.AsNoTracking().Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            var entity = await query.FirstOrDefaultAsync();

            if (entity != null)
            {
                _logger.LogDebug("Found first {EntityType} entity matching predicate with includes", typeof(T).Name);
            }
            else
            {
                _logger.LogDebug("No {EntityType} entity found matching predicate with includes", typeof(T).Name);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting first {EntityType} entity matching predicate with includes", typeof(T).Name);
            throw new RepositoryException($"Failed to get first {typeof(T).Name} entity matching predicate with includes", ex);
        }
    }

    /// <summary>
    /// Creates an expression for filtering by ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>An expression that filters by the ID property.</returns>
    protected virtual Expression<Func<T, bool>> CreateIdExpression(int id)
    {
        var parameter = Expression.Parameter(typeof(T), "entity");
        var idProperty = Expression.Property(parameter, "Id");
        var idValue = Expression.Constant(id);
        var equalExpression = Expression.Equal(idProperty, idValue);

        return Expression.Lambda<Func<T, bool>>(equalExpression, parameter);
    }

    /// <summary>
    /// Gets the ID value of an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The ID value.</returns>
    protected virtual object? GetEntityId(T entity)
    {
        var idProperty = typeof(T).GetProperty("Id");
        return idProperty?.GetValue(entity);
    }
}
