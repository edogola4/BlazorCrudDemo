using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Shared.Exceptions;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// Service interface for product business logic operations.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Gets all products with optional filtering and pagination.
    /// </summary>
    /// <param name="categoryId">Optional category filter.</param>
    /// <param name="searchTerm">Optional search term.</param>
    /// <param name="pageNumber">Page number for pagination.</param>
    /// <param name="pageSize">Page size for pagination.</param>
    /// <param name="sortBy">Sort field.</param>
    /// <param name="sortDescending">Sort direction.</param>
    /// <returns>Paginated result of products.</returns>
    Task<PaginatedResult<ProductDto>> GetProductsAsync(
        int? categoryId = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "Name",
        bool sortDescending = false);

    /// <summary>
    /// Gets a product by ID with full details.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product DTO or null if not found.</returns>
    Task<ProductDto?> GetProductAsync(int id);

    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    /// <param name="sku">The product SKU.</param>
    /// <returns>The product DTO or null if not found.</returns>
    Task<ProductDto?> GetProductBySkuAsync(string sku);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="createDto">The product creation data.</param>
    /// <returns>The created product DTO.</returns>
    Task<ProductDto> CreateProductAsync(CreateProductDto createDto);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="updateDto">The product update data.</param>
    /// <returns>The updated product DTO.</returns>
    Task<ProductDto> UpdateProductAsync(UpdateProductDto updateDto);

    /// <summary>
    /// Deletes a product by ID.
    /// </summary>
    /// <param name="id">The product ID to delete.</param>
    /// <returns>True if deleted successfully.</returns>
    Task<bool> DeleteProductAsync(int id);

    /// <summary>
    /// Updates the stock quantity for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="newStock">The new stock quantity.</param>
    /// <returns>True if updated successfully.</returns>
    Task<bool> UpdateStockAsync(int productId, int newStock);

    /// <summary>
    /// Validates product data for business rules.
    /// </summary>
    /// <param name="productDto">The product data to validate.</param>
    /// <returns>Validation result with any errors.</returns>
    Task<ValidationResult> ValidateProductAsync(ProductDto productDto);

    /// <summary>
    /// Exports products to Excel format.
    /// </summary>
    /// <param name="categoryId">Optional category filter.</param>
    /// <param name="searchTerm">Optional search term.</param>
    /// <returns>Excel file as byte array.</returns>
    Task<byte[]> ExportToExcelAsync(int? categoryId = null, string? searchTerm = null);

    /// <summary>
    /// Gets product statistics.
    /// </summary>
    /// <returns>Product statistics.</returns>
    Task<(int TotalProducts, int ActiveProducts, int OutOfStockProducts, decimal AveragePrice)> GetProductStatisticsAsync();

    /// <summary>
    /// Gets products with low stock.
    /// </summary>
    /// <param name="threshold">Stock threshold (default: 10).</param>
    /// <returns>Products with stock below threshold.</returns>
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10);

    /// <summary>
    /// Gets products in a price range.
    /// </summary>
    /// <param name="minPrice">Minimum price.</param>
    /// <param name="maxPrice">Maximum price.</param>
    /// <returns>Products in the price range.</returns>
    Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);

    /// <summary>
    /// Searches products with advanced filters.
    /// </summary>
    /// <param name="searchDto">Search criteria.</param>
    /// <returns>Filtered products.</returns>
    Task<IEnumerable<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto);
}

/// <summary>
/// DTO for product search criteria.
/// </summary>
public class ProductSearchDto
{
    /// <summary>
    /// Search term for name and description.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Category ID filter.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Minimum price filter.
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Maximum price filter.
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Stock availability filter.
    /// </summary>
    public bool? InStock { get; set; }

    /// <summary>
    /// Active products only filter.
    /// </summary>
    public bool? ActiveOnly { get; set; }
}
