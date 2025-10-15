// ConfirmDialog keyboard support
window.addConfirmDialogKeyboardSupport = function (dotNetObject) {
    const dialog = document.querySelector('.confirm-dialog-overlay.visible .confirm-dialog');
    if (dialog) {
        dialog.focus();

        const handleKeyDown = function (event) {
            if (event.key === 'Escape' || event.key === 'Enter') {
                event.preventDefault();
                event.stopPropagation();
                dotNetObject.invokeMethodAsync('HandleKeyDown', event.key);
            }
        };

        dialog.addEventListener('keydown', handleKeyDown);

        // Store the handler for cleanup
        dialog._keyboardHandler = handleKeyDown;
    }
};

// Cleanup function to remove keyboard handlers
window.removeConfirmDialogKeyboardSupport = function () {
    const dialogs = document.querySelectorAll('.confirm-dialog');
    dialogs.forEach(dialog => {
        if (dialog._keyboardHandler) {
            dialog.removeEventListener('keydown', dialog._keyboardHandler);
            delete dialog._keyboardHandler;
        }
    });
};
