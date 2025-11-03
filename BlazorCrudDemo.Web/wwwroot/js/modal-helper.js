// Modal Helper - Handles ESC key and accessibility features
window.modalHelper = {
    initialize: function () {
        // Add ESC key listener
        document.addEventListener('keydown', this.handleEscKey);
        
        // Prevent body scroll when modal is open
        document.body.style.overflow = 'hidden';
        
        // Focus trap - focus the first focusable element in the modal
        setTimeout(() => {
            const modal = document.querySelector('.modal-container');
            if (modal) {
                const focusableElements = modal.querySelectorAll(
                    'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
                );
                if (focusableElements.length > 0) {
                    focusableElements[0].focus();
                }
            }
        }, 100);
    },

    handleEscKey: function (event) {
        if (event.key === 'Escape') {
            const modal = document.querySelector('.modal-overlay');
            if (modal) {
                // Trigger click on close button
                const closeBtn = modal.querySelector('.modal-close-btn');
                if (closeBtn) {
                    closeBtn.click();
                }
            }
        }
    },

    cleanup: function () {
        document.removeEventListener('keydown', this.handleEscKey);
        document.body.style.overflow = '';
    }
};

// Cleanup when modal is closed
document.addEventListener('DOMContentLoaded', function () {
    const observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.removedNodes.length > 0) {
                mutation.removedNodes.forEach(function (node) {
                    if (node.classList && node.classList.contains('modal-overlay')) {
                        window.modalHelper.cleanup();
                    }
                });
            }
        });
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
});
