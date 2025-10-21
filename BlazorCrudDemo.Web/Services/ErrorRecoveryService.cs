using Serilog;
using System.Net.Http;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// Service for managing error recovery and retry mechanisms.
/// </summary>
public class ErrorRecoveryService
{
    private readonly ErrorNotificationService _notificationService;
    private readonly Serilog.ILogger _logger;

    public ErrorRecoveryService(ErrorNotificationService notificationService)
    {
        _notificationService = notificationService;
        _logger = Log.ForContext<ErrorRecoveryService>();
    }

    /// <summary>
    /// Executes an operation with retry logic and error handling.
    /// </summary>
    public async Task<T> ExecuteWithRetry<T>(
        Func<Task<T>> operation,
        string operationName,
        int maxRetries = 3,
        int delayMs = 1000)
    {
        var attempts = 0;

        while (attempts < maxRetries)
        {
            try
            {
                attempts++;
                return await operation();
            }
            catch (Exception ex) when (attempts < maxRetries)
            {
                _logger.Warning(ex, "Retry attempt {Attempt} for {Operation} failed: {Message}",
                    attempts, operationName, ex.Message);

                if (attempts < maxRetries)
                {
                    await Task.Delay(delayMs * attempts); // Exponential backoff
                }
            }
        }

        // Final attempt - let the exception bubble up
        throw new InvalidOperationException($"{operationName} failed after {maxRetries} attempts.");
    }

    /// <summary>
    /// Handles network errors with retry and offline detection.
    /// </summary>
    public async Task<T> ExecuteWithNetworkRetry<T>(
        Func<Task<T>> operation,
        string operationName,
        Action? onOffline = null)
    {
        try
        {
            return await ExecuteWithRetry(operation, operationName, 2, 2000);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                ex.InnerException is TimeoutException)
            {
                _notificationService.ShowTimeoutError(operationName);
            }
            else
            {
                _notificationService.ShowNetworkError($"Failed to {operationName.ToLower()}.");
            }

            onOffline?.Invoke();
            throw;
        }
        catch (Exception ex)
        {
            _notificationService.ShowApiError(operationName, ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an operation with exponential backoff retry logic.
    /// </summary>
    public async Task<T> ExecuteWithExponentialRetry<T>(
        Func<Task<T>> operation,
        string operationName,
        int maxRetries = 3,
        int baseDelayMs = 1000,
        double backoffMultiplier = 2.0,
        Func<Exception, bool>? shouldRetry = null)
    {
        var attempts = 0;

        while (attempts < maxRetries)
        {
            try
            {
                attempts++;
                return await operation();
            }
            catch (Exception ex) when (attempts < maxRetries)
            {
                // Check if we should retry this exception type
                if (shouldRetry != null && !shouldRetry(ex))
                {
                    _logger.Warning("Not retrying {Operation} due to exception type: {Exception}", operationName, ex.GetType().Name);
                    throw;
                }

                var delay = (int)(baseDelayMs * Math.Pow(backoffMultiplier, attempts - 1));

                _logger.Warning(ex, "Retry attempt {Attempt}/{MaxRetries} for {Operation} failed, retrying in {DelayMs}ms: {Message}",
                    attempts, maxRetries, operationName, delay, ex.Message);

                await Task.Delay(delay);
            }
        }

        // All retries exhausted
        _logger.Error("Operation {Operation} failed after {MaxRetries} attempts", operationName, maxRetries);
        throw new InvalidOperationException($"{operationName} failed after {maxRetries} attempts.");
    }

    /// <summary>
    /// Executes an operation with circuit breaker pattern for handling repeated failures.
    /// </summary>
    public async Task<T> ExecuteWithCircuitBreaker<T>(
        Func<Task<T>> operation,
        string operationName,
        int failureThreshold = 5,
        TimeSpan? timeout = null)
    {
        var circuitBreaker = new CircuitBreakerState(operationName, failureThreshold, timeout ?? TimeSpan.FromMinutes(1));

        try
        {
            return await circuitBreaker.ExecuteAsync(operation);
        }
        catch (Exception ex)
        {
            _notificationService.ShowCriticalError($"Service unavailable for {operationName}. Please try again later.", "Service Unavailable");
            throw;
        }
    }
}

/// <summary>
/// Simple circuit breaker implementation for handling repeated failures.
/// </summary>
public class CircuitBreakerState
{
    private readonly string _operationName;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private int _failureCount;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private bool _isOpen;

    public CircuitBreakerState(string operationName, int failureThreshold, TimeSpan timeout)
    {
        _operationName = operationName;
        _failureThreshold = failureThreshold;
        _timeout = timeout;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        if (_isOpen)
        {
            if (DateTime.UtcNow - _lastFailureTime > _timeout)
            {
                // Try to close the circuit (half-open state)
                _isOpen = false;
                _failureCount = 0;
            }
            else
            {
                throw new InvalidOperationException($"Circuit breaker is OPEN for {_operationName}");
            }
        }

        try
        {
            var result = await operation();

            // Success - reset failure count
            _failureCount = 0;
            return result;
        }
        catch (Exception)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            if (_failureCount >= _failureThreshold)
            {
                _isOpen = true;
                Serilog.Log.Logger.Warning("Circuit breaker OPENED for {OperationName} after {FailureCount} failures", _operationName, _failureCount);
            }

            throw;
        }
    }
}
