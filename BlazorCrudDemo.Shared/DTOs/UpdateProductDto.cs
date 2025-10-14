using System.ComponentModel.DataAnnotations;

namespace BlazorCrudDemo.Shared.DTOs;

/// <summary>
/// Data Transfer Object for updating an existing product.
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Unique identifier for the product to update.
    /// </summary>
    [Required(ErrorMessage = "Product ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid product ID")]
    public int Id { get; set; }

    /// <summary>
    /// Name of the product.
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// Description of the product.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Price of the product.
    /// </summary>
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
    public decimal Price { get; set; }

    /// <summary>
    /// Stock quantity of the product.
    /// </summary>
    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int Stock { get; set; }

    /// <summary>
    /// Stock Keeping Unit (SKU) for the product.
    /// </summary>
    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
    public string? SKU { get; set; }

    /// <summary>
    /// URL of the product image.
    /// </summary>
    [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// ID of the category this product belongs to.
    /// </summary>
    [Required(ErrorMessage = "Category is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
    public int CategoryId { get; set; }

    /// <summary>
    /// Indicates whether the product is active.
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    /// Validates the DTO for required fields and business rules.
    /// </summary>
    /// <returns>A collection of validation results.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // Validate that ID is provided and valid
        if (Id <= 0)
        {
            results.Add(new ValidationResult(
                "Valid product ID is required for update operation.",
                new[] { nameof(Id) }));
        }

        // Validate that price is positive
        if (Price <= 0)
        {
            results.Add(new ValidationResult(
                "Price must be greater than zero.",
                new[] { nameof(Price) }));
        }

        // Validate that SKU is not empty
        if (string.IsNullOrWhiteSpace(SKU))
        {
            results.Add(new ValidationResult(
                "SKU is required and cannot be empty.",
                new[] { nameof(SKU) }));
        }

        // Validate that stock is not negative
        if (Stock < 0)
        {
            results.Add(new ValidationResult(
                "Stock quantity cannot be negative.",
                new[] { nameof(Stock) }));
        }

        return results;
    }

    /// <summary>
    /// Creates an UpdateProductDto from a Product entity.
    /// </summary>
    /// <param name="product">The product entity.</param>
    /// <returns>A new UpdateProductDto instance.</returns>
    public static UpdateProductDto FromProduct(BlazorCrudDemo.Shared.Models.Product product)
    {
        return new UpdateProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            SKU = product.SKU,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive
        };
    }
}
