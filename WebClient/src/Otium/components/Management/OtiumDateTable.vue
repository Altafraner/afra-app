<script setup>
import { formatDate, formatPerson } from '@/helpers/formatters';
import { computed, defineAsyncComponent, h } from 'vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import UButton from '@nuxt/ui/components/Button.vue';
import UTooltip from '@nuxt/ui/components/Tooltip.vue';

const CreateTerminForm = defineAsyncComponent(
    () => import('@/Otium/components/Management/CreateTerminForm.vue'),
);

const { openConfirmDialog } = useConfirmPopover();
const overlay = useOverlay();

const props = defineProps({
    dates: Array,
    allowEdit: Boolean,
});

const emit = defineEmits(['delete', 'cancel', 'create', 'continue']);

const confirmCancel = (event, id) => {
    const onConfirm = () => emit('cancel', id);
    openConfirmDialog(event, onConfirm, 'Termin absagen und Schüler:innen benachrichtigen?');
};

const confirmDelete = (event, id) => {
    const onConfirm = () => emit('delete', id);
    openConfirmDialog(event, onConfirm, 'Termin löschen?');
};

const confirmContinue = (event, id) => {
    const onConfirm = () => emit('continue', id);
    openConfirmDialog(event, onConfirm, 'Termin nicht mehr abbrechen?', null, 'success');
};

const triggerCreateDialog = async () => {
    const modal = overlay.create(CreateTerminForm);
    const data = await modal.open();
    if (data) emit('create', data);
};

const columns = [
    {
        header: 'Termin',
        cell: ({ row }) =>
            !row.original.istAbgesagt
                ? h(UButton, {
                      label: formatDate(new Date(row.original.datum)),
                      to: { name: 'Verwaltung-Termin', params: { terminId: row.original.id } },
                      icon: row.original.wiederholungId === null ? '' : 'i-lucide-repeat',
                      variant: 'subtle',
                  })
                : h(UButton, {
                      label: formatDate(new Date(row.original.datum)),
                      icon: row.original.wiederholungId === null ? '' : 'i-lucide-repeat',
                      variant: 'subtle',
                      color: 'error',
                      disabled: true,
                  }),
    },
    {
        header: 'Slot',
        accessorKey: 'block',
    },
    {
        header: 'Tutor',
        accessorFn: (data) => (data.tutor ? formatPerson(data.tutor) : null),
    },
    {
        id: 'bezeichnung',
        header: 'Geänderte Bezeichnung',
        accessorKey: 'bezeichnung',
        cell: ({ row }) =>
            h(
                'span',
                {
                    class: 'trucate max-w-[20ch] whitespace-nowrap text-ellipsis overflow-hidden inline-block',
                },
                row.getValue('bezeichnung'),
            ),
    },
    {
        id: 'actions',
        header: () =>
            h(UButton, {
                ariaLabel: 'Neuer Termin',
                icon: 'i-lucide-plus',
                onClick: triggerCreateDialog,
            }),
        cell: ({ row }) =>
            !row.original.istAbgesagt
                ? h(UTooltip, { text: 'Absagen' }, () =>
                      h(UButton, {
                          ariaLabel: 'Absagen',
                          icon: 'i-lucide-square',
                          color: 'error',
                          variant: 'ghost',
                          size: 'sm',
                          onClick: (evt) => confirmCancel(evt, row.original.id),
                      }),
                  )
                : h('span', { class: 'flex gap-1 justify-end' }, [
                      h(UTooltip, { text: 'Nicht mehr Absagen' }, () =>
                          h(UButton, {
                              ariaLabel: 'Absagen beenden',
                              icon: 'i-lucide-play',
                              color: 'success',
                              variant: 'ghost',
                              size: 'sm',
                              onClick: (event) => confirmContinue(event, row.original.id),
                          }),
                      ),
                      row.original.wiederholungId === null
                          ? h(UTooltip, { text: 'Löschen' }, () =>
                                h(UButton, {
                                    ariaLabel: 'Löschen',
                                    icon: 'i-lucide-x',
                                    color: 'error',
                                    variant: 'ghost',
                                    size: 'sm',
                                    onClick: (evt) => confirmDelete(evt, row.original.id),
                                }),
                            )
                          : h(
                                UTooltip,
                                {
                                    text: 'Das Löschen von Terminen aus einer Wiederholung ist nicht möglich.',
                                },
                                () =>
                                    h(UButton, {
                                        ariaLabel: 'Löschen',
                                        disabled: true,
                                        icon: 'i-lucide-x',
                                        color: 'neutral',
                                        variant: 'ghost',
                                        size: 'sm',
                                    }),
                            ),
                  ]),
        meta: {
            class: {
                th: 'text-right',
                td: 'text-right',
            },
        },
    },
];

const columnVisibility = computed(() => ({
    bezeichnung: props.dates.some((d) => d.bezeichnung),
    actions: props.allowEdit,
}));
</script>

<template>
    <UTable :column-visibility="columnVisibility" :columns="columns" :data="dates">
        <template #empty>Keine Termine angelegt.</template>
    </UTable>
</template>
