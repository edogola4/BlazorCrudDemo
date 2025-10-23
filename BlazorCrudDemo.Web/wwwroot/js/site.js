// Global error handler for uncaught JavaScript errors
window.addEventListener('error', function (event) {
    console.error('Uncaught error:', event.error);
    
    // Handle Blazor renderer errors
    if (event.error && 
        (event.error.message.includes('There is no browser renderer with ID') ||
         event.error.message.includes('No element is currently associated with component'))) {
        console.warn('Blazor renderer error detected, reloading...');
        setTimeout(() => window.location.reload(), 100);
        return true; // Prevent default error handling
    }
    return false;
});

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
