using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BlazorCrudDemo.Shared.Models;

/// <summary>
/// Abstract base class for all entities with common properties.
/// </summary>
public abstract class BaseEntity : INotifyPropertyChanged
{
    private int _id;
    private DateTime _createdDate;
    private DateTime _modifiedDate;
    private bool _isActive;

    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    [Key]
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
    /// Date and time when the entity was created.
    /// </summary>
    [Required]
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
    /// Date and time when the entity was last modified.
    /// </summary>
    [Required]
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
    /// Indicates whether the entity is active.
    /// </summary>
    [Required]
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
}
