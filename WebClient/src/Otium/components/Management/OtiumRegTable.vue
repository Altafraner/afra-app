<script setup>
import { h } from 'vue';
import { formatDate, formatDayOfWeek, formatTutor } from '@/helpers/formatters';
import CreateWiederholungForm from '@/Otium/components/Management/CreateWiederholungForm.vue';
import CancelWiederholungForm from '@/Otium/components/Management/CancelWiederholungForm.vue';
import UButton from '@nuxt/ui/components/Button.vue';
import UTooltip from '@nuxt/ui/components/Tooltip.vue';

const emits = defineEmits(['create', 'delete', 'cancel', 'edit']);
const props = defineProps({
    regs: Array,
    allowEnrollment: Boolean,
    allowEdit: Boolean,
});
const overlay = useOverlay();

async function showCreateDialog() {
    const modal = overlay.create(CreateWiederholungForm);
    const returnedData = await modal.open();
    if (returnedData) emits('create', returnedData);
}

async function showCancelDialog(data) {
    const modal = overlay.create(CancelWiederholungForm);
    const returnedData = await modal.open({
        wiederholung: data,
    });
    if (returnedData) emits('cancel', data.id, returnedData);
}

async function edit(data) {
    const modal = overlay.create(CreateWiederholungForm);
    const returnedData = await modal.open({ initialValues: data });
    if (!returnedData) return;
    emits('edit', Object.assign(returnedData, { id: data.id }));
}

const columns = [
    {
        header: 'Woche',
        accessorKey: 'wochentyp',
    },
    {
        header: 'Tag',
        accessorKey: 'wochentag',
        cell: ({ row }) => formatDayOfWeek(row.getValue('wochentag')),
    },
    {
        header: 'Slot',
        accessorKey: 'block',
    },
    {
        header: 'Betreuer:in',
        accessorFn: (row) => (row.tutor ? formatTutor(row.tutor) : ''),
    },
    {
        header: 'Ort',
        accessorKey: 'ort',
    },
    {
        header: 'Start',
        accessorKey: 'startDate',
        cell: ({ row }) => formatDate(new Date(row.getValue('startDate')), true),
    },
    {
        header: 'Ende',
        accessorKey: 'endDate',
        cell: ({ row }) => formatDate(new Date(row.getValue('endDate')), true),
    },
    {
        id: 'actions',
        header: () => h(UButton, { icon: 'i-lucide-plus', onClick: showCreateDialog }),
        cell: ({ row }) =>
            h('span', { class: 'flex gap-1 justify-end' }, [
                h(UTooltip, { text: 'Bearbeiten' }, () =>
                    h(UButton, {
                        icon: 'i-lucide-pencil',
                        variant: 'ghost',
                        color: 'primary',
                        size: 'sm',
                        onClick: () => edit(row.original),
                    }),
                ),
                h(UTooltip, { text: 'Einkürzen' }, () =>
                    h(UButton, {
                        icon: 'i-lucide-square',
                        variant: 'ghost',
                        color: 'warning',
                        size: 'sm',
                        onClick: () => showCancelDialog(row.original),
                    }),
                ),
                h(UTooltip, { text: 'Löschen' }, () =>
                    h(UButton, {
                        icon: 'i-lucide-x',
                        variant: 'ghost',
                        color: 'error',
                        size: 'sm',
                        onClick: () => emits('delete', row.original.id),
                    }),
                ),
            ]),
        meta: {
            class: {
                td: 'text-right',
                th: 'text-right',
            },
        },
    },
];
</script>

<template>
    <UTable
        :columns="columns"
        :data="regs"
        :ui="{
            td: 'p-2 first:pl-4 last:pr-4',
            th: 'p-2 first:pl-4 last:pr-4',
        }"
    />
</template>

<style scoped></style>
