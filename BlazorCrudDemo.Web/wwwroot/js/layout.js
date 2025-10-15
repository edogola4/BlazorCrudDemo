// Main Layout JavaScript functionality

window.initializeKeyboardShortcuts = function (dotNetObject) {
    document.addEventListener('keydown', function (event) {
        // Ctrl+K for search command palette
        if (event.ctrlKey && event.key === 'k') {
            event.preventDefault();
            dotNetObject.invokeMethodAsync('OnKeyboardShortcut', 'ctrl+k');
        }

        // Ctrl+B for sidebar toggle
        if (event.ctrlKey && event.key === 'b') {
            event.preventDefault();
            dotNetObject.invokeMethodAsync('OnKeyboardShortcut', 'ctrl+b');
        }

        // Ctrl+Shift+L for theme toggle
        if (event.ctrlKey && event.shiftKey && event.key === 'L') {
            event.preventDefault();
            dotNetObject.invokeMethodAsync('OnKeyboardShortcut', 'ctrl+shift+l');
        }
    });
};

// Responsive detection functions
window.isMobileDevice = function () {
    return window.innerWidth < 768; // Tablet breakpoint
};

window.initializeResponsiveDetection = function (dotNetObject) {
    // Initial check
    var isMobile = window.isMobileDevice();

    // Listen for window resize
    window.addEventListener('resize', function() {
        var newIsMobile = window.isMobileDevice();
        if (newIsMobile !== isMobile) {
            isMobile = newIsMobile;
            dotNetObject.invokeMethodAsync('UpdateMobileState');
        }
    });
};

window.toggleFullscreen = function () {
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen().catch(err => {
            console.log('Error attempting to enable fullscreen:', err);
        });
    } else {
        document.exitFullscreen().catch(err => {
            console.log('Error attempting to exit fullscreen:', err);
        });
    }
};

// Update fullscreen status
document.addEventListener('fullscreenchange', function () {
    const isFullscreen = !!document.fullscreenElement;
    // In a real app, you might want to notify the Blazor component about this change
    console.log('Fullscreen status changed:', isFullscreen);
});

// Toast notification system
window.showToast = function (message, type = 'info') {
    // Simple toast implementation - in a real app, you'd use a proper toast library
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
        <div class="toast-content">
            <i class="bi ${getToastIcon(type)} me-2"></i>
            <span>${message}</span>
        </div>
    `;

    document.body.appendChild(toast);

    // Show toast with animation
    setTimeout(() => {
        toast.classList.add('show');
    }, 10);

    // Hide toast after 3 seconds
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => {
            document.body.removeChild(toast);
        }, 300);
    }, 3000);
};

function getToastIcon(type) {
    return {
        'success': 'bi-check-circle-fill',
        'error': 'bi-exclamation-triangle-fill',
        'warning': 'bi-exclamation-circle-fill',
        'info': 'bi-info-circle-fill'
    }[type] || 'bi-info-circle-fill';
}

// Add CSS for toasts if not already present
if (!document.getElementById('toast-styles')) {
    const style = document.createElement('style');
    style.id = 'toast-styles';
    style.textContent = `
        .toast {
            position: fixed;
            top: 20px;
            right: 20px;
            background: white;
            border: 1px solid #e5e7eb;
            border-radius: 0.5rem;
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
            padding: 1rem 1.5rem;
            z-index: 9999;
            transform: translateX(400px);
            transition: transform 0.3s ease;
            max-width: 350px;
        }

        .toast.show {
            transform: translateX(0);
        }

        .toast-content {
            display: flex;
            align-items: center;
            font-size: 0.875rem;
        }

        .toast-success {
            border-left: 4px solid #10b981;
        }

        .toast-error {
            border-left: 4px solid #ef4444;
        }

        .toast-warning {
            border-left: 4px solid #f59e0b;
        }

        .toast-info {
            border-left: 4px solid #3b82f6;
        }
    `;
    document.head.appendChild(style);
}
