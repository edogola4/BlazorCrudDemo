using BlazorCrudDemo.Shared.Exceptions;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// Service for managing toast notifications in the Blazor application.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Shows a success toast message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">Optional title for the toast.</param>
    void ShowSuccess(string message, string? title = null);

    /// <summary>
    /// Shows an error toast message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">Optional title for the toast.</param>
    void ShowError(string message, string? title = null);

    /// <summary>
    /// Shows a warning toast message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">Optional title for the toast.</param>
    void ShowWarning(string message, string? title = null);

    /// <summary>
    /// Shows an info toast message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">Optional title for the toast.</param>
    void ShowInfo(string message, string? title = null);

    /// <summary>
    /// Shows a toast message for a specific operation result.
    /// </summary>
    /// <param name="operation">The operation that was performed.</param>
    /// <param name="entityType">The type of entity (e.g., "Product", "Category").</param>
    /// <param name="success">Whether the operation was successful.</param>
    void ShowOperationResult(string operation, string entityType, bool success);

    /// <summary>
    /// Shows validation error messages.
    /// </summary>
    /// <param name="errors">List of validation errors.</param>
    void ShowValidationErrors(IEnumerable<string> errors);

    /// <summary>
    /// Shows validation error message for a single field.
    /// </summary>
    /// <param name="fieldName">The field name.</param>
    /// <param name="error">The error message.</param>
    void ShowValidationError(string fieldName, string error);

    /// <summary>
    /// Clears all toast messages.
    /// </summary>
    void ClearAll();

    /// <summary>
    /// Event raised when a toast should be shown.
    /// </summary>
    event Action<ToastMessage>? OnShowToast;
}

/// <summary>
/// Represents a toast message.
/// </summary>
public class ToastMessage
{
    /// <summary>
    /// The message type.
    /// </summary>
    public ToastType Type { get; set; }

    /// <summary>
    /// The message text.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The message title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// How long to show the toast (in seconds).
    /// </summary>
    public int Duration { get; set; } = 5;

    /// <summary>
    /// Whether the toast can be dismissed by the user.
    /// </summary>
    public bool Dismissible { get; set; } = true;

    /// <summary>
    /// CSS classes for the toast.
    /// </summary>
    public string CssClass => Type switch
    {
        ToastType.Success => "toast-success",
        ToastType.Error => "toast-error",
        ToastType.Warning => "toast-warning",
        ToastType.Info => "toast-info",
        _ => "toast-info"
    };
}

/// <summary>
/// Toast message types.
/// </summary>
public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

