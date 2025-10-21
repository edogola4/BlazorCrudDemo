using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Serilog;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// Service for managing error notifications throughout the application.
/// </summary>
public class ErrorNotificationService
{
    private readonly IToastService _toastService;
    private readonly Serilog.ILogger _logger;

    public ErrorNotificationService(IToastService toastService)
    {
        _toastService = toastService;
        _logger = Log.ForContext<ErrorNotificationService>();
    }

    /// <summary>
    /// Shows a toast notification for minor errors or warnings.
    /// </summary>
    public void ShowError(string message, string? title = null, bool isDismissible = true)
    {
        _toastService.ShowError(message);
        _logger.Warning("Error notification shown: {Title} - {Message}", title, message);
    }

    /// <summary>
    /// Shows a toast notification for success messages.
    /// </summary>
    public void ShowSuccess(string message, string? title = null, bool isDismissible = true)
    {
        _toastService.ShowSuccess(message);
        _logger.Information("Success notification shown: {Title} - {Message}", title, message);
    }

    /// <summary>
    /// Shows a toast notification for informational messages.
    /// </summary>
    public void ShowInfo(string message, string? title = null, bool isDismissible = true)
    {
        _toastService.ShowInfo(message);
        _logger.Information("Info notification shown: {Title} - {Message}", title, message);
    }

    /// <summary>
    /// Shows a toast notification for warning messages.
    /// </summary>
    public void ShowWarning(string message, string? title = null, bool isDismissible = true)
    {
        _toastService.ShowWarning(message);
        _logger.Warning("Warning notification shown: {Title} - {Message}", title, message);
    }

    /// <summary>
    /// Shows a critical error that requires user attention.
    /// </summary>
    public void ShowCriticalError(string message, string? title = null, Exception? exception = null)
    {
        var errorTitle = title ?? "Critical Error";

        // Log the critical error
        if (exception != null)
        {
            _logger.Error(exception, "Critical error shown to user: {Title} - {Message}", errorTitle, message);
        }
        else
        {
            _logger.Error("Critical error shown to user: {Title} - {Message}", errorTitle, message);
        }

        // Show as error toast for now - in a real app you might want a modal
        _toastService.ShowError($"{message}\n\nPlease refresh the page or contact support if the problem persists.");
    }

    /// <summary>
    /// Shows a notification for network connectivity issues.
    /// </summary>
    public void ShowNetworkError(string? message = null)
    {
        var errorMessage = message ?? "Unable to connect to the server. Please check your internet connection and try again.";

        ShowError(errorMessage, "Connection Error");

        _logger.Warning("Network error notification shown: {Message}", errorMessage);
    }

    /// <summary>
    /// Shows a notification for API call failures.
    /// </summary>
    public void ShowApiError(string operation, Exception exception)
    {
        var message = $"Failed to {operation.ToLower()}. Please try again.";

        ShowError(message, $"API Error - {operation}");

        _logger.Error(exception, "API error during {Operation}: {Message}", operation, exception.Message);
    }

    /// <summary>
    /// Shows a notification for validation errors.
    /// </summary>
    public void ShowValidationError(string message, string? fieldName = null)
    {
        var title = fieldName != null ? $"Validation Error - {fieldName}" : "Validation Error";

        ShowWarning(message, title);

        _logger.Warning("Validation error notification shown: {Field} - {Message}", fieldName, message);
    }

    /// <summary>
    /// Shows a notification for operation timeouts.
    /// </summary>
    public void ShowTimeoutError(string operation)
    {
        var message = $"{operation} timed out. Please try again.";

        ShowWarning(message, "Operation Timeout");

        _logger.Warning("Timeout error notification shown for operation: {Operation}", operation);
    }

    /// <summary>
    /// Shows a notification for permission/authorization errors.
    /// </summary>
    public void ShowPermissionError(string requiredPermission)
    {
        var message = $"You don't have permission to {requiredPermission.ToLower()}.";

        ShowError(message, "Permission Denied");

        _logger.Warning("Permission error notification shown for: {Permission}", requiredPermission);
    }
}
