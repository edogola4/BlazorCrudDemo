using System.ComponentModel;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Shared.DTOs;

/// <summary>
/// Data Transfer Object for Product display.
/// </summary>
public class ProductDto : INotifyPropertyChanged
{
    private int _id;
    private string? _name;
    private string? _description;
    private decimal _price;
    private int _stock;
    private string? _sku;
    private string? _imageUrl;
    private int _categoryId;
    private Category? _category;
    private DateTime _createdDate;
    private DateTime _modifiedDate;
    private bool _isActive;

    /// <summary>
    /// Unique identifier for the product.
    /// </summary>
    public int Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
    }

    /// <summary>
    /// Name of the product.
    /// </summary>
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
    public decimal Price
    {
        get => _price;
        set
        {
            if (_price != value)
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(FormattedPrice));
            }
        }
    }

    /// <summary>
    /// Stock quantity of the product.
    /// </summary>
    public int Stock
    {
        get => _stock;
        set
        {
            if (_stock != value)
            {
                _stock = value;
                OnPropertyChanged(nameof(Stock));
                OnPropertyChanged(nameof(IsInStock));
                OnPropertyChanged(nameof(StockStatus));
            }
        }
    }

    /// <summary>
    /// Stock quantity of the product (alias for Stock).
    /// </summary>
    public int StockQuantity => Stock;

    /// <summary>
    /// Original price of the product before any discounts.
    /// </summary>
    public decimal OriginalPrice { get; set; } = 0;

    /// <summary>
    /// Tags associated with the product.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Average rating of the product (1-5).
    /// </summary>
    public decimal Rating { get; set; } = 0;

    /// <summary>
    /// Number of reviews for the product.
    /// </summary>
    public int ReviewCount { get; set; } = 0;

    /// <summary>
    /// Stock Keeping Unit (SKU) for the product.
    /// </summary>
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
    public Category? Category
    {
        get => _category;
        set
        {
            if (_category != value)
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(CategoryName));
            }
        }
    }

    /// <summary>
    /// Name of the category this product belongs to.
    /// </summary>
    public string? CategoryName => Category?.Name;

    /// <summary>
    /// Date and time when the product was created.
    /// </summary>
    public DateTime CreatedDate
    {
        get => _createdDate;
        set
        {
            if (_createdDate != value)
            {
                _createdDate = value;
                OnPropertyChanged(nameof(CreatedDate));
            }
        }
    }

    /// <summary>
    /// Date and time when the product was last modified.
    /// </summary>
    public DateTime ModifiedDate
    {
        get => _modifiedDate;
        set
        {
            if (_modifiedDate != value)
            {
                _modifiedDate = value;
                OnPropertyChanged(nameof(ModifiedDate));
            }
        }
    }

    /// <summary>
    /// Indicates whether the product is active.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the product is in stock.
    /// </summary>
    public bool IsInStock => Stock > 0;

    /// <summary>
    /// Gets the formatted price as currency.
    /// </summary>
    public string FormattedPrice => Price.ToString("C");

    /// <summary>
    /// Gets the stock status display text.
    /// </summary>
    public string StockStatus => IsInStock ? $"{Stock} in stock" : "Out of stock";

    /// <summary>
    /// Gets the availability status for display.
    /// </summary>
    public string AvailabilityStatus => IsActive ? "Active" : "Inactive";

    /// <summary>
    /// Gets a truncated description for display purposes.
    /// </summary>
    public string? TruncatedDescription => Description?.Length > 100
        ? Description.Substring(0, 100) + "..."
        : Description;

    /// <summary>
    /// Event raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Creates a ProductDto from a Product entity.
    /// </summary>
    /// <param name="product">The product entity.</param>
    /// <returns>A new ProductDto instance.</returns>
    public static ProductDto FromProduct(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            SKU = product.SKU,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            Category = product.Category,
            CreatedDate = product.CreatedDate,
            ModifiedDate = product.ModifiedDate,
            IsActive = product.IsActive,
            OriginalPrice = product.OriginalPrice,
            Tags = product.Tags?.ToList(),
            Rating = product.Rating,
            ReviewCount = product.ReviewCount
        };
    }
}
