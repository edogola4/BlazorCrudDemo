using Blazored.LocalStorage;
using Serilog;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// Service for handling offline mode functionality.
/// </summary>
public class OfflineModeService
{
    private readonly ILocalStorageService _localStorage;
    private readonly ErrorNotificationService _notificationService;
    private readonly Serilog.ILogger _logger;
    private readonly List<string> _offlineActions = new();

    public bool IsOfflineMode { get; private set; }
    public event EventHandler<bool>? OfflineModeChanged;

    public OfflineModeService(
        ILocalStorageService localStorage,
        ErrorNotificationService notificationService)
    {
        _localStorage = localStorage;
        _notificationService = notificationService;
        _logger = Log.ForContext<OfflineModeService>();
    }

    /// <summary>
    /// Initializes the offline mode service and loads any pending offline actions.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Load pending offline actions
            var storedActions = await _localStorage.GetItemAsync<List<string>>("offlineActions");
            if (storedActions != null)
            {
                _offlineActions.AddRange(storedActions);
                _logger.Information("Loaded {Count} pending offline actions", _offlineActions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize offline mode service");
        }
    }

    /// <summary>
    /// Enables offline mode.
    /// </summary>
    public void EnableOfflineMode()
    {
        IsOfflineMode = true;
        _notificationService.ShowWarning(
            "You're currently offline. Some features may not be available.",
            "Offline Mode");

        OfflineModeChanged?.Invoke(this, true);
        _logger.Warning("Offline mode enabled");
    }

    /// <summary>
    /// Disables offline mode.
    /// </summary>
    public void DisableOfflineMode()
    {
        IsOfflineMode = false;
        _notificationService.ShowSuccess("You're back online!", "Online");

        OfflineModeChanged?.Invoke(this, false);
        _logger.Information("Offline mode disabled");
    }

    /// <summary>
    /// Queues an action to be executed when back online.
    /// </summary>
    public async Task QueueOfflineActionAsync(string actionType, object? data = null)
    {
        var action = new
        {
            Type = actionType,
            Data = data,
            Timestamp = DateTime.UtcNow,
            Id = Guid.NewGuid().ToString()
        };

        _offlineActions.Add(System.Text.Json.JsonSerializer.Serialize(action));

        // Save to localStorage
        await _localStorage.SetItemAsync("offlineActions", _offlineActions);

        _logger.Information("Queued offline action: {ActionType}", actionType);
    }

    /// <summary>
    /// Processes all pending offline actions.
    /// </summary>
    public async Task ProcessOfflineActionsAsync(Func<string, object?, Task<bool>> actionProcessor)
    {
        if (!_offlineActions.Any())
        {
            _logger.Information("No offline actions to process");
            return;
        }

        _logger.Information("Processing {Count} offline actions", _offlineActions.Count);

        var processedCount = 0;
        var failedCount = 0;
        var remainingActions = new List<string>();

        foreach (var actionJson in _offlineActions)
        {
            try
            {
                var action = System.Text.Json.JsonSerializer.Deserialize<dynamic>(actionJson);
                if (action == null) continue;

                var success = await actionProcessor(
                    action.GetProperty("Type").GetString() ?? "",
                    action.Data);

                if (success)
                {
                    processedCount++;
                }
                else
                {
                    failedCount++;
                    remainingActions.Add(actionJson); // Keep for retry
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to process offline action: {Action}", actionJson);
                failedCount++;
                remainingActions.Add(actionJson);
            }
        }

        // Update the stored actions
        _offlineActions.Clear();
        _offlineActions.AddRange(remainingActions);
        await _localStorage.SetItemAsync("offlineActions", _offlineActions);

        if (processedCount > 0)
        {
            _notificationService.ShowSuccess(
                $"Successfully processed {processedCount} offline actions.",
                "Actions Synced");
        }

        if (failedCount > 0)
        {
            _notificationService.ShowWarning(
                $"{failedCount} actions could not be processed and will be retried later.",
                "Sync Issues");
        }

        _logger.Information("Processed {Processed} actions, {Failed} failed", processedCount, failedCount);
    }

    /// <summary>
    /// Gets the count of pending offline actions.
    /// </summary>
    public int GetPendingActionsCount()
    {
        return _offlineActions.Count;
    }

    /// <summary>
    /// Clears all offline actions (for testing or reset).
    /// </summary>
    public async Task ClearOfflineActionsAsync()
    {
        _offlineActions.Clear();
        await _localStorage.RemoveItemAsync("offlineActions");
        _logger.Warning("All offline actions cleared");
    }
}

/// <summary>
/// Service for providing user guidance during error recovery.
/// </summary>
public class ErrorRecoveryGuidanceService
{
    private readonly ErrorNotificationService _notificationService;
    private readonly Serilog.ILogger _logger;

