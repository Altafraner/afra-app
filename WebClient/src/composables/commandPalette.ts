import CommandPaletteModal from '@/components/CommandPalette/CommandPaletteModal.vue';

export function useCommandPalette() {
    const overlay = useOverlay();
    const modal = overlay.create(CommandPaletteModal);

    function open() {
        modal.open();
    }

    return { open };
}
