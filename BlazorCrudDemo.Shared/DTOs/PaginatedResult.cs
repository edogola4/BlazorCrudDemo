using System.ComponentModel;

namespace BlazorCrudDemo.Shared.DTOs;

/// <summary>
/// Generic class for paginated results.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public class PaginatedResult<T> : INotifyPropertyChanged
{
    private int _currentPage;
    private int _pageSize;
    private int _totalCount;
    private int _totalPages;
    private bool _hasPreviousPage;
    private bool _hasNextPage;
    private IEnumerable<T>? _items;

    /// <summary>
    /// Initializes a new instance of the PaginatedResult class.
    /// </summary>
    public PaginatedResult()
    {
        _currentPage = 1;
        _pageSize = 10;
        _totalCount = 0;
        _totalPages = 0;
        _hasPreviousPage = false;
        _hasNextPage = false;
        _items = Enumerable.Empty<T>();
    }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                UpdatePaginationProperties();
            }
        }
    }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize != value)
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
                UpdatePaginationProperties();
            }
        }
    }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalCount
    {
        get => _totalCount;
        set
        {
            if (_totalCount != value)
            {
                _totalCount = value;
                OnPropertyChanged(nameof(TotalCount));
                UpdatePaginationProperties();
            }
        }
    }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages
    {
        get => _totalPages;
        private set
        {
            if (_totalPages != value)
            {
                _totalPages = value;
                OnPropertyChanged(nameof(TotalPages));
            }
        }
    }

    /// <summary>
    /// Indicates whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage
    {
        get => _hasPreviousPage;
        private set
        {
            if (_hasPreviousPage != value)
            {
                _hasPreviousPage = value;
                OnPropertyChanged(nameof(HasPreviousPage));
            }
        }
    }

    /// <summary>
    /// Indicates whether there is a next page.
    /// </summary>
    public bool HasNextPage
    {
        get => _hasNextPage;
        private set
        {
            if (_hasNextPage != value)
            {
                _hasNextPage = value;
                OnPropertyChanged(nameof(HasNextPage));
            }
        }
    }

    /// <summary>
    /// Items on the current page.
    /// </summary>
    public IEnumerable<T>? Items
    {
        get => _items;
        set
        {
            if (_items != value)
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }
    }

    /// <summary>
    /// Gets the start index of the current page (1-based).
    /// </summary>
    public int StartIndex => ((CurrentPage - 1) * PageSize) + 1;

    /// <summary>
    /// Gets the end index of the current page (1-based).
    /// </summary>
    public int EndIndex => Math.Min(CurrentPage * PageSize, TotalCount);

    /// <summary>
    /// Gets the pagination summary text.
    /// </summary>
    public string PaginationSummary
    {
        get
        {
            if (TotalCount == 0)
                return "No items found";

            return $"Showing {StartIndex}-{EndIndex} of {TotalCount} items";
        }
    }

    /// <summary>
    /// Gets a value indicating whether there are any items.
    /// </summary>
    public bool HasItems => Items?.Any() == true;

    /// <summary>
    /// Gets a value indicating whether this is the first page.
    /// </summary>
    public bool IsFirstPage => CurrentPage == 1;

    /// <summary>
    /// Gets a value indicating whether this is the last page.
    /// </summary>
    public bool IsLastPage => CurrentPage == TotalPages || TotalPages == 0;

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
    /// Updates pagination-related calculated properties.
    /// </summary>
    private void UpdatePaginationProperties()
    {
        TotalPages = PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
        HasPreviousPage = CurrentPage > 1;
        HasNextPage = CurrentPage < TotalPages;
    }

    /// <summary>
    /// Creates a new PaginatedResult with updated pagination info.
    /// </summary>
    /// <param name="items">Items for the current page.</param>
    /// <param name="totalCount">Total number of items.</param>
    /// <param name="currentPage">Current page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A new PaginatedResult instance.</returns>
    public static PaginatedResult<T> Create(IEnumerable<T> items, int totalCount, int currentPage, int pageSize)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = currentPage,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Creates an empty PaginatedResult.
    /// </summary>
    /// <returns>An empty PaginatedResult instance.</returns>
    public static PaginatedResult<T> Empty()
    {
        return new PaginatedResult<T>();
    }
}