/// <summary>
/// Service implementation for managing toast notifications.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public event Action<ToastMessage>? OnShowToast;

    /// <inheritdoc />
    public void ShowSuccess(string message, string? title = null)
    {
        var toast = new ToastMessage
        {
            Type = ToastType.Success,
            Message = message,
            Title = title ?? "Success",
            Duration = 5
        };

        _logger.LogInformation("Showing success toast: {Message}", message);
        OnShowToast?.Invoke(toast);
    }

    /// <inheritdoc />
    public void ShowError(string message, string? title = null)
    {
        var toast = new ToastMessage
        {
            Type = ToastType.Error,
            Message = message,
            Title = title ?? "Error",
            Duration = 8
        };

        _logger.LogError("Showing error toast: {Message}", message);
        OnShowToast?.Invoke(toast);
    }

    /// <inheritdoc />
    public void ShowWarning(string message, string? title = null)
    {
        var toast = new ToastMessage
        {
            Type = ToastType.Warning,
            Message = message,
            Title = title ?? "Warning",
            Duration = 6
        };

        _logger.LogWarning("Showing warning toast: {Message}", message);
        OnShowToast?.Invoke(toast);
    }

    /// <inheritdoc />
    public void ShowInfo(string message, string? title = null)
    {
        var toast = new ToastMessage
        {
            Type = ToastType.Info,
            Message = message,
            Title = title ?? "Information",
            Duration = 5
        };

        _logger.LogInformation("Showing info toast: {Message}", message);
        OnShowToast?.Invoke(toast);
    }

    /// <inheritdoc />
    public void ShowOperationResult(string operation, string entityType, bool success)
    {
        if (success)
        {
            ShowSuccess($"{entityType} {operation.ToLower()} successfully", $"{operation} Successful");
        }
        else
        {
            ShowError($"Failed to {operation.ToLower()} {entityType.ToLower()}", $"{operation} Failed");
        }
    }

    /// <inheritdoc />
    public void ShowValidationErrors(IEnumerable<string> errors)
    {
        var errorMessage = string.Join(Environment.NewLine, errors);

        var toast = new ToastMessage
        {
            Type = ToastType.Error,
            Message = $"Please fix the following errors:{Environment.NewLine}{errorMessage}",
            Title = "Validation Errors",
            Duration = 10
        };

        _logger.LogWarning("Showing validation errors toast: {Errors}", errorMessage);
        OnShowToast?.Invoke(toast);
    }

    /// <inheritdoc />
    public void ShowValidationError(string fieldName, string error)
    {
        var toast = new ToastMessage
        {
            Type = ToastType.Warning,
            Message = $"{fieldName}: {error}",
            Title = "Validation Error",
            Duration = 6
        };

        _logger.LogWarning("Showing field validation error toast: {FieldName} - {Error}", fieldName, error);
        OnShowToast?.Invoke(toast);
    }

    /// <inheritdoc />
    public void ClearAll()
    {
        _logger.LogDebug("Clearing all toast messages");
        // This would be implemented by the Blazor component to clear visible toasts
    }

    /// <summary>
    /// Shows a toast for common CRUD operations.
    /// </summary>
    /// <param name="operation">The operation performed.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityName">The entity name or identifier.</param>
    /// <param name="success">Whether the operation was successful.</param>
    public void ShowCrudResult(string operation, string entityType, string entityName, bool success)
    {
        if (success)
        {
            ShowSuccess($"{entityType} '{entityName}' {operation.ToLower()} successfully", $"{operation} Successful");
        }
        else
        {
            ShowError($"Failed to {operation.ToLower()} {entityType.ToLower()} '{entityName}'", $"{operation} Failed");
        }
    }

    /// <summary>
    /// Shows a toast for bulk operations.
    /// </summary>
    /// <param name="operation">The operation performed.</param>
    /// <param name="entityType">The entity type.</param>
    /// <param name="count">The number of items affected.</param>
    /// <param name="success">Whether the operation was successful.</param>
    public void ShowBulkOperationResult(string operation, string entityType, int count, bool success)
    {
        if (success)
        {
            ShowSuccess($"{count} {entityType.ToLower()}{(count == 1 ? "" : "s")} {operation.ToLower()} successfully",
                       "Bulk Operation Successful");
        }
        else
        {
            ShowError($"Failed to {operation.ToLower()} {count} {entityType.ToLower()}{(count == 1 ? "" : "s")}",
                     "Bulk Operation Failed");
        }
    }

    /// <summary>
    /// Shows a toast for system operations.
    /// </summary>
    /// <param name="operation">The operation performed.</param>
    /// <param name="success">Whether the operation was successful.</param>
    public void ShowSystemOperationResult(string operation, bool success)
    {
        if (success)
        {
            ShowSuccess($"System {operation.ToLower()} completed successfully", "System Operation");
        }
        else
        {
            ShowError($"System {operation.ToLower()} failed", "System Operation");
        }
    }

    /// <summary>
    /// Shows a confirmation toast asking for user action.
    /// </summary>
    /// <param name="message">The confirmation message.</param>
    /// <param name="title">The toast title.</param>
    /// <param name="onConfirm">Action to execute when user confirms.</param>
    /// <param name="onCancel">Action to execute when user cancels.</param>
    public void ShowConfirmation(string message, string title, Action? onConfirm = null, Action? onCancel = null)
    {
        var toast = new ToastMessage
        {
            Type = ToastType.Info,
            Message = message,
            Title = title,
            Duration = 0, // No auto-dismiss for confirmations
            Dismissible = false
        };

        _logger.LogInformation("Showing confirmation toast: {Message}", message);

        // In a real implementation, you would need to extend ToastMessage
        // to support callbacks and implement this in the Blazor component
        OnShowToast?.Invoke(toast);
    }
}
