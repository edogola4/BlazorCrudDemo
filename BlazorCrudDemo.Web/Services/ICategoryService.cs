using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Shared.Exceptions;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// Service interface for category business logic operations.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets all categories.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories.</param>
    /// <returns>Collection of category DTOs.</returns>
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync(bool includeInactive = false);

    /// <summary>
    /// Gets a category by ID with optional product inclusion.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="includeProducts">Whether to include products.</param>
    /// <returns>The category DTO or null if not found.</returns>
    Task<CategoryDto?> GetCategoryAsync(int id, bool includeProducts = false);

    /// <summary>
    /// Gets a category by name.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <returns>The category DTO or null if not found.</returns>
    Task<CategoryDto?> GetCategoryByNameAsync(string name);

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="createDto">The category creation data.</param>
    /// <returns>The created category DTO.</returns>
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createDto);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="updateDto">The category update data.</param>
    /// <returns>The updated category DTO.</returns>
    Task<CategoryDto> UpdateCategoryAsync(UpdateCategoryDto updateDto);

    /// <summary>
    /// Deletes a category by ID.
    /// </summary>
    /// <param name="id">The category ID to delete.</param>
    /// <returns>True if deleted successfully.</returns>
    Task<bool> DeleteCategoryAsync(int id);

    /// <summary>
    /// Updates the display order of categories.
    /// </summary>
    /// <param name="categoryOrders">Dictionary mapping category ID to new display order.</param>
    /// <returns>True if updated successfully.</returns>
    Task<bool> UpdateDisplayOrdersAsync(Dictionary<int, int> categoryOrders);

    /// <summary>
    /// Gets categories with product counts.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories.</param>
    /// <returns>Categories with product count information.</returns>
    Task<IEnumerable<CategoryDto>> GetCategoriesWithProductCountsAsync(bool includeInactive = false);

    /// <summary>
    /// Gets categories that have products.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories.</param>
    /// <returns>Categories that contain products.</returns>
    Task<IEnumerable<CategoryDto>> GetCategoriesWithProductsAsync(bool includeInactive = false);

    /// <summary>
    /// Gets categories that have no products.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories.</param>
    /// <returns>Categories without any products.</returns>
    Task<IEnumerable<CategoryDto>> GetEmptyCategoriesAsync(bool includeInactive = false);

    /// <summary>
    /// Validates category data for business rules.
    /// </summary>
    /// <param name="categoryDto">The category data to validate.</param>
    /// <returns>Validation result with any errors.</returns>
    Task<ValidationResult> ValidateCategoryAsync(CategoryDto categoryDto);

    /// <summary>
    /// Gets category statistics.
    /// </summary>
    /// <returns>Category statistics.</returns>
    Task<(int TotalCategories, int ActiveCategories, int CategoriesWithProducts, int EmptyCategories)> GetCategoryStatisticsAsync();

    /// <summary>
    /// Checks if a category name already exists.
    /// </summary>
    /// <param name="name">The category name to check.</param>
    /// <param name="excludeId">Optional category ID to exclude from check (for updates).</param>
    /// <returns>True if the name exists, false otherwise.</returns>
    Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null);
}

/// <summary>
/// DTO for category creation.
/// </summary>
public class CreateCategoryDto
{
    /// <summary>
    /// Category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon class or URL.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for category updates.
/// </summary>
public class UpdateCategoryDto
{
    /// <summary>
    /// Category ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon class or URL.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether the category is active.
    /// </summary>
    public bool IsActive { get; set; }
}
