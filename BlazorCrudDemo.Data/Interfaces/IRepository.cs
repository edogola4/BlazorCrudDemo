using System.Linq.Expressions;

namespace BlazorCrudDemo.Data.Interfaces;

/// <summary>
/// Generic repository interface for basic CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Gets all entities with optional includes.
    /// </summary>
    /// <param name="includes">The navigation properties to include.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of entities.</returns>
    Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity or null if not found.</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Gets an entity by its ID with optional includes.
    /// </summary>
    /// <param name="id">The entity ID.</param>
    /// <param name="includes">The navigation properties to include.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity or null if not found.</returns>
    Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Finds entities based on a predicate.
    /// </summary>
    /// <param name="predicate">The condition to filter entities.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the filtered entities.</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Finds entities based on a predicate with optional includes.
    /// </summary>
    /// <param name="predicate">The condition to filter entities.</param>
    /// <param name="includes">The navigation properties to include.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the filtered entities.</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the added entity.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The entity ID to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if an entity exists by its ID.
    /// </summary>
    /// <param name="id">The entity ID to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the entity exists, false otherwise.</returns>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the count of entities.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the total count.</returns>
    Task<int> CountAsync();

    /// <summary>
    /// Gets the count of entities based on a predicate.
    /// </summary>
    /// <param name="predicate">The condition to filter entities.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the filtered count.</returns>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Checks if any entity matches the predicate.
    /// </summary>
    /// <param name="predicate">The condition to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if any entity matches, false otherwise.</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Gets the first entity or default.
    /// </summary>
    /// <param name="predicate">The condition to filter entities.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first entity or null.</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Gets the first entity or default with includes.
    /// </summary>
    /// <param name="predicate">The condition to filter entities.</param>
    /// <param name="includes">The navigation properties to include.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first entity or null.</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
}
