using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorCrudDemo.Shared.Models;

/// <summary>
/// Represents a product in the system.
/// </summary>
public class Product : BaseEntity
{
    private string? _name;
    private string? _description;
    private decimal _price;
    private int _stock;
    private string? _sku;
    private string? _imageUrl;
    private int _categoryId;
    private Category? _category;

    /// <summary>
    /// Name of the product.
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    public string? Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    /// <summary>
    /// Description of the product.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }

    /// <summary>
    /// Price of the product.
    /// </summary>
    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price
    {
        get => _price;
        set
        {
            if (_price != value)
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }
    }

    /// <summary>
    /// Stock quantity of the product.
    /// </summary>
    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public int Stock
    {
        get => _stock;
        set
        {
            if (_stock != value)
            {
                _stock = value;
                OnPropertyChanged(nameof(Stock));
            }
        }
    }

    /// <summary>
    /// Stock Keeping Unit (SKU) for the product.
    /// </summary>
    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
    public string? SKU
    {
        get => _sku;
        set
        {
            if (_sku != value)
            {
                _sku = value;
                OnPropertyChanged(nameof(SKU));
            }
        }
    }

    /// <summary>
    /// URL of the product image.
    /// </summary>
    [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? ImageUrl
    {
        get => _imageUrl;
        set
        {
            if (_imageUrl != value)
            {
                _imageUrl = value;
                OnPropertyChanged(nameof(ImageUrl));
            }
        }
    }

    /// <summary>
    /// ID of the category this product belongs to.
    /// </summary>
    [Required(ErrorMessage = "Category is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
    public int CategoryId
    {
        get => _categoryId;
        set
        {
            if (_categoryId != value)
            {
                _categoryId = value;
                OnPropertyChanged(nameof(CategoryId));
            }
        }
    }

    /// <summary>
    /// Category this product belongs to.
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public Category? Category
    {
        get => _category;
        set
        {
            if (_category != value)
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the product is in stock.
    /// </summary>
    [NotMapped]
    public bool IsInStock => Stock > 0;

    /// <summary>
    /// Gets the formatted price as currency.
    /// </summary>
    [NotMapped]
    public string FormattedPrice => Price.ToString("C");

    /// <summary>
    /// Gets the stock status display text.
    /// </summary>
    [NotMapped]
    public string StockStatus => IsInStock ? $"{Stock} in stock" : "Out of stock";
}
