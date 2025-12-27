export interface SimpleTextDialogModel {
    default: string | undefined;
    maxLength: number | undefined;
    minLength: number | undefined;
    label: string;
    buttonLabel: string;
    buttonSeverity:
        | 'success'
        | 'warn'
        | 'danger'
        | 'info'
        | 'primary'
        | 'secondary'
        | undefined;
}
