import ConfirmDialog from '@/components/ConfirmDialog.vue';

export function useConfirmPopover() {
    const overlay = useOverlay();
    const modal = overlay.create(ConfirmDialog);

    /**
     * Opens a modal requiring the user to confirm an action.
     * @param message The message to show the user
     * @param header An optional header for the modal to display
     * @param color The color of the confirm button
     */
    async function requireConfirm(
        message: string,
        header: string = 'Sind Sie sicher?',
        color: 'info' | 'secondary' | 'neutral' | 'success' | 'warning' | 'error' = 'error',
    ): Promise<boolean> {
        const data = await modal.open({
            color,
            header,
            message,
        });
        return data ?? false;
    }

    return { requireConfirm };
}
