using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BlazorCrudDemo.Data.Interfaces;
using BlazorCrudDemo.Data.UnitOfWork;
using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Shared.Exceptions;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// Service implementation for category business logic operations.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CategoryService> _logger;

    private const string CategoriesCacheKey = "AllCategories";
    private const string CategoryCacheKeyPrefix = "Category_";
    private const string CategoryStatisticsCacheKey = "CategoryStatistics";

    public CategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync(bool includeInactive = false)
    {
        try
        {
            var cacheKey = $"{CategoriesCacheKey}_{includeInactive}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<CategoryDto>? cachedCategories))
            {
                _logger.LogDebug("Returning cached categories (includeInactive: {IncludeInactive})", includeInactive);
                return cachedCategories!;
            }

            var categories = await _unitOfWork.Categories.GetOrderedByDisplayOrderAsync(includeInactive);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            // Cache for 10 minutes
            _cache.Set(cacheKey, categoryDtos, TimeSpan.FromMinutes(10));

            _logger.LogDebug("Retrieved {Count} categories (includeInactive: {IncludeInactive})", categoryDtos.Count(), includeInactive);
            return categoryDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting categories");
            throw new ServiceException("Failed to get categories", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CategoryDto?> GetCategoryAsync(int id, bool includeProducts = false)
    {
        try
        {
            var cacheKey = $"{CategoryCacheKeyPrefix}{id}_{includeProducts}";

            if (_cache.TryGetValue(cacheKey, out CategoryDto? cachedCategory))
            {
                _logger.LogDebug("Returning cached category with ID {Id} (includeProducts: {IncludeProducts})", id, includeProducts);
                return cachedCategory;
            }

            Category? category;
            if (includeProducts)
            {
                category = await _unitOfWork.Categories.GetWithProductsAsync(id);
            }
            else
            {
                category = await _unitOfWork.Categories.GetByIdAsync(id);
            }

            if (category == null)
            {
                return null;
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);

            // Cache for 15 minutes
            _cache.Set(cacheKey, categoryDto, TimeSpan.FromMinutes(15));

            _logger.LogDebug("Retrieved category with ID {Id} (includeProducts: {IncludeProducts})", id, includeProducts);
            return categoryDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting category with ID {Id}", id);
            throw new ServiceException($"Failed to get category with ID {id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CategoryDto?> GetCategoryByNameAsync(string name)
    {
        try
        {
            _logger.LogDebug("Getting category by name {Name}", name);

            var category = await _unitOfWork.Categories.GetByNameAsync(name);
            if (category == null)
            {
                return null;
            }

            var categoryDto = _mapper.Map<CategoryDto>(category);

            _logger.LogDebug("Retrieved category with name {Name}", name);
            return categoryDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting category by name {Name}", name);
            throw new ServiceException($"Failed to get category by name {name}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto)
    {
        try
        {
            _logger.LogDebug("Creating category with name {Name}", createDto.Name);

            // Validate the category
            var validationResult = await ValidateCategoryAsync(_mapper.Map<CategoryDto>(createDto));
            if (!validationResult.IsValid)
            {
                throw new ValidationException("Category validation failed", validationResult.Errors);
            }

            // Check if category name already exists
            if (await _unitOfWork.Categories.CategoryNameExistsAsync(createDto.Name))
            {
                throw new DuplicateEntityException(nameof(Category), "Name", createDto.Name);
            }

            var category = _mapper.Map<Category>(createDto);

            // Use transaction for atomic operation
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                category = await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Invalidate relevant caches
                InvalidateCategoryCaches();

                var categoryDto = _mapper.Map<CategoryDto>(category);
                _logger.LogInformation("Successfully created category with ID {Id} and name {Name}", category.Id, category.Name);

                return categoryDto;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating category");
            throw new ServiceException("Failed to create category", ex);
        }
    }

    /// <inheritdoc />
    public async Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto updateDto)
    {
        try
        {
            _logger.LogDebug("Updating category with ID {Id}", updateDto.Id);

            // Validate the category
            var validationResult = await ValidateCategoryAsync(_mapper.Map<CategoryDto>(updateDto));
            if (!validationResult.IsValid)
            {
                throw new ValidationException("Category validation failed", validationResult.Errors);
            }

            var existingCategory = await _unitOfWork.Categories.GetByIdAsync(updateDto.Id);
            if (existingCategory == null)
            {
                throw new EntityNotFoundException($"Category with ID {updateDto.Id} not found");
            }

            // Check if name is being changed and if it conflicts
            if (updateDto.Name != existingCategory.Name &&
                await _unitOfWork.Categories.CategoryNameExistsAsync(updateDto.Name, updateDto.Id))
            {
                throw new DuplicateEntityException(nameof(Category), "Name", updateDto.Name);
            }

            var updatedCategory = _mapper.Map(updateDto, existingCategory);

            // Use transaction for atomic operation
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Categories.UpdateAsync(updatedCategory);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Invalidate relevant caches
                InvalidateCategoryCaches();

                var categoryDto = _mapper.Map<CategoryDto>(updatedCategory);
                _logger.LogInformation("Successfully updated category with ID {Id}", updateDto.Id);

                return categoryDto;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (EntityNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating category with ID {Id}", updateDto.Id);
            throw new ServiceException($"Failed to update category with ID {updateDto.Id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            _logger.LogDebug("Deleting category with ID {Id}", id);

            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            // Use transaction for atomic operation
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Categories.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Invalidate relevant caches
                InvalidateCategoryCaches();

                _logger.LogInformation("Successfully deleted category with ID {Id}", id);
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting category with ID {Id}", id);
            throw new ServiceException($"Failed to delete category with ID {id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateDisplayOrdersAsync(Dictionary<int, int> categoryOrders)
    {
        try
        {
            _logger.LogDebug("Updating display orders for {Count} categories", categoryOrders.Count);

            // Use transaction for atomic operation
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Categories.UpdateDisplayOrdersAsync(categoryOrders);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Invalidate relevant caches
                InvalidateCategoryCaches();

                _logger.LogInformation("Successfully updated display orders for {Count} categories", categoryOrders.Count);
                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating display orders");
            throw new ServiceException("Failed to update display orders", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryDto>> GetCategoriesWithProductCountsAsync(bool includeInactive = false)
    {
        try
        {
            var cacheKey = $"CategoriesWithCounts_{includeInactive}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<CategoryDto>? cachedCategories))
            {
                _logger.LogDebug("Returning cached categories with product counts (includeInactive: {IncludeInactive})", includeInactive);
                return cachedCategories!;
            }

            var categories = await _unitOfWork.Categories.GetWithProductCountsAsync(includeInactive);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            // Cache for 5 minutes
            _cache.Set(cacheKey, categoryDtos, TimeSpan.FromMinutes(5));

            _logger.LogDebug("Retrieved {Count} categories with product counts", categoryDtos.Count());
            return categoryDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting categories with product counts");
            throw new ServiceException("Failed to get categories with product counts", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryDto>> GetCategoriesWithProductsAsync(bool includeInactive = false)
    {
        try
        {
            _logger.LogDebug("Getting categories with products (includeInactive: {IncludeInactive})", includeInactive);

            var categories = await _unitOfWork.Categories.GetCategoriesWithProductsAsync(includeInactive);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            _logger.LogDebug("Found {Count} categories with products", categoryDtos.Count());
            return categoryDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting categories with products");
            throw new ServiceException("Failed to get categories with products", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryDto>> GetEmptyCategoriesAsync(bool includeInactive = false)
    {
        try
        {
            _logger.LogDebug("Getting empty categories (includeInactive: {IncludeInactive})", includeInactive);

            var categories = await _unitOfWork.Categories.GetEmptyCategoriesAsync(includeInactive);
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

            _logger.LogDebug("Found {Count} empty categories", categoryDtos.Count());
            return categoryDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting empty categories");
            throw new ServiceException("Failed to get empty categories", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateCategoryAsync(CategoryDto categoryDto)
    {
        var errors = new Dictionary<string, string[]>();
        var errorMessages = new List<string>();

        // Business validation rules
        if (string.IsNullOrWhiteSpace(categoryDto.Name))
            errorMessages.Add("Category name is required");

        if (categoryDto.Name?.Length > 100)
            errorMessages.Add("Category name cannot exceed 100 characters");

        if (categoryDto.Description?.Length > 500)
            errorMessages.Add("Description cannot exceed 500 characters");

        if (categoryDto.Icon?.Length > 200)
            errorMessages.Add("Icon cannot exceed 200 characters");

        if (categoryDto.DisplayOrder < 0)
            errorMessages.Add("Display order must be non-negative");

        if (errorMessages.Any())
        {
            errors["General"] = errorMessages.ToArray();
        }

        return new ValidationResult(!errors.Any(), errors);
    }

    /// <inheritdoc />
    public async Task<(int TotalCategories, int ActiveCategories, int CategoriesWithProducts, int EmptyCategories)> GetCategoryStatisticsAsync()
    {
        try
        {
            if (_cache.TryGetValue(CategoryStatisticsCacheKey, out var cachedStats))
            {
                _logger.LogDebug("Returning cached category statistics");
                return ((int TotalCategories, int ActiveCategories, int CategoriesWithProducts, int EmptyCategories))cachedStats!;
            }

            var stats = await _unitOfWork.Categories.GetCategoryStatisticsAsync();

            // Cache for 15 minutes
            _cache.Set(CategoryStatisticsCacheKey, stats, TimeSpan.FromMinutes(15));

            _logger.LogDebug("Retrieved category statistics: Total={Total}, Active={Active}, WithProducts={WithProducts}, Empty={Empty}",
                stats.TotalCategories, stats.ActiveCategories, stats.CategoriesWithProducts, stats.EmptyCategories);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting category statistics");
            throw new ServiceException("Failed to get category statistics", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
    {
        try
        {
            _logger.LogDebug("Checking if category name {Name} exists (excludeId: {ExcludeId})", name, excludeId);

            var exists = await _unitOfWork.Categories.CategoryNameExistsAsync(name, excludeId);

            _logger.LogDebug("Category name {Name} exists: {Exists}", name, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if category name {Name} exists", name);
            throw new ServiceException($"Failed to check if category name {name} exists", ex);
        }
    }

    /// <summary>
    /// Invalidates category-related caches.
    /// </summary>
    private void InvalidateCategoryCaches()
    {
        _cache.Remove(CategoriesCacheKey);
        _cache.Remove(CategoryStatisticsCacheKey);

        // Note: Individual category caches would be invalidated here
        // For simplicity, we're removing all category-related caches
    }
}
