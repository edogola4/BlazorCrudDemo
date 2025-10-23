// Network Status Monitoring JavaScript
window.blazorCrudDemo = window.blazorCrudDemo || {};

window.blazorCrudDemo.networkStatus = {
    dotNetRef: null,

    initialize: function (dotNetRef) {
        this.dotNetRef = dotNetRef;

        // Listen for online/offline events
        window.addEventListener('online', this.onOnline.bind(this));
        window.addEventListener('offline', this.onOffline.bind(this));

        console.log('Network status monitoring initialized');
    },

    dispose: function () {
        if (this.dotNetRef) {
            window.removeEventListener('online', this.onOnline.bind(this));
            window.removeEventListener('offline', this.onOffline.bind(this));
            this.dotNetRef = null;
        }

        console.log('Network status monitoring disposed');
    },

    onOnline: function () {
        console.log('Network status: Online');
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnNetworkStatusChanged', true);
        }
    },

    onOffline: function () {
        console.log('Network status: Offline');
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnNetworkStatusChanged', false);
        }
    }
};

// Error tracking and reporting
window.blazorCrudDemo.errorTracking = {
    initialize: function () {
        // Check if already initialized
        if (this._initialized) {
            console.log('Error tracking already initialized, skipping...');
            return;
        }

        this._initialized = true;

        // Global error handler
        window.addEventListener('error', this.onError.bind(this));
        window.addEventListener('unhandledrejection', this.onUnhandledRejection.bind(this));

        // Track component errors (Blazor specific)
        if (window.Blazor) {
            // Monitor for Blazor component errors
            this.setupBlazorErrorMonitoring();
        }

        console.log('Error tracking initialized');
    },

    onError: function (event) {
        const error = {
            message: event.message,
            filename: event.filename,
            lineno: event.lineno,
            colno: event.colno,
            error: event.error ? event.error.toString() : 'Unknown error',
            stack: event.error ? event.error.stack : '',
            timestamp: new Date().toISOString(),
            userAgent: navigator.userAgent,
            url: window.location.href
        };

        console.error('JavaScript error caught:', error);

        // In a real application, you might send this to an error reporting service
        // For now, we'll just log it
        this.logError('JavaScript Error', error);
    },

    onUnhandledRejection: function (event) {
        const error = {
            message: 'Unhandled promise rejection',
            reason: event.reason ? event.reason.toString() : 'Unknown reason',
            promise: event.promise ? event.promise.toString() : 'Unknown promise',
            timestamp: new Date().toISOString(),
            userAgent: navigator.userAgent,
            url: window.location.href
        };

        console.error('Unhandled promise rejection:', error);

        this.logError('Unhandled Promise Rejection', error);
    },

    setupBlazorErrorMonitoring: function () {
        // Monitor Blazor component errors by overriding console methods
        const originalError = console.error;
        const originalWarn = console.warn;

        console.error = function(...args) {
            // Check if this is a Blazor component error
            const message = args.join(' ');
            if (message.includes('Error:') && message.includes('component')) {
                console.log('Blazor component error detected:', message);
            }
            originalError.apply(console, args);
        };

        console.warn = function(...args) {
            // Check if this is a Blazor component warning
            const message = args.join(' ');
            if (message.includes('Warning:') && message.includes('component')) {
                console.log('Blazor component warning detected:', message);
            }
            originalWarn.apply(console, args);
        };
    },

    logError: function (type, error) {
        // In a real application, you would send this to your logging service
        // For now, we'll store it locally and could send it via AJAX
        try {
            const errors = JSON.parse(localStorage.getItem('blazorCrudDemo_errors') || '[]');
            errors.push({
                type: type,
                error: error,
                timestamp: new Date().toISOString()
            });

            // Keep only last 50 errors
            if (errors.length > 50) {
                errors.splice(0, errors.length - 50);
            }

            localStorage.setItem('blazorCrudDemo_errors', JSON.stringify(errors));
        } catch (e) {
            console.error('Failed to store error locally:', e);
        }
    },

    // Method to retrieve stored errors (could be called from .NET)
    getStoredErrors: function () {
        try {
            return JSON.parse(localStorage.getItem('blazorCrudDemo_errors') || '[]');
        } catch (e) {
            return [];
        }
    },

    clearStoredErrors: function () {
        localStorage.removeItem('blazorCrudDemo_errors');
    }
};

// Initialize error tracking when DOM is ready (only if not already initialized)
document.addEventListener('DOMContentLoaded', function() {
    // Check if error tracking is already initialized
    if (window.blazorCrudDemo && window.blazorCrudDemo.errorTracking && window.blazorCrudDemo.errorTracking._initialized) {
        console.log('Error tracking already initialized, skipping...');
        return;
    }

    // Mark as initialized
    if (window.blazorCrudDemo && window.blazorCrudDemo.errorTracking) {
        window.blazorCrudDemo.errorTracking._initialized = true;
    }

    window.blazorCrudDemo.errorTracking.initialize();
});
