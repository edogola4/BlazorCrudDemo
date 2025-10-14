using BlazorCrudDemo.Shared.Models;
using System.Linq.Expressions;

namespace BlazorCrudDemo.Data.Interfaces;

/// <summary>
/// Repository interface for Product entities with specialized operations.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Gets products by category ID.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the products in the category.</returns>
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);

    /// <summary>
    /// Searches products based on search criteria.
    /// </summary>
    /// <param name="searchTerm">The search term (searches in name and description).</param>
    /// <param name="categoryId">Optional category filter.</param>
    /// <param name="minPrice">Optional minimum price filter.</param>
    /// <param name="maxPrice">Optional maximum price filter.</param>
    /// <param name="inStock">Optional stock filter.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching products.</returns>
    Task<IEnumerable<Product>> SearchAsync(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null);

    /// <summary>
    /// Gets paginated products with sorting options.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="sortBy">The field to sort by.</param>
    /// <param name="sortDescending">True for descending order, false for ascending.</param>
    /// <param name="categoryId">Optional category filter.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the paginated products.</returns>
    Task<(IEnumerable<Product> Products, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string sortBy = "Name",
        bool sortDescending = false,
        int? categoryId = null);

    /// <summary>
    /// Gets all active products.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the active products.</returns>
    Task<IEnumerable<Product>> GetActiveProductsAsync();

    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    /// <param name="sku">The SKU to search for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the product or null if not found.</returns>
    Task<Product?> GetBySkuAsync(string sku);

    /// <summary>
    /// Gets products with low stock.
    /// </summary>
    /// <param name="threshold">The stock threshold (default: 10).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains products with stock below threshold.</returns>
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);

    /// <summary>
    /// Gets products within a price range.
    /// </summary>
    /// <param name="minPrice">The minimum price.</param>
    /// <param name="maxPrice">The maximum price.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains products in the price range.</returns>
    Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);

    /// <summary>
    /// Updates the stock quantity for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="newStock">The new stock quantity.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateStockAsync(int productId, int newStock);

    /// <summary>
    /// Gets product statistics.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains product statistics.</returns>
    Task<(int TotalProducts, int ActiveProducts, int OutOfStockProducts, decimal AveragePrice)> GetProductStatisticsAsync();
}
