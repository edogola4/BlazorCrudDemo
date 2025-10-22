using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Web.Services;
using BlazorCrudDemo.Shared.Exceptions;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// Service for business logic validation.
/// </summary>
public interface IBusinessValidationService
{
    /// <summary>
    /// Validates product creation data.
    /// </summary>
    /// <param name="createDto">The product creation data.</param>
    /// <returns>Validation result with errors if any.</returns>
    Task<BusinessValidationResult> ValidateProductCreationAsync(CreateProductDto createDto);

    /// <summary>
    /// Validates product update data.
    /// </summary>
    /// <param name="updateDto">The product update data.</param>
    /// <param name="existingProductId">The existing product ID.</param>
    /// <returns>Validation result with errors if any.</returns>
    Task<BusinessValidationResult> ValidateProductUpdateAsync(UpdateProductDto updateDto, int existingProductId);

    /// <summary>
    /// Validates category creation data.
    /// </summary>
    /// <param name="createDto">The category creation data.</param>
    /// <returns>Validation result with errors if any.</returns>
    Task<BusinessValidationResult> ValidateCategoryCreationAsync(CreateCategoryDto createDto);

    /// <summary>
    /// Validates category update data.
    /// </summary>
    /// <param name="updateDto">The category update data.</param>
    /// <param name="existingCategoryId">The existing category ID.</param>
    /// <returns>Validation result with errors if any.</returns>
    Task<BusinessValidationResult> ValidateCategoryUpdateAsync(UpdateCategoryDto updateDto, int existingCategoryId);

    /// <summary>
    /// Validates stock update operation.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="newStock">The new stock quantity.</param>
    /// <returns>Validation result with errors if any.</returns>
    Task<BusinessValidationResult> ValidateStockUpdateAsync(int productId, int newStock);

    /// <summary>
    /// Validates bulk operations.
    /// </summary>
    /// <param name="operation">The operation type.</param>
    /// <param name="items">The items to validate.</param>
    /// <returns>Validation result with errors if any.</returns>
    Task<BusinessValidationResult> ValidateBulkOperationAsync(string operation, IEnumerable<object> items);
}

/// <summary>
/// Business validation result.
/// </summary>
public class BusinessValidationResult
{
    /// <summary>
    /// Whether the validation passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation errors.
    /// </summary>
    public List<BusinessValidationError> Errors { get; set; } = new();

    /// <summary>
    /// List of validation warnings.
    /// </summary>
    public List<BusinessValidationWarning> Warnings { get; set; } = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful validation result.</returns>
    public static BusinessValidationResult Success()
    {
        return new BusinessValidationResult { IsValid = true };
    }

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>A failed validation result.</returns>
    public static BusinessValidationResult Failure(params BusinessValidationError[] errors)
    {
        return new BusinessValidationResult
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }

    /// <summary>
    /// Creates a validation result with warnings.
    /// </summary>
    /// <param name="warnings">The validation warnings.</param>
    /// <returns>A validation result with warnings.</returns>
    public static BusinessValidationResult WithWarnings(params BusinessValidationWarning[] warnings)
    {
        return new BusinessValidationResult
        {
            IsValid = true,
            Warnings = warnings.ToList()
        };
    }

    /// <summary>
    /// Adds an error to the validation result.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="message">The error message.</param>
    public void AddError(string field, string message)
    {
        Errors.Add(new BusinessValidationError(field, message));
        IsValid = false;
    }

    /// <summary>
    /// Adds a warning to the validation result.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="message">The warning message.</param>
    public void AddWarning(string field, string message)
    {
        Warnings.Add(new BusinessValidationWarning(field, message));
    }

    /// <summary>
    /// Merges another validation result into this one.
    /// </summary>
    /// <param name="other">The other validation result.</param>
    public void Merge(BusinessValidationResult other)
    {
        Errors.AddRange(other.Errors);
        Warnings.AddRange(other.Warnings);
        IsValid = IsValid && other.IsValid;
    }
}

