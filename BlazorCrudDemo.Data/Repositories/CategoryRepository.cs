using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BlazorCrudDemo.Shared.Models;
using BlazorCrudDemo.Data.Interfaces;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Exceptions;

namespace BlazorCrudDemo.Data.Repositories;

/// <summary>
/// Repository implementation for Category entities with specialized operations.
/// </summary>
public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    /// <summary>
    /// Initializes a new instance of the CategoryRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public CategoryRepository(ApplicationDbContext context, ILogger<CategoryRepository> logger)
        : base(context, logger)
    {
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<Category>> GetAllAsync()
    {
        var categories = await base.GetAllAsync();
        return categories ?? Enumerable.Empty<Category>();
    }

    /// <inheritdoc />
    public override async Task<Category?> GetByIdAsync(int id)
    {
        return await base.GetByIdAsync(id, c => c.Products);
    }

    /// <inheritdoc />
    public async Task<Category?> GetWithProductsAsync(int id)
    {
        try
        {
            _logger.LogDebug("Getting category with products for ID {Id}", id);

            var category = await _dbSet
                .Include(c => c.Products.Where(p => p.IsActive))
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                _logger.LogDebug("Found category with ID {Id} with {ProductCount} products", id, category.Products?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("No category found with ID {Id}", id);
            }

            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting category with products for ID {Id}", id);
            throw new RepositoryException($"Failed to get category with products for ID {id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
    {
        try
        {
            _logger.LogDebug("Getting all active categories");

            var categories = await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

            _logger.LogDebug("Retrieved {Count} active categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active categories");
            throw new RepositoryException("Failed to get active categories", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetOrderedByDisplayOrderAsync(bool includeInactive = false)
    {
        try
        {
            _logger.LogDebug("Getting categories ordered by display order (includeInactive: {IncludeInactive})", includeInactive);

            var query = _dbSet.AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            _logger.LogDebug("Retrieved {Count} categories ordered by display order", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting categories ordered by display order");
            throw new RepositoryException("Failed to get categories ordered by display order", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetWithProductCountsAsync(bool includeInactive = false)
    {
        try
        {
            _logger.LogDebug("Getting categories with product counts (includeInactive: {IncludeInactive})", includeInactive);

            var query = _dbSet.AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var categories = await query
                .Include(c => c.Products.Where(p => p.IsActive))
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            _logger.LogDebug("Retrieved {Count} categories with product counts", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting categories with product counts");
            throw new RepositoryException("Failed to get categories with product counts", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Category?> GetByNameAsync(string name)
    {
        try
        {
            _logger.LogDebug("Getting category by name {Name}", name);

            var category = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name == name);

            if (category != null)
            {
                _logger.LogDebug("Found category with name {Name}", name);
            }
            else
            {
                _logger.LogWarning("No category found with name {Name}", name);
            }

            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting category by name {Name}", name);
            throw new RepositoryException($"Failed to get category by name {name}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
    {
        try
        {
            _logger.LogDebug("Checking if category name {Name} exists (excludeId: {ExcludeId})", name, excludeId);

            var query = _dbSet.Where(c => c.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync();

            _logger.LogDebug("Category name {Name} exists: {Exists}", name, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if category name {Name} exists", name);
            throw new RepositoryException($"Failed to check if category name {name} exists", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetCategoriesWithProductsAsync(bool includeInactive = false)
    {
        try
        {
            _logger.LogDebug("Getting categories that have products (includeInactive: {IncludeInactive})", includeInactive);

            var query = _dbSet.AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var categories = await query
                .Where(c => c.Products.Any(p => p.IsActive))
                .Include(c => c.Products.Where(p => p.IsActive))
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            _logger.LogDebug("Found {Count} categories with products", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting categories with products");
            throw new RepositoryException("Failed to get categories with products", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetEmptyCategoriesAsync(bool includeInactive = false)
    {
        try
        {
            _logger.LogDebug("Getting categories with no products (includeInactive: {IncludeInactive})", includeInactive);

            var query = _dbSet.AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var categories = await query
                .Where(c => !c.Products.Any(p => p.IsActive))
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            _logger.LogDebug("Found {Count} empty categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting empty categories");
            throw new RepositoryException("Failed to get empty categories", ex);
        }
    }

    /// <inheritdoc />
    public async Task<(int TotalCategories, int ActiveCategories, int CategoriesWithProducts, int EmptyCategories)> GetCategoryStatisticsAsync()
    {
        try
        {
            _logger.LogDebug("Getting category statistics");

            var totalCategories = await _dbSet.CountAsync();
            var activeCategories = await _dbSet.CountAsync(c => c.IsActive);
            var categoriesWithProducts = await _dbSet.CountAsync(c => c.IsActive && c.Products.Any(p => p.IsActive));
            var emptyCategories = await _dbSet.CountAsync(c => c.IsActive && !c.Products.Any(p => p.IsActive));

            var stats = (totalCategories, activeCategories, categoriesWithProducts, emptyCategories);

            _logger.LogDebug("Category statistics: Total={Total}, Active={Active}, WithProducts={WithProducts}, Empty={Empty}",
                stats.totalCategories, stats.activeCategories, stats.categoriesWithProducts, stats.emptyCategories);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting category statistics");
            throw new RepositoryException("Failed to get category statistics", ex);
        }
    }

    /// <inheritdoc />
    public async Task UpdateDisplayOrdersAsync(Dictionary<int, int> categoryOrders)
    {
        try
        {
            _logger.LogDebug("Updating display orders for {Count} categories", categoryOrders.Count);

            foreach (var (categoryId, displayOrder) in categoryOrders)
            {
                var category = await _dbSet.FindAsync(categoryId);
                if (category != null)
                {
                    category.DisplayOrder = displayOrder;
                    category.ModifiedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated display orders for {Count} categories", categoryOrders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating display orders");
            throw new RepositoryException("Failed to update display orders", ex);
        }
    }
}
