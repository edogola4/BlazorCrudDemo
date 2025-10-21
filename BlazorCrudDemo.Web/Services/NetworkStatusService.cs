using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Serilog;
using System.Timers;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// Service for monitoring network connectivity status.
/// </summary>
public class NetworkStatusService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ErrorNotificationService _notificationService;
    private readonly Serilog.ILogger _logger;
    private System.Timers.Timer? _heartbeatTimer;
    private bool _isOnline = true;
    private bool _wasOnline = true;

    public event EventHandler<bool>? NetworkStatusChanged;

    public bool IsOnline => _isOnline;

    public NetworkStatusService(IJSRuntime jsRuntime, ErrorNotificationService notificationService)
    {
        _jsRuntime = jsRuntime;
        _notificationService = notificationService;
        _logger = Log.ForContext<NetworkStatusService>();
    }

    /// <summary>
    /// Initializes the network status monitoring.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Check initial network status
            _isOnline = await _jsRuntime.InvokeAsync<bool>("navigator.onLine");
            _wasOnline = _isOnline;

            _logger.Information("Network status initialized: {Status}", _isOnline ? "Online" : "Offline");

            // Set up JavaScript event listeners for network changes
            await _jsRuntime.InvokeVoidAsync("blazorCrudDemo.networkStatus.initialize",
                DotNetObjectReference.Create(this));

            // Start heartbeat monitoring
            StartHeartbeatMonitoring();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize network status monitoring");
        }
    }

    /// <summary>
    /// Called from JavaScript when network status changes.
    /// </summary>
    [JSInvokable]
    public void OnNetworkStatusChanged(bool isOnline)
    {
        _isOnline = isOnline;

        if (_isOnline != _wasOnline)
        {
            _logger.Information("Network status changed: {Status}", _isOnline ? "Online" : "Offline");

            // Show appropriate notification
            if (_isOnline)
            {
                _notificationService.ShowSuccess("Connection restored!", "Back Online");
            }
            else
            {
                _notificationService.ShowWarning("Connection lost. Some features may not work properly.", "Offline Mode");
            }

            // Raise event
            NetworkStatusChanged?.Invoke(this, _isOnline);

            _wasOnline = _isOnline;
        }
    }

    /// <summary>
    /// Starts periodic heartbeat monitoring to detect network issues.
    /// </summary>
    private void StartHeartbeatMonitoring()
    {
        _heartbeatTimer = new System.Timers.Timer(30000); // Check every 30 seconds
        _heartbeatTimer.Elapsed += async (sender, e) => await PerformHeartbeatCheck();
        _heartbeatTimer.Start();

        _logger.Information("Network heartbeat monitoring started");
    }

    /// <summary>
    /// Performs a heartbeat check to verify network connectivity.
    /// </summary>
    private async Task PerformHeartbeatCheck()
    {
        try
        {
            // Try to reach a reliable endpoint (like a favicon or health check)
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

            // Use a simple HEAD request to check connectivity
            var response = await httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Head, "https://www.google.com/favicon.ico"));

            var currentOnline = response.IsSuccessStatusCode;

            if (currentOnline != _isOnline)
            {
                OnNetworkStatusChanged(currentOnline);
            }
        }
        catch (Exception ex)
        {
            // If the heartbeat fails and we think we're online, mark as offline
            if (_isOnline)
            {
                _logger.Warning(ex, "Heartbeat check failed, marking as offline");
                OnNetworkStatusChanged(false);
            }
        }
    }

    /// <summary>
    /// Manually checks current network status.
    /// </summary>
    public async Task<bool> CheckNetworkStatusAsync()
    {
        try
        {
            var isOnline = await _jsRuntime.InvokeAsync<bool>("navigator.onLine");
            OnNetworkStatusChanged(isOnline);
            return isOnline;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to check network status");
            return false;
        }
    }

    /// <summary>
    /// Disposes of the service and cleanup resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _heartbeatTimer?.Stop();
        _heartbeatTimer?.Dispose();

        try
        {
            await _jsRuntime.InvokeVoidAsync("blazorCrudDemo.networkStatus.dispose");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error disposing network status service");
        }
    }
}
