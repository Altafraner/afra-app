import { useConfirm } from 'primevue';

export function useConfirmPopover() {
    const confirm = useConfirm();

    /**
     * Opens a confirmation dialog with the given message and severity.
     * @param event The event that triggered the dialog.
     * @param confirmCallback The function to call if the user confirms.
     * @param rejectCallback The function to call if the user rejects.
     * @param header The header to display in the dialog.
     * @param message The message to display in the dialog.
     * @param severity The severity of the dialog (default: 'danger').
     */
    const openConfirmDialogWithReject = (
        event: any,
        confirmCallback: () => void,
        rejectCallback: () => void,
        header: string,
        message: null | string = null,
        severity: 'secondary' | 'info' | 'success' | 'warn' | 'danger' | 'contrast' = 'danger',
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
     * @param event The event that triggered the dialog.
     * @param callback The function to call if the user confirms.
     * @param header The header to display in the dialog.
     * @param message The message to display in the dialog.
     * @param severity The severity of the dialog (default: 'danger').
     */
    const openConfirmDialog = (
        event: any,
        callback: () => void,
        header: string,
        message: null | string = null,
        severity: 'secondary' | 'info' | 'success' | 'warn' | 'danger' | 'contrast' = 'danger',
    ) => {
        openConfirmDialogWithReject(event, callback, () => {}, header, message, severity);
    };
    return { openConfirmDialog, openConfirmDialogWithReject };
}
