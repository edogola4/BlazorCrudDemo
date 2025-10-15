// Products page JavaScript functionality
window.setupProductsKeyboardShortcuts = (dotnetHelper) => {
    const handleKeyDown = (event) => {
        // Only handle shortcuts when not in input fields
        if (event.target.tagName === 'INPUT' || event.target.tagName === 'TEXTAREA' || event.target.tagName === 'SELECT') {
            return;
        }

        const isCtrl = event.ctrlKey || event.metaKey;
        const key = event.key.toLowerCase();

        switch (key) {
            case 'delete':
                event.preventDefault();
                dotnetHelper.invokeMethodAsync('HandleKeyboardShortcut', 'delete');
                break;
            case 'n':
                if (isCtrl) {
                    event.preventDefault();
                    dotnetHelper.invokeMethodAsync('HandleKeyboardShortcut', 'new');
                }
                break;
            case 'a':
                if (isCtrl) {
                    event.preventDefault();
                    dotnetHelper.invokeMethodAsync('HandleKeyboardShortcut', 'select-all');
                }
                break;
            case 'escape':
                event.preventDefault();
                dotnetHelper.invokeMethodAsync('HandleKeyboardShortcut', 'clear-search');
                break;
            case 'f':
                if (isCtrl) {
                    event.preventDefault();
                    // Focus search input
                    const searchInput = document.querySelector('input[placeholder*="Search"]');
                    if (searchInput) {
                        searchInput.focus();
                        searchInput.select();
                    }
                }
                break;
        }
    };

    document.addEventListener('keydown', handleKeyDown);

    // Return cleanup function
    return () => {
        document.removeEventListener('keydown', handleKeyDown);
    };
};

// File download utility
window.downloadFile = (fileName, contentType, content) => {
    const blob = new Blob([content], { type: contentType });
    const url = window.URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.style.display = 'none';

    document.body.appendChild(a);
    a.click();

    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
};

// Toast notification utility
window.showToast = (title, message, type = 'info') => {
    // Check if Blazored.Toast is available
    if (window.BlazoredToast) {
        window.BlazoredToast.show({
            Title: title,
            Message: message,
            Type: type
        });
    } else {
        // Fallback to browser alert
        console.log(`${type.toUpperCase()}: ${title} - ${message}`);
    }
};

// Intersection Observer for lazy loading (if needed)
window.setupLazyLoading = () => {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.removeAttribute('data-src');
                    }
                    observer.unobserve(img);
                }
            });
        });

        // Observe all images with data-src attribute
        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }
};

// Initialize lazy loading when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.setupLazyLoading();
});
