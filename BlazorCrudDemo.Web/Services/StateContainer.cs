using BlazorCrudDemo.Shared.DTOs;
using BlazorCrudDemo.Shared.Exceptions;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// Application state container for managing global application state.
/// </summary>
public interface IStateContainer
{
    /// <summary>
    /// Gets the current user information.
    /// </summary>
    UserInfo? CurrentUser { get; }

    /// <summary>
    /// Gets or sets the current theme.
    /// </summary>
    string Theme { get; set; }

    /// <summary>
    /// Gets or sets whether the application is in loading state.
    /// </summary>
    bool IsLoading { get; set; }

    /// <summary>
    /// Gets or sets the current page title.
    /// </summary>
    string PageTitle { get; set; }

    /// <summary>
    /// Gets or sets the current breadcrumb navigation.
    /// </summary>
    List<BreadcrumbItem> Breadcrumbs { get; set; }

    /// <summary>
    /// Gets or sets the current search filters.
    /// </summary>
    Dictionary<string, object> SearchFilters { get; set; }

    /// <summary>
    /// Gets or sets the current page state for navigation.
    /// </summary>
    Dictionary<string, object> PageState { get; set; }

    /// <summary>
    /// Gets or sets the application configuration.
    /// </summary>
    AppConfig AppConfig { get; set; }

    /// <summary>
    /// Event raised when the state changes.
    /// </summary>
    event Action? OnStateChanged;

    /// <summary>
    /// Sets the current user.
    /// </summary>
    /// <param name="user">The user information.</param>
    void SetCurrentUser(UserInfo user);

    /// <summary>
    /// Clears the current user (logout).
    /// </summary>
    void ClearCurrentUser();

    /// <summary>
    /// Updates the loading state.
    /// </summary>
    /// <param name="isLoading">Whether the application is loading.</param>
    void SetLoading(bool isLoading);

    /// <summary>
    /// Updates the page title.
    /// </summary>
    /// <param name="title">The new page title.</param>
    void SetPageTitle(string title);

    /// <summary>
    /// Updates the breadcrumb navigation.
    /// </summary>
    /// <param name="breadcrumbs">The breadcrumb items.</param>
    void SetBreadcrumbs(List<BreadcrumbItem> breadcrumbs);

    /// <summary>
    /// Updates search filters.
    /// </summary>
    /// <param name="filters">The search filters.</param>
    void SetSearchFilters(Dictionary<string, object> filters);

    /// <summary>
    /// Updates page state for navigation.
    /// </summary>
    /// <param name="state">The page state.</param>
    void SetPageState(Dictionary<string, object> state);

    /// <summary>
    /// Updates application configuration.
    /// </summary>
    /// <param name="config">The application configuration.</param>
    void SetAppConfig(AppConfig config);

    /// <summary>
    /// Gets a value from the page state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The state key.</param>
    /// <returns>The value or default if not found.</returns>
    T? GetPageStateValue<T>(string key);

    /// <summary>
    /// Sets a value in the page state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The state key.</param>
    /// <param name="value">The value to set.</param>
    void SetPageStateValue<T>(string key, T value);

    /// <summary>
    /// Clears all state.
    /// </summary>
    void ClearAllState();

    /// <summary>
    /// Saves the current state to local storage.
    /// </summary>
    Task SaveStateAsync();

    /// <summary>
    /// Loads the state from local storage.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task LoadStateAsync();
}

/// <summary>
/// User information model.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User roles.
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Whether the user is authenticated.
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// Last login time.
    /// </summary>
    public DateTime LastLogin { get; set; }

    /// <summary>
    /// Profile picture URL.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }
}

/// <summary>
/// Breadcrumb navigation item.
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// Display text for the breadcrumb.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// URL for the breadcrumb link.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Whether this is the active/current page.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Icon class for the breadcrumb.
    /// </summary>
    public string? Icon { get; set; }
}

