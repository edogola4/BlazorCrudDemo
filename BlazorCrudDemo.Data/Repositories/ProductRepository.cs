using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BlazorCrudDemo.Shared.Models;
using BlazorCrudDemo.Data.Interfaces;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Exceptions;

namespace BlazorCrudDemo.Data.Repositories;

/// <summary>
/// Repository implementation for Product entities with advanced operations.
/// </summary>
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    /// <summary>
    /// Initializes a new instance of the ProductRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger)
        : base(context, logger)
    {
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        var products = await base.GetAllAsync(p => p.Category);
        return products ?? Enumerable.Empty<Product>();
    }

    /// <inheritdoc />
    public override async Task<Product?> GetByIdAsync(int id)
    {
        return await base.GetByIdAsync(id, p => p.Category);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        try
        {
            _logger.LogDebug("Getting products for category ID {CategoryId}", categoryId);

            var products = await _dbSet
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();

            _logger.LogDebug("Retrieved {Count} products for category ID {CategoryId}", products.Count, categoryId);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting products for category ID {CategoryId}", categoryId);
            throw new RepositoryException($"Failed to get products for category ID {categoryId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> SearchAsync(
        string? searchTerm = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? inStock = null)
    {
        try
        {
            _logger.LogDebug("Searching products with filters: SearchTerm={SearchTerm}, CategoryId={CategoryId}, MinPrice={MinPrice}, MaxPrice={MaxPrice}, InStock={InStock}",
                searchTerm, categoryId, minPrice, maxPrice, inStock);

            var query = _dbSet.AsNoTracking().Where(p => p.IsActive);

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm) ||
                    p.SKU.Contains(searchTerm));
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Apply price range filters
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Apply stock filter
            if (inStock.HasValue && inStock.Value)
            {
                query = query.Where(p => p.Stock > 0);
            }

            var products = await query
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            _logger.LogDebug("Search returned {Count} products", products.Count);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching products");
            throw new RepositoryException("Failed to search products", ex);
        }
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string sortBy = "Name",
        bool sortDescending = false,
        int? categoryId = null)
    {
        try
        {
            _logger.LogDebug("Getting paginated products: PageNumber={PageNumber}, PageSize={PageSize}, SortBy={SortBy}, SortDescending={SortDescending}, CategoryId={CategoryId}",
                pageNumber, pageSize, sortBy, sortDescending, categoryId);

            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            var query = _dbSet.AsNoTracking().Where(p => p.IsActive);

            // Apply category filter
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => sortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "createddate" => sortDescending ? query.OrderByDescending(p => p.CreatedDate) : query.OrderBy(p => p.CreatedDate),
                "stock" => sortDescending ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
                _ => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
            };

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Category)
                .ToListAsync();

            _logger.LogDebug("Retrieved {Count} products out of {TotalCount} total (Page {PageNumber}/{TotalPages})",
                products.Count, totalCount, pageNumber, (int)Math.Ceiling(totalCount / (double)pageSize));

            return (products, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting paginated products");
            throw new RepositoryException("Failed to get paginated products", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        try
        {
            _logger.LogDebug("Getting all active products");

            var products = await _dbSet
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();

            _logger.LogDebug("Retrieved {Count} active products", products.Count);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active products");
            throw new RepositoryException("Failed to get active products", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        try
        {
            _logger.LogDebug("Getting product by SKU {Sku}", sku);

            var product = await _dbSet
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.SKU == sku);

            if (product != null)
            {
                _logger.LogDebug("Found product with SKU {Sku}", sku);
            }
            else
            {
                _logger.LogWarning("No product found with SKU {Sku}", sku);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product by SKU {Sku}", sku);
            throw new RepositoryException($"Failed to get product by SKU {sku}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        try
        {
            _logger.LogDebug("Getting products with stock below threshold {Threshold}", threshold);

            var products = await _dbSet
                .Where(p => p.IsActive && p.Stock < threshold)
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderBy(p => p.Stock)
                .ToListAsync();

            _logger.LogDebug("Found {Count} products with low stock (threshold: {Threshold})", products.Count, threshold);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting low stock products");
            throw new RepositoryException("Failed to get low stock products", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        try
        {
            _logger.LogDebug("Getting products in price range {MinPrice} to {MaxPrice}", minPrice, maxPrice);

            var products = await _dbSet
                .Where(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice)
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderBy(p => p.Price)
                .ToListAsync();

            _logger.LogDebug("Found {Count} products in price range {MinPrice} to {MaxPrice}", products.Count, minPrice, maxPrice);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting products by price range");
            throw new RepositoryException("Failed to get products by price range", ex);
        }
    }

    /// <inheritdoc />
    public async Task UpdateStockAsync(int productId, int newStock)
    {
        try
        {
            _logger.LogDebug("Updating stock for product ID {ProductId} to {NewStock}", productId, newStock);

            var product = await _dbSet.FindAsync(productId);
            if (product == null)
            {
                throw new EntityNotFoundException(nameof(Product), productId);
            }

            var oldStock = product.Stock;
            product.Stock = newStock;
            product.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated stock for product ID {ProductId} from {OldStock} to {NewStock}", productId, oldStock, newStock);
        }
        catch (EntityNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating stock for product ID {ProductId}", productId);
            throw new RepositoryException($"Failed to update stock for product ID {productId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<(int TotalProducts, int ActiveProducts, int OutOfStockProducts, decimal AveragePrice)> GetProductStatisticsAsync()
    {
        try
        {
            _logger.LogDebug("Getting product statistics");

            var totalProducts = await _dbSet.CountAsync();
            var activeProducts = await _dbSet.CountAsync(p => p.IsActive);
            var outOfStockProducts = await _dbSet.CountAsync(p => p.IsActive && p.Stock == 0);
            var averagePrice = await _dbSet
                .Where(p => p.IsActive)
                .AverageAsync(p => p.Price);

            var stats = (totalProducts, activeProducts, outOfStockProducts, averagePrice);

            _logger.LogDebug("Product statistics: Total={Total}, Active={Active}, OutOfStock={OutOfStock}, AveragePrice={AveragePrice}",
                stats.totalProducts, stats.activeProducts, stats.outOfStockProducts, stats.averagePrice);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product statistics");
            throw new RepositoryException("Failed to get product statistics", ex);
        }
    }
}