/// <summary>
/// Business validation error.
/// </summary>
public class BusinessValidationError
{
    /// <summary>
    /// The field name that has the error.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The error code.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the BusinessValidationError class.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="message">The error message.</param>
    public BusinessValidationError(string field, string message)
    {
        Field = field;
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the BusinessValidationError class.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    public BusinessValidationError(string field, string message, string errorCode)
    {
        Field = field;
        Message = message;
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Business validation warning.
/// </summary>
public class BusinessValidationWarning
{
    /// <summary>
    /// The field name that has the warning.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// The warning message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The warning code.
    /// </summary>
    public string? WarningCode { get; set; }

    /// <summary>
    /// Initializes a new instance of the BusinessValidationWarning class.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="message">The warning message.</param>
    public BusinessValidationWarning(string field, string message)
    {
        Field = field;
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the BusinessValidationWarning class.
    /// </summary>
    /// <param name="field">The field name.</param>
    /// <param name="message">The warning message.</param>
    /// <param name="warningCode">The warning code.</param>
    public BusinessValidationWarning(string field, string message, string warningCode)
    {
        Field = field;
        Message = message;
        WarningCode = warningCode;
    }
}

/// <summary>
/// Service implementation for business validation.
/// </summary>
public class BusinessValidationService : IBusinessValidationService
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    /// <summary>
    /// Initializes a new instance of the BusinessValidationService class.
    /// </summary>
    /// <param name="productService">The product service.</param>
    /// <param name="categoryService">The category service.</param>
    public BusinessValidationService(
        IProductService productService,
        ICategoryService categoryService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
    }

    /// <inheritdoc />
    public async Task<BusinessValidationResult> ValidateProductCreationAsync(CreateProductDto createDto)
    {
        var result = BusinessValidationResult.Success();

        // Basic validation
        if (string.IsNullOrWhiteSpace(createDto.Name))
        {
            result.AddError(nameof(createDto.Name), "Product name is required");
        }
        else if (createDto.Name.Length > 200)
        {
            result.AddError(nameof(createDto.Name), "Product name cannot exceed 200 characters");
        }

        if (createDto.Price <= 0)
        {
            result.AddError(nameof(createDto.Price), "Price must be greater than zero");
        }
        else if (createDto.Price > 999999.99m)
        {
            result.AddError(nameof(createDto.Price), "Price cannot exceed 999,999.99");
        }

        if (createDto.Stock < 0)
        {
            result.AddError(nameof(createDto.Stock), "Stock quantity cannot be negative");
        }

        if (string.IsNullOrWhiteSpace(createDto.SKU))
        {
            result.AddError(nameof(createDto.SKU), "SKU is required");
        }
        else if (createDto.SKU.Length > 50)
        {
            result.AddError(nameof(createDto.SKU), "SKU cannot exceed 50 characters");
        }

        if (createDto.CategoryId <= 0)
        {
            result.AddError(nameof(createDto.CategoryId), "Valid category must be selected");
        }

        // Business rule validation
        if (createDto.Price > 1000)
        {
            result.AddWarning(nameof(createDto.Price), "High-value product - consider special handling");
        }

        if (createDto.Stock == 0)
        {
            result.AddWarning(nameof(createDto.Stock), "Product will be out of stock");
        }

        if (createDto.Name?.ToLower().Contains("test") == true)
        {
            result.AddWarning(nameof(createDto.Name), "Product name contains 'test' - consider if this is intentional");
        }

        // SKU format validation
        if (!string.IsNullOrWhiteSpace(createDto.SKU) && !IsValidSkuFormat(createDto.SKU))
        {
            result.AddError(nameof(createDto.SKU), "SKU must follow the format: XXX-XXX-XXX (3 letters, dash, 3 letters, dash, 3 digits)");
        }

        await Task.CompletedTask;
        return result;
    }

    /// <inheritdoc />
    public async Task<BusinessValidationResult> ValidateProductUpdateAsync(UpdateProductDto updateDto, int existingProductId)
    {
        var result = BusinessValidationResult.Success();

        // Basic validation
        if (string.IsNullOrWhiteSpace(updateDto.Name))
        {
            result.AddError(nameof(updateDto.Name), "Product name is required");
        }
        else if (updateDto.Name.Length > 200)
        {
            result.AddError(nameof(updateDto.Name), "Product name cannot exceed 200 characters");
        }

        if (updateDto.Price <= 0)
        {
            result.AddError(nameof(updateDto.Price), "Price must be greater than zero");
        }
        else if (updateDto.Price > 999999.99m)
        {
            result.AddError(nameof(updateDto.Price), "Price cannot exceed 999,999.99");
        }

        if (updateDto.Stock < 0)
        {
            result.AddError(nameof(updateDto.Stock), "Stock quantity cannot be negative");
        }

        if (string.IsNullOrWhiteSpace(updateDto.SKU))
        {
            result.AddError(nameof(updateDto.SKU), "SKU is required");
        }
        else if (updateDto.SKU.Length > 50)
        {
            result.AddError(nameof(updateDto.SKU), "SKU cannot exceed 50 characters");
        }

        if (updateDto.CategoryId <= 0)
        {
            result.AddError(nameof(updateDto.CategoryId), "Valid category must be selected");
        }

        // SKU format validation
        if (!string.IsNullOrWhiteSpace(updateDto.SKU) && !IsValidSkuFormat(updateDto.SKU))
        {
            result.AddError(nameof(updateDto.SKU), "SKU must follow the format: XXX-XXX-XXX (3 letters, dash, 3 letters, dash, 3 digits)");
        }

        // Price change validation
        if (updateDto.Price > 500 && updateDto.Price > await GetCurrentProductPrice(existingProductId) * 2)
        {
            result.AddWarning(nameof(updateDto.Price), "Significant price increase detected");
        }

        // Stock change validation
        var currentStock = await GetCurrentProductStock(existingProductId);
        if (updateDto.Stock == 0 && currentStock > 0)
        {
            result.AddWarning(nameof(updateDto.Stock), "Product will be marked as out of stock");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<BusinessValidationResult> ValidateCategoryCreationAsync(CreateCategoryDto createDto)
    {
        var result = BusinessValidationResult.Success();

        // Basic validation
        if (string.IsNullOrWhiteSpace(createDto.Name))
        {
            result.AddError(nameof(createDto.Name), "Category name is required");
        }
        else if (createDto.Name.Length > 100)
        {
            result.AddError(nameof(createDto.Name), "Category name cannot exceed 100 characters");
        }

        if (createDto.Description?.Length > 500)
        {
            result.AddError(nameof(createDto.Description), "Description cannot exceed 500 characters");
        }

        if (createDto.Icon?.Length > 200)
        {
            result.AddError(nameof(createDto.Icon), "Icon cannot exceed 200 characters");
        }

        if (createDto.DisplayOrder < 0)
        {
            result.AddError(nameof(createDto.DisplayOrder), "Display order must be non-negative");
        }

        // Category name uniqueness
        if (await _categoryService.CategoryNameExistsAsync(createDto.Name))
        {
            result.AddError(nameof(createDto.Name), "Category name already exists");
        }

        // Icon format validation
        if (!string.IsNullOrWhiteSpace(createDto.Icon) && !IsValidIconFormat(createDto.Icon))
        {
            result.AddError(nameof(createDto.Icon), "Icon must be a valid CSS class or URL");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<BusinessValidationResult> ValidateCategoryUpdateAsync(UpdateCategoryDto updateDto, int existingCategoryId)
    {
        var result = BusinessValidationResult.Success();

        // Basic validation
        if (string.IsNullOrWhiteSpace(updateDto.Name))
        {
            result.AddError(nameof(updateDto.Name), "Category name is required");
        }
        else if (updateDto.Name.Length > 100)
        {
            result.AddError(nameof(updateDto.Name), "Category name cannot exceed 100 characters");
        }

        if (updateDto.Description?.Length > 500)
        {
            result.AddError(nameof(updateDto.Description), "Description cannot exceed 500 characters");
        }

        if (updateDto.Icon?.Length > 200)
        {
            result.AddError(nameof(updateDto.Icon), "Icon cannot exceed 200 characters");
        }

        if (updateDto.DisplayOrder < 0)
        {
            result.AddError(nameof(updateDto.DisplayOrder), "Display order must be non-negative");
        }

        // Category name uniqueness (excluding current category)
        if (await _categoryService.CategoryNameExistsAsync(updateDto.Name, existingCategoryId))
        {
            result.AddError(nameof(updateDto.Name), "Category name already exists");
        }

        // Icon format validation
        if (!string.IsNullOrWhiteSpace(updateDto.Icon) && !IsValidIconFormat(updateDto.Icon))
        {
            result.AddError(nameof(updateDto.Icon), "Icon must be a valid CSS class or URL");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<BusinessValidationResult> ValidateStockUpdateAsync(int productId, int newStock)
    {
        var result = BusinessValidationResult.Success();

        if (newStock < 0)
        {
            result.AddError("Stock", "Stock quantity cannot be negative");
        }

        if (newStock == 0)
        {
            result.AddWarning("Stock", "Product will be marked as out of stock");
        }

        if (newStock > 10000)
        {
            result.AddWarning("Stock", "Very high stock quantity - consider warehouse capacity");
        }

        // Check if product exists
        var product = await _productService.GetProductAsync(productId);
        if (product == null)
        {
            result.AddError("Product", "Product not found");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<BusinessValidationResult> ValidateBulkOperationAsync(string operation, IEnumerable<object> items)
    {
        var result = BusinessValidationResult.Success();

        if (!items.Any())
        {
            result.AddError("Items", "No items provided for bulk operation");
            return result;
        }

        var itemCount = items.Count();
        if (itemCount > 100)
        {
            result.AddWarning("Items", $"Large bulk operation ({itemCount} items) - this may take some time");
        }

        // Additional validation based on operation type
        switch (operation.ToLower())
        {
            case "delete":
                result.AddWarning("Operation", "Bulk delete operation - ensure proper backup exists");
                break;
            case "priceupdate":
                result.AddWarning("Operation", "Bulk price update - consider market impact");
                break;
            case "stockupdate":
                result.AddWarning("Operation", "Bulk stock update - verify inventory system capacity");
                break;
        }

        await Task.CompletedTask;
        return result;
    }

    /// <summary>
    /// Validates SKU format.
    /// </summary>
    /// <param name="sku">The SKU to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    private bool IsValidSkuFormat(string sku)
    {
        // Expected format: XXX-XXX-XXX (3 letters, dash, 3 letters, dash, 3 digits)
        var pattern = @"^[A-Z]{3}-[A-Z]{3}-\d{3}$";
        return System.Text.RegularExpressions.Regex.IsMatch(sku, pattern);
    }

    /// <summary>
    /// Validates icon format.
    /// </summary>
    /// <param name="icon">The icon to validate.</param>
    /// <returns>True if valid, false otherwise.</returns>
    private bool IsValidIconFormat(string icon)
    {
        // Valid formats: CSS class (e.g., "fas fa-home") or URL (e.g., "https://example.com/icon.png")
        return icon.StartsWith("fa") || icon.StartsWith("http");
    }

    /// <summary>
    /// Gets the current price of a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>The current price.</returns>
    private async Task<decimal> GetCurrentProductPrice(int productId)
    {
        var product = await _productService.GetProductAsync(productId);
        return product?.Price ?? 0;
    }

    /// <summary>
    /// Gets the current stock of a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>The current stock.</returns>
    private async Task<int> GetCurrentProductStock(int productId)
    {
        var product = await _productService.GetProductAsync(productId);
        return product?.Stock ?? 0;
    }
}