/// <summary>
/// Application configuration.
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Application name.
    /// </summary>
    public string AppName { get; set; } = "Blazor CRUD Demo";

    /// <summary>
    /// Application version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// API base URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "/api";

    /// <summary>
    /// Default page size for pagination.
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>
    /// Maximum page size for pagination.
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Whether to enable caching.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache duration in minutes.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 10;

    /// <summary>
    /// Whether to enable real-time updates.
    /// </summary>
    public bool EnableRealTimeUpdates { get; set; } = true;

    /// <summary>
    /// Whether to enable audit logging.
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Supported file formats for export.
    /// </summary>
    public List<string> SupportedExportFormats { get; set; } = new() { "Excel", "PDF", "CSV" };

    /// <summary>
    /// Maximum file size for uploads (in MB).
    /// </summary>
    public int MaxUploadFileSizeMB { get; set; } = 10;
}

/// <summary>
/// State container implementation for managing global application state.
/// </summary>
public class StateContainer : IStateContainer
{
    private UserInfo? _currentUser;
    private string _theme = "light";
    private bool _isLoading = false;
    private string _pageTitle = "Dashboard";
    private List<BreadcrumbItem> _breadcrumbs = new();
    private Dictionary<string, object> _searchFilters = new();
    private Dictionary<string, object> _pageState = new();
    private AppConfig _appConfig = new();

    /// <inheritdoc />
    public UserInfo? CurrentUser => _currentUser;

    /// <inheritdoc />
    public string Theme
    {
        get => _theme;
        set
        {
            if (_theme != value)
            {
                _theme = value;
                NotifyStateChanged();
            }
        }
    }

