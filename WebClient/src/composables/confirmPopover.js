import { useConfirm } from 'primevue';

export function useConfirmPopover() {
    const confirm = useConfirm();

    /**
     * Opens a confirmation dialog with the given message and severity.
     * @param {Event} event The event that triggered the dialog.
     * @param {Function} confirmCallback The function to call if the user confirms.
     * @param {Function} rejectCallback The function to call if the user rejects.
     * @param {string} header The header to display in the dialog.
     * @param {null | string} message The message to display in the dialog.
     * @param {"secondary" | "info" | "success" | "warn" | "danger" | "contrast"} severity The severity of the dialog (default: 'danger').
     */
    const openConfirmDialogWithReject = (
        event,
        confirmCallback,
        rejectCallback,
        header,
        message = null,
        severity = 'danger',
    ) => {
        confirm.require({
            target: event.currentTarget,
            header: message === null ? null : header,
            message: message === null ? header : message,
            icon: 'pi pi-exclamation-triangle',
            acceptProps: {
                label: 'Ja',
                severity: severity,
            },
            rejectProps: {
                label: 'Nein',
                severity: 'secondary',
            },
            accept: confirmCallback,
            reject: rejectCallback,
        });
    };

    /**
     * Opens a confirmation dialog with the given message and severity.
     * @param {Event} event The event that triggered the dialog.
     * @param {Function} callback The function to call if the user confirms.
     * @param {string} header The header to display in the dialog.
     * @param {null | string} message The message to display in the dialog.
     * @param {"secondary" | "info" | "success" | "warn" | "danger" | "contrast"} severity The severity of the dialog (default: 'danger').
     */
    const openConfirmDialog = (
        event,
        callback,
        header,
        message = null,
        severity = 'danger',
    ) => {
        openConfirmDialogWithReject(event, callback, () => {}, header, message, severity);
    };
    return { openConfirmDialog, openConfirmDialogWithReject };
}
