// Global error handler for uncaught JavaScript errors
window.addEventListener('error', function (event) {
    // Skip if error is already handled
    if (event.defaultPrevented) return false;
    
    const error = event.error || event;
    const message = error.message || '';
    
    // Handle Blazor already started error
    if (message.includes('Blazor has already started')) {
        console.warn('Blazor already started, ignoring duplicate initialization');
        event.preventDefault();
        return true;
    }
    
    // Handle Blazor renderer errors
    if (message.includes('There is no browser renderer with ID') ||
        message.includes('No element is currently associated with component')) {
        console.warn('Blazor renderer error detected, reloading...');
        setTimeout(() => window.location.reload(), 100);
        event.preventDefault();
        return true;
    }
    
    // Handle SignalR connection errors
    if (message.includes('WebSocket is not in the OPEN state') || 
        message.includes('Failed to start the connection') ||
        message.includes('Error: Connection disconnected with error')) {
        console.warn('Connection issue detected, attempting to reconnect...');
        if (window.Blazor && window.Blazor.reconnect) {
            window.Blazor.reconnect();
        }
        event.preventDefault();
        return true;
    }
    
    // Log other errors but don't prevent default handling
    console.error('Uncaught error:', error);
    return false;
});

// Handle unhandled promise rejections
window.addEventListener('unhandledrejection', function (event) {
    const error = event.reason || {};
    const message = error.message || String(error);
    
    // Handle connection-related promise rejections
    if (message.includes('WebSocket is not in the OPEN state') || 
        message.includes('Failed to start the connection') ||
        message.includes('Connection disconnected with error')) {
        console.warn('Connection issue detected in promise, attempting to reconnect...');
        event.preventDefault();
        
        if (window.Blazor && window.Blazor.reconnect) {
            // Try to reconnect after a short delay
            setTimeout(() => {
                try {
                    window.Blazor.reconnect();
                } catch (e) {
                    console.error('Failed to reconnect:', e);
                    window.location.reload();
                }
            }, 1000);
        } else {
            // If Blazor isn't available yet, just reload
            window.location.reload();
        }
        
        return true;
    }
    
    console.error('Unhandled promise rejection:', error);
    return false;
});

// Add a global function to manually trigger reconnection
window.forceReconnect = function() {
    if (window.Blazor && window.Blazor.reconnect) {
        window.Blazor.reconnect();
    } else {
        window.location.reload();
    }
};

// Add a function to check connection status
window.checkConnectionStatus = async function() {
    try {
        const response = await fetch('/_blazor/negotiate', { 
            method: 'HEAD',
            cache: 'no-store'
        });
        return response.ok;
    } catch (error) {
        console.error('Connection check failed:', error);
        return false;
    }
};

// Periodically check connection status
if (typeof window !== 'undefined') {
    setInterval(async () => {
        const isConnected = await window.checkConnectionStatus();
        if (!isConnected) {
            console.warn('Connection check failed, attempting to reconnect...');
            window.forceReconnect();
        }
    }, 30000); // Check every 30 seconds
}

// Handle unhandled promise rejections
window.addEventListener('unhandledrejection', function (event) {
    console.error('Unhandled rejection:', event.reason);
    
    // Handle Blazor navigation errors
    if (event.reason && 
        (event.reason.message && 
         (event.reason.message.includes('There is no browser renderer with ID') ||
          event.reason.message.includes('No element is currently associated with component') ||
          event.reason.message.includes('A task was canceled')))) {
        console.warn('Blazor navigation error detected, reloading...');
        setTimeout(() => window.location.reload(), 100);
        event.preventDefault();
        return true;
    }
    return false;
});

// Safe navigation function
window.safeNavigate = function (url) {
    try {
        if (window.Blazor && window.Blazor.navigateTo) {
            window.Blazor.navigateTo(url);
        } else {
            window.location.href = url;
        }
    } catch (error) {
        console.warn('Navigation error, falling back to window.location:', error);
        window.location.href = url;
    }
};
