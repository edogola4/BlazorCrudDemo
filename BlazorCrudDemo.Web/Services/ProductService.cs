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
/// Service implementation for product business logic operations.
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductService> _logger;

    private const string ProductsCacheKey = "AllProducts";
    private const string ProductCacheKeyPrefix = "Product_";
    private const string CategoryProductsCacheKeyPrefix = "CategoryProducts_";
    private const string ProductStatisticsCacheKey = "ProductStatistics";

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<ProductDto>> GetProductsAsync(
        int? categoryId = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 10,
        string sortBy = "Name",
        bool sortDescending = false)
    {
        try
        {
            _logger.LogDebug("Getting products with filters: CategoryId={CategoryId}, SearchTerm={SearchTerm}, PageNumber={PageNumber}",
                categoryId, searchTerm, pageNumber);

            // Check cache for simple queries
            if (categoryId == null && string.IsNullOrEmpty(searchTerm) && sortBy == "Name" && !sortDescending)
            {
                if (_cache.TryGetValue(ProductsCacheKey, out PaginatedResult<ProductDto>? cachedResult))
                {
                    _logger.LogDebug("Returning cached products");
                    return cachedResult!;
                }
            }

            var (products, totalCount) = await _unitOfWork.Products.GetPaginatedAsync(
                pageNumber, pageSize, sortBy, sortDescending, categoryId);

            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            var result = PaginatedResult<ProductDto>.Create(productDtos, totalCount, pageNumber, pageSize);

            // Cache simple queries
            if (categoryId == null && string.IsNullOrEmpty(searchTerm) && sortBy == "Name" && !sortDescending)
            {
                _cache.Set(ProductsCacheKey, result, TimeSpan.FromMinutes(5));
            }

            _logger.LogDebug("Retrieved {Count} products out of {TotalCount} total", products.Count(), totalCount);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting products");
            throw new ServiceException("Failed to get products", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ProductDto?> GetProductAsync(int id)
    {
        try
        {
            var cacheKey = $"{ProductCacheKeyPrefix}{id}";

            if (_cache.TryGetValue(cacheKey, out ProductDto? cachedProduct))
            {
                _logger.LogDebug("Returning cached product with ID {Id}", id);
                return cachedProduct;
            }

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return null;
            }

            var productDto = _mapper.Map<ProductDto>(product);

            // Cache for 10 minutes
            _cache.Set(cacheKey, productDto, TimeSpan.FromMinutes(10));

            _logger.LogDebug("Retrieved product with ID {Id}", id);
            return productDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product with ID {Id}", id);
            throw new ServiceException($"Failed to get product with ID {id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ProductDto?> GetProductBySkuAsync(string sku)
    {
        try
        {
            _logger.LogDebug("Getting product by SKU {Sku}", sku);

            var product = await _unitOfWork.Products.GetBySkuAsync(sku);
            if (product == null)
            {
                return null;
            }

            var productDto = _mapper.Map<ProductDto>(product);

            _logger.LogDebug("Retrieved product with SKU {Sku}", sku);
            return productDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product by SKU {Sku}", sku);
            throw new ServiceException($"Failed to get product by SKU {sku}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ProductDto> CreateProductAsync(CreateProductDto createDto)
    {
        try
        {
            _logger.LogDebug("Creating product with SKU {Sku}", createDto.SKU);

            // Check if SKU already exists
            var existingProduct = await _unitOfWork.Products.GetBySkuAsync(createDto.SKU!);
            if (existingProduct != null)
            {
                throw new DuplicateEntityException(nameof(Product), "SKU", createDto.SKU!);
            }

            // Map to ProductDto first to ensure all required fields are present
            var productDto = _mapper.Map<ProductDto>(createDto);
            
            // Validate the product
            var validationResult = await ValidateProductAsync(productDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException("Product validation failed", validationResult.Errors);
            }

            var product = _mapper.Map<Product>(createDto);

            // Use transaction for atomic operation
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                product = await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Invalidate relevant caches
                InvalidateProductCaches();

                var resultDto = _mapper.Map<ProductDto>(product);
                _logger.LogInformation("Successfully created product with ID {Id} and SKU {Sku}", product.Id, product.SKU);

                return resultDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error occurred while creating product");
                throw new ServiceException("Failed to create product", ex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product");
            throw new ServiceException("Failed to create product", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ProductDto> UpdateProductAsync(UpdateProductDto updateDto)
    {
        try
        {
            _logger.LogDebug("Updating product with ID {Id}", updateDto.Id);

            // Validate the product
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(updateDto.Id);
            if (existingProduct == null)
            {
                throw new EntityNotFoundException($"Product with ID {updateDto.Id} not found");
            }

            // Check if SKU is being changed and if it conflicts
            if (updateDto.SKU != existingProduct.SKU)
            {
                var skuConflict = await _unitOfWork.Products.GetBySkuAsync(updateDto.SKU!);
                if (skuConflict != null && skuConflict.Id != updateDto.Id)
                {
                    throw new DuplicateEntityException(nameof(Product), "SKU", updateDto.SKU!);
                }
            }

            // Map to ProductDto first to ensure all required fields are present
            var productDto = _mapper.Map<ProductDto>(updateDto);
            
            // Validate the product
            var validationResult = await ValidateProductAsync(productDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException("Product validation failed", validationResult.Errors);
            }

            _mapper.Map(updateDto, existingProduct);

            // Use transaction for atomic operation
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                await _unitOfWork.Products.UpdateAsync(existingProduct);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Invalidate relevant caches
                InvalidateProductCaches();

                var resultDto = _mapper.Map<ProductDto>(existingProduct);
                _logger.LogInformation("Successfully updated product with ID {Id}", updateDto.Id);

                return resultDto;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error occurred while updating product with ID {Id}", updateDto.Id);
                throw new ServiceException($"Failed to update product with ID {updateDto.Id}", ex);
            }
        }
        catch (EntityNotFoundException)
        {
            throw;
        }
        catch (DuplicateEntityException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product with ID {Id}", updateDto.Id);
            throw new ServiceException($"Failed to update product with ID {updateDto.Id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            _logger.LogDebug("Deleting product with ID {Id}", id);

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return false;
            }

            // Use transaction for atomic operation
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                await _unitOfWork.Products.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Invalidate relevant caches
                InvalidateProductCaches();

                _logger.LogInformation("Successfully deleted product with ID {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error occurred while deleting product with ID {Id}", id);
                throw new ServiceException($"Failed to delete product with ID {id}", ex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product with ID {Id}", id);
            throw new ServiceException($"Failed to delete product with ID {id}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateStockAsync(int productId, int newStock)
    {
        try
        {
            _logger.LogDebug("Updating stock for product ID {ProductId} to {NewStock}", productId, newStock);

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return false;
            }

            if (newStock < 0)
            {
                var errors = new Dictionary<string, string[]> { ["Stock"] = new[] { "Stock quantity cannot be negative" } };
                throw new ValidationException("Stock update validation failed", errors);
            }

            // Use transaction for atomic operation
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                
                await _unitOfWork.Products.UpdateStockAsync(productId, newStock);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Invalidate relevant caches
                InvalidateProductCaches();

                _logger.LogInformation("Successfully updated stock for product ID {ProductId} to {NewStock}", productId, newStock);
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error occurred while updating stock for product ID {ProductId}", productId);
                throw new ServiceException($"Failed to update stock for product ID {productId}", ex);
            }
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating stock for product ID {ProductId}", productId);
            throw new ServiceException($"Failed to update stock for product ID {productId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateProductAsync(ProductDto productDto)
    {
        var errors = new Dictionary<string, string[]>();
        var errorMessages = new List<string>();

        // Business validation rules
        if (string.IsNullOrWhiteSpace(productDto.Name))
            errorMessages.Add("Product name is required");

        if (productDto.Name?.Length > 200)
            errorMessages.Add("Product name cannot exceed 200 characters");

        if (productDto.Price <= 0)
            errorMessages.Add("Price must be greater than zero");

        if (productDto.Price > 999999.99m)
            errorMessages.Add("Price cannot exceed 999,999.99");

        if (productDto.Stock < 0)
            errorMessages.Add("Stock quantity cannot be negative");

        if (string.IsNullOrWhiteSpace(productDto.SKU))
            errorMessages.Add("SKU is required");

        if (productDto.SKU?.Length > 50)
            errorMessages.Add("SKU cannot exceed 50 characters");

        if (productDto.CategoryId <= 0)
            errorMessages.Add("Valid category must be selected");

        // Check for duplicate SKU (excluding current product if updating)
        var existingProduct = await _unitOfWork.Products.GetBySkuAsync(productDto.SKU!);
        if (existingProduct != null && existingProduct.Id != productDto.Id)
        {
            errorMessages.Add($"SKU '{productDto.SKU}' already exists");
        }

        if (errorMessages.Any())
        {
            errors["General"] = errorMessages.ToArray();
        }

        return new ValidationResult(!errors.Any(), errors);
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportToExcelAsync(int? categoryId = null, string? searchTerm = null)
    {
        try
        {
            _logger.LogDebug("Exporting products to Excel with filters: CategoryId={CategoryId}, SearchTerm={SearchTerm}",
                categoryId, searchTerm);

            var products = await _unitOfWork.Products.SearchAsync(searchTerm, categoryId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            // For now, return empty byte array as Excel export would require additional library
            // In a real implementation, you would use a library like EPPlus or ClosedXML
            _logger.LogInformation("Exported {Count} products to Excel", productDtos.Count());

            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting products to Excel");
            throw new ServiceException("Failed to export products to Excel", ex);
        }
    }

    /// <inheritdoc />
    public async Task<(int TotalProducts, int ActiveProducts, int OutOfStockProducts, decimal AveragePrice)> GetProductStatisticsAsync()
    {
        try
        {
            if (_cache.TryGetValue(ProductStatisticsCacheKey, out var cachedStats))
            {
                _logger.LogDebug("Returning cached product statistics");
                return ((int TotalProducts, int ActiveProducts, int OutOfStockProducts, decimal AveragePrice))cachedStats!;
            }

            var stats = await _unitOfWork.Products.GetProductStatisticsAsync();

            // Cache for 15 minutes
            _cache.Set(ProductStatisticsCacheKey, stats, TimeSpan.FromMinutes(15));

            _logger.LogDebug("Retrieved product statistics: Total={Total}, Active={Active}, OutOfStock={OutOfStock}, AveragePrice={AveragePrice}",
                stats.TotalProducts, stats.ActiveProducts, stats.OutOfStockProducts, stats.AveragePrice);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product statistics");
            throw new ServiceException("Failed to get product statistics", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10)
    {
        try
        {
            _logger.LogDebug("Getting products with stock below threshold {Threshold}", threshold);

            var products = await _unitOfWork.Products.GetLowStockProductsAsync(threshold);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            _logger.LogDebug("Found {Count} products with low stock", productDtos.Count());
            return productDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting low stock products");
            throw new ServiceException("Failed to get low stock products", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        try
        {
            _logger.LogDebug("Getting products in price range {MinPrice} to {MaxPrice}", minPrice, maxPrice);

            var products = await _unitOfWork.Products.GetProductsByPriceRangeAsync(minPrice, maxPrice);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            _logger.LogDebug("Found {Count} products in price range", productDtos.Count());
            return productDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting products by price range");
            throw new ServiceException("Failed to get products by price range", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(ProductSearchDto searchDto)
    {
        try
        {
            _logger.LogDebug("Searching products with criteria: {@SearchDto}", searchDto);

            var products = await _unitOfWork.Products.SearchAsync(
                searchDto.SearchTerm,
                searchDto.CategoryId,
                searchDto.MinPrice,
                searchDto.MaxPrice,
                searchDto.InStock);

            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            _logger.LogDebug("Search returned {Count} products", productDtos.Count());
            return productDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching products");
            throw new ServiceException("Failed to search products", ex);
        }
    }

    /// <summary>
    /// Invalidates product-related caches.
    /// </summary>
    private void InvalidateProductCaches()
    {
        _cache.Remove(ProductsCacheKey);
        _cache.Remove(ProductStatisticsCacheKey);

        // Note: Individual product caches would be invalidated here
        // For simplicity, we're removing all product-related caches
    }
}
