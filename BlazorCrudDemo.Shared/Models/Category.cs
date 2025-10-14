using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorCrudDemo.Shared.Models;

/// <summary>
/// Represents a product category.
/// </summary>
public class Category : BaseEntity
{
    private string? _name;
    private string? _description;
    private string? _icon;
    private int _displayOrder;
    private ICollection<Product>? _products;

    /// <summary>
    /// Name of the category.
    /// </summary>
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
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
    /// Description of the category.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
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
    [StringLength(200, ErrorMessage = "Icon cannot exceed 200 characters")]
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
    [Required(ErrorMessage = "Display order is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Display order must be a non-negative number")]
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
    /// Collection of products in this category.
    /// </summary>
    public ICollection<Product>? Products
    {
        get => _products;
        set
        {
            if (_products != value)
            {
                _products = value;
                OnPropertyChanged(nameof(Products));
            }
        }
    }

    /// <summary>
    /// Gets the number of products in this category.
    /// </summary>
    [NotMapped]
    public int ProductCount => Products?.Count ?? 0;

    /// <summary>
    /// Gets the number of active products in this category.
    /// </summary>
    [NotMapped]
    public int ActiveProductCount => Products?.Count(p => p.IsActive) ?? 0;

    /// <summary>
    /// Gets a value indicating whether this category has any products.
    /// </summary>
    [NotMapped]
    public bool HasProducts => ProductCount > 0;

    /// <summary>
    /// Gets the display name with product count.
    /// </summary>
    [NotMapped]
    public string DisplayNameWithCount => $"{Name} ({ProductCount})";
}