    public ErrorRecoveryGuidanceService(ErrorNotificationService notificationService)
    {
        _notificationService = notificationService;
        _logger = Log.ForContext<ErrorRecoveryGuidanceService>();
    }

    /// <summary>
    /// Provides user guidance for common error scenarios.
    /// </summary>
    public void ProvideGuidance(string errorType, Exception? exception = null)
    {
        var guidance = errorType.ToLower() switch
        {
            "network" => new ErrorGuidance
            {
                Title = "Network Connection Issues",
                Message = "Please check your internet connection and try again. If the problem persists, the server might be temporarily unavailable.",
                Actions = new[]
                {
                    "Check your Wi-Fi or mobile data connection",
                    "Try refreshing the page",
                    "Contact support if the issue continues"
                }
            },
            "timeout" => new ErrorGuidance
            {
                Title = "Request Timed Out",
                Message = "The request took too long to complete. This might be due to slow internet or server load.",
                Actions = new[]
                {
                    "Try again in a few moments",
                    "Check if other websites load normally",
                    "Simplify your request if possible"
                }
            },
            "validation" => new ErrorGuidance
            {
                Title = "Invalid Input",
                Message = "Please check your input and ensure all required fields are filled correctly.",
                Actions = new[]
                {
                    "Review the form for missing or incorrect information",
                    "Check for format requirements (dates, numbers, etc.)",
                    "Clear and re-enter the information if needed"
                }
            },
            "permission" => new ErrorGuidance
            {
                Title = "Access Denied",
                Message = "You don't have permission to perform this action. Please contact your administrator if you believe this is an error.",
                Actions = new[]
                {
                    "Contact your system administrator",
                    "Request the necessary permissions",
                    "Try logging out and back in"
                }
            },
            "server" => new ErrorGuidance
            {
                Title = "Server Error",
                Message = "We're experiencing technical difficulties. Our team has been notified and is working on a fix.",
                Actions = new[]
                {
                    "Try again in a few minutes",
                    "Clear your browser cache and cookies",
                    "Contact support if the problem persists"
                }
            },
            _ => new ErrorGuidance
            {
                Title = "Unexpected Error",
                Message = "Something unexpected happened. Please try again or contact support if the issue continues.",
                Actions = new[]
                {
                    "Try refreshing the page",
                    "Clear your browser cache",
                    "Contact support with details about what you were doing"
                }
            }
        };

        // Show guidance notification
        ShowGuidanceNotification(guidance);

        _logger.Information("Provided recovery guidance for error type: {ErrorType}", errorType);
    }

    private void ShowGuidanceNotification(ErrorGuidance guidance)
    {
        var message = $"{guidance.Message}\n\nWhat you can do:\n" +
                     string.Join("\n• ", guidance.Actions.Select((action, i) => $"{i + 1}. {action}"));

        _notificationService.ShowInfo(message, guidance.Title);
    }
}

/// <summary>
/// Represents error recovery guidance information.
/// </summary>
public class ErrorGuidance
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string[] Actions { get; set; } = Array.Empty<string>();
}
