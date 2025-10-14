using BlazorCrudDemo.Shared.Models;
using System.Linq.Expressions;

namespace BlazorCrudDemo.Data.Interfaces;

/// <summary>
/// Repository interface for Category entities with specialized operations.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Gets a category with its products.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category with products or null if not found.</returns>
    Task<Category?> GetWithProductsAsync(int id);

    /// <summary>
    /// Gets all active categories ordered by display order.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the active categories.</returns>
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();

    /// <summary>
    /// Gets categories ordered by display order.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the categories ordered by display order.</returns>
    Task<IEnumerable<Category>> GetOrderedByDisplayOrderAsync(bool includeInactive = false);

    /// <summary>
    /// Gets categories with product counts.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains categories with product count information.</returns>
    Task<IEnumerable<Category>> GetWithProductCountsAsync(bool includeInactive = false);

    /// <summary>
    /// Gets a category by name.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category or null if not found.</returns>
    Task<Category?> GetByNameAsync(string name);

    /// <summary>
    /// Checks if a category name already exists.
    /// </summary>
    /// <param name="name">The category name to check.</param>
    /// <param name="excludeId">Optional category ID to exclude from the check (for updates).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the name exists, false otherwise.</returns>
    Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Gets categories that have products.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains categories that have products.</returns>
    Task<IEnumerable<Category>> GetCategoriesWithProductsAsync(bool includeInactive = false);

    /// <summary>
    /// Gets categories that have no products.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains categories that have no products.</returns>
    Task<IEnumerable<Category>> GetEmptyCategoriesAsync(bool includeInactive = false);

    /// <summary>
    /// Gets category statistics.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains category statistics.</returns>
    Task<(int TotalCategories, int ActiveCategories, int CategoriesWithProducts, int EmptyCategories)> GetCategoryStatisticsAsync();

    /// <summary>
    /// Updates the display order of categories.
    /// </summary>
    /// <param name="categoryOrders">Dictionary mapping category ID to new display order.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateDisplayOrdersAsync(Dictionary<int, int> categoryOrders);
}