    /// <inheritdoc />
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                NotifyStateChanged();
            }
        }
    }

    /// <inheritdoc />
    public string PageTitle
    {
        get => _pageTitle;
        set
        {
            if (_pageTitle != value)
            {
                _pageTitle = value;
                NotifyStateChanged();
            }
        }
    }

    /// <inheritdoc />
    public List<BreadcrumbItem> Breadcrumbs
    {
        get => _breadcrumbs;
        set
        {
            if (_breadcrumbs != value)
            {
                _breadcrumbs = value ?? new List<BreadcrumbItem>();
                NotifyStateChanged();
            }
        }
    }

    /// <inheritdoc />
    public Dictionary<string, object> SearchFilters
    {
        get => _searchFilters;
        set
        {
            if (_searchFilters != value)
            {
                _searchFilters = value ?? new Dictionary<string, object>();
                NotifyStateChanged();
            }
        }
    }

    /// <inheritdoc />
    public Dictionary<string, object> PageState
    {
        get => _pageState;
        set
        {
            if (_pageState != value)
            {
                _pageState = value ?? new Dictionary<string, object>();
                NotifyStateChanged();
            }
        }
    }

    /// <inheritdoc />
    public AppConfig AppConfig
    {
        get => _appConfig;
        set
        {
            if (_appConfig != value)
            {
                _appConfig = value ?? new AppConfig();
                NotifyStateChanged();
            }
        }
    }

    /// <inheritdoc />
    public event Action? OnStateChanged;

    /// <inheritdoc />
    public void SetCurrentUser(UserInfo user)
    {
        if (_currentUser != user)
        {
            _currentUser = user;
            NotifyStateChanged();
        }
    }

    /// <inheritdoc />
    public void ClearCurrentUser()
    {
        if (_currentUser != null)
        {
            _currentUser = null;
            NotifyStateChanged();
        }
    }

    /// <inheritdoc />
    public void SetLoading(bool isLoading)
    {
        IsLoading = isLoading;
    }

    /// <inheritdoc />
    public void SetPageTitle(string title)
    {
        PageTitle = title;
    }

    /// <inheritdoc />
    public void SetBreadcrumbs(List<BreadcrumbItem> breadcrumbs)
    {
        Breadcrumbs = breadcrumbs;
    }

    /// <inheritdoc />
    public void SetSearchFilters(Dictionary<string, object> filters)
    {
        SearchFilters = filters;
    }

    /// <inheritdoc />
    public void SetPageState(Dictionary<string, object> state)
    {
        PageState = state;
    }

    /// <inheritdoc />
    public void SetAppConfig(AppConfig config)
    {
        AppConfig = config;
    }

    /// <inheritdoc />
    public T? GetPageStateValue<T>(string key)
    {
        return _pageState.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : default;
    }

    /// <inheritdoc />
    public void SetPageStateValue<T>(string key, T value)
    {
        _pageState[key] = value!;
        NotifyStateChanged();
    }

    /// <inheritdoc />
    public void ClearAllState()
    {
        _currentUser = null;
        _theme = "light";
        _isLoading = false;
        _pageTitle = "Dashboard";
        _breadcrumbs.Clear();
        _searchFilters.Clear();
        _pageState.Clear();
        _appConfig = new AppConfig();

        NotifyStateChanged();
    }

    /// <inheritdoc />
    public async Task SaveStateAsync()
    {
        try
        {
            // In a real implementation, you would save to localStorage or a backend service
            // For now, we'll just log the operation
            Console.WriteLine("State saved to storage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save state: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task LoadStateAsync()
    {
        try
        {
            // In a real implementation, you would load from localStorage or a backend service
            // For now, we'll just log the operation
            Console.WriteLine("State loaded from storage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load state: {ex.Message}");
        }
    }

    /// <summary>
    /// Notifies subscribers that the state has changed.
    /// </summary>
    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Adds a breadcrumb item.
    /// </summary>
    /// <param name="text">The breadcrumb text.</param>
    /// <param name="url">The breadcrumb URL.</param>
    /// <param name="icon">The breadcrumb icon.</param>
    public void AddBreadcrumb(string text, string? url = null, string? icon = null)
    {
        var breadcrumb = new BreadcrumbItem
        {
            Text = text,
            Url = url,
            Icon = icon,
            IsActive = false
        };

        // Mark the last breadcrumb as active
        if (_breadcrumbs.Any())
        {
            _breadcrumbs.Last().IsActive = false;
        }

        _breadcrumbs.Add(breadcrumb);
        NotifyStateChanged();
    }

    /// <summary>
    /// Sets the active breadcrumb.
    /// </summary>
    /// <param name="text">The breadcrumb text to make active.</param>
    public void SetActiveBreadcrumb(string text)
    {
        foreach (var breadcrumb in _breadcrumbs)
        {
            breadcrumb.IsActive = breadcrumb.Text == text;
        }
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears breadcrumbs.
    /// </summary>
    public void ClearBreadcrumbs()
    {
        _breadcrumbs.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Sets a search filter value.
    /// </summary>
    /// <typeparam name="T">The type of the filter value.</typeparam>
    /// <param name="key">The filter key.</param>
    /// <param name="value">The filter value.</param>
    public void SetSearchFilter<T>(string key, T value)
    {
        _searchFilters[key] = value!;
        NotifyStateChanged();
    }

    /// <summary>
    /// Gets a search filter value.
    /// </summary>
    /// <typeparam name="T">The type of the filter value.</typeparam>
    /// <param name="key">The filter key.</param>
    /// <returns>The filter value or default if not found.</returns>
    public T? GetSearchFilter<T>(string key)
    {
        return _searchFilters.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : default;
    }

    /// <summary>
    /// Clears all search filters.
    /// </summary>
    public void ClearSearchFilters()
    {
        _searchFilters.Clear();
        NotifyStateChanged();
    }

    /// <summary>
    /// Navigates to a page and updates state.
    /// </summary>
    /// <param name="pageTitle">The page title.</param>
    /// <param name="breadcrumbs">The breadcrumb navigation.</param>
    /// <param name="pageState">The page state.</param>
    public void NavigateToPage(string pageTitle, List<BreadcrumbItem>? breadcrumbs = null, Dictionary<string, object>? pageState = null)
    {
        PageTitle = pageTitle;

        if (breadcrumbs != null)
        {
            Breadcrumbs = breadcrumbs;
        }

        if (pageState != null)
        {
            PageState = pageState;
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Updates the user profile information.
    /// </summary>
    /// <param name="profilePictureUrl">The profile picture URL.</param>
    /// <param name="fullName">The full name.</param>
    public void UpdateUserProfile(string? profilePictureUrl = null, string? fullName = null)
    {
        if (_currentUser == null) return;

        if (profilePictureUrl != null)
        {
            _currentUser.ProfilePictureUrl = profilePictureUrl;
        }

        if (fullName != null)
        {
            _currentUser.FullName = fullName;
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Toggles the theme between light and dark.
    /// </summary>
    public void ToggleTheme()
    {
        Theme = _theme == "light" ? "dark" : "light";
    }
}
