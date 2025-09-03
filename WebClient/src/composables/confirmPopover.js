import { useConfirm } from 'primevue';

export function useConfirmPopover() {
    const confirm = useConfirm();

    /**
     * Opens a confirmation dialog with the given message and severity.
     * @param {Event} event The event that triggered the dialog.
     * @param {Function} callback The function to call if the user confirms.
     * @param {string} message The message to display in the dialog.
     * @param {"secondary" | "info" | "success" | "warn" | "danger" | "contrast"} severity The severity of the dialog (default: 'danger').
     */
    const openConfirmDialog = (event, callback, message, severity = 'danger') => {
        confirm.require({
            target: event.currentTarget,
            message: message,
            icon: 'pi pi-exclamation-triangle',
            acceptProps: {
                label: 'Ja',
                severity: severity,
            },
            rejectProps: {
                label: 'Nein',
                severity: 'secondary',
            },
            accept: callback,
        });
    };
    return { openConfirmDialog };
}
