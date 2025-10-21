using Microsoft.JSInterop;

namespace BlazorCrudDemo.Web.Services;

/// <summary>
/// JavaScript interop methods for network status monitoring.
/// </summary>
public static class NetworkStatusInterop
{
    /// <summary>
    /// Initializes network status monitoring in JavaScript.
    /// </summary>
    public static ValueTask InitializeNetworkStatus(this IJSRuntime jsRuntime, DotNetObjectReference<NetworkStatusService> dotNetRef)
    {
        return jsRuntime.InvokeVoidAsync("blazorCrudDemo.networkStatus.initialize", dotNetRef);
    }

    /// <summary>
    /// Disposes of network status monitoring.
    /// </summary>
    public static ValueTask DisposeNetworkStatus(this IJSRuntime jsRuntime)
    {
        return jsRuntime.InvokeVoidAsync("blazorCrudDemo.networkStatus.dispose");
    }
}
