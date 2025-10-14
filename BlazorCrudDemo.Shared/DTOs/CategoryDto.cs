using System.ComponentModel;
using BlazorCrudDemo.Shared.Models;

namespace BlazorCrudDemo.Shared.DTOs;

/// <summary>
/// Data Transfer Object for Category display.
/// </summary>
public class CategoryDto : INotifyPropertyChanged
{
    private int _id;
    private string? _name;
    private string? _description;
    private string? _icon;
    private int _displayOrder;
    private int _productCount;
    private int _activeProductCount;
    private DateTime _createdDate;
    private bool _isActive;

    /// <summary>
    /// Unique identifier for the category.
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
    /// Name of the category.
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
                OnPropertyChanged(nameof(DisplayNameWithCount));
            }
        }
    }

    /// <summary>
    /// Description of the category.
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
    /// Icon class or URL for the category.
    /// </summary>
    public string? Icon
    {
        get => _icon;
        set
        {
            if (_icon != value)
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }
    }

    /// <summary>
    /// Display order for sorting categories.
    /// </summary>
    public int DisplayOrder
    {
        get => _displayOrder;
        set
        {
            if (_displayOrder != value)
            {
                _displayOrder = value;
                OnPropertyChanged(nameof(DisplayOrder));
            }
        }
    }

    /// <summary>
    /// Total number of products in this category.
    /// </summary>
    public int ProductCount
    {
        get => _productCount;
        set
        {
            if (_productCount != value)
            {
                _productCount = value;
                OnPropertyChanged(nameof(ProductCount));
                OnPropertyChanged(nameof(DisplayNameWithCount));
                OnPropertyChanged(nameof(HasProducts));
            }
        }
    }

    /// <summary>
    /// Number of active products in this category.
    /// </summary>
    public int ActiveProductCount
    {
        get => _activeProductCount;
        set
        {
            if (_activeProductCount != value)
            {
                _activeProductCount = value;
                OnPropertyChanged(nameof(ActiveProductCount));
            }
        }
    }

    /// <summary>
    /// Date and time when the category was created.
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
    /// Indicates whether the category is active.
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
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether this category has any products.
    /// </summary>
    public bool HasProducts => ProductCount > 0;

    /// <summary>
    /// Gets the status display text.
    /// </summary>
    public string Status => IsActive ? "Active" : "Inactive";

    /// <summary>
    /// Gets the display name with product count.
    /// </summary>
    public string DisplayNameWithCount => $"{Name} ({ProductCount})";

    /// <summary>
    /// Gets a truncated description for display purposes.
    /// </summary>
    public string? TruncatedDescription => Description?.Length > 100
        ? Description.Substring(0, 100) + "..."
        : Description;

    /// <summary>
    /// Gets the product count summary text.
    /// </summary>
    public string ProductSummary => HasProducts
        ? $"{ProductCount} product{(ProductCount == 1 ? "" : "s")} ({ActiveProductCount} active)"
        : "No products";

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
    /// Creates a CategoryDto from a Category entity.
    /// </summary>
    /// <param name="category">The category entity.</param>
    /// <returns>A new CategoryDto instance.</returns>
    public static CategoryDto FromCategory(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Icon = category.Icon,
            DisplayOrder = category.DisplayOrder,
            ProductCount = category.ProductCount,
            ActiveProductCount = category.ActiveProductCount,
            CreatedDate = category.CreatedDate,
            IsActive = category.IsActive
        };
    }

    /// <summary>
    /// Creates a CategoryDto from a Category entity with product collection.
    /// </summary>
    /// <param name="category">The category entity.</param>
    /// <param name="products">The products in this category.</param>
    /// <returns>A new CategoryDto instance.</returns>
    public static CategoryDto FromCategory(Category category, ICollection<Product> products)
    {
        var dto = FromCategory(category);
        dto.ProductCount = products?.Count ?? 0;
        dto.ActiveProductCount = products?.Count(p => p.IsActive) ?? 0;
        return dto;
    }
}
