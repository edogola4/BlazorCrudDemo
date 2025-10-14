namespace BlazorCrudDemo.Data.Interfaces;

/// <summary>
/// Unit of Work interface for managing database transactions and repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the product repository.
    /// </summary>
    IProductRepository Products { get; }

    /// <summary>
    /// Gets the category repository.
    /// </summary>
    ICategoryRepository Categories { get; }

    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Saves all changes made in this unit of work to the database with cancellation support.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Begins a database transaction.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync();

    /// <summary>
    /// Begins a database transaction with a specific isolation level.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackTransactionAsync();

    /// <summary>
    /// Executes an action within a transaction scope.
    /// </summary>
    /// <param name="action">The action to execute within the transaction.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteInTransactionAsync(Func<Task> action);

    /// <summary>
    /// Executes an action within a transaction scope with a specific isolation level.
    /// </summary>
    /// <param name="action">The action to execute within the transaction.</param>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteInTransactionAsync(Func<Task> action, System.Data.IsolationLevel isolationLevel);

    /// <summary>
    /// Executes a function within a transaction scope and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The function to execute within the transaction.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the function.</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func);

    /// <summary>
    /// Executes a function within a transaction scope with a specific isolation level and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The function to execute within the transaction.</param>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the function.</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func, System.Data.IsolationLevel isolationLevel);
}
