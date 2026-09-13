<script setup>
import { computed, h, ref } from 'vue';
import { formatCalendarDate, formatDayOfWeek, formatTutor } from '@/helpers/formatters';
import CreateWiederholungForm from '@/Otium/components/Management/CreateWiederholungForm.vue';
import CancelWiederholungForm from '@/Otium/components/Management/CancelWiederholungForm.vue';
import UButton from '@nuxt/ui/components/Button.vue';
import UTooltip from '@nuxt/ui/components/Tooltip.vue';
import { parseDate } from '@internationalized/date';

const emits = defineEmits(['create', 'delete', 'cancel', 'edit']);
const props = defineProps({
    regs: Array,
    allowEnrollment: Boolean,
    allowEdit: Boolean,
});
const overlay = useOverlay();

const showDone = ref(false);

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
        cell: ({ row }) => formatCalendarDate(parseDate(row.getValue('startDate')), false),
    },
    {
        header: 'Ende',
        accessorKey: 'endDate',
        cell: ({ row }) => formatCalendarDate(parseDate(row.getValue('endDate')), false),
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
                        color: row.original.isDone ? 'neutral' : 'primary',
                        size: 'sm',
                        disabled: row.original.isDone,
                        onClick: () => edit(row.original),
                    }),
                ),
                h(UTooltip, { text: 'Einkürzen' }, () =>
                    h(UButton, {
                        icon: 'i-lucide-square',
                        variant: 'ghost',
                        color: row.original.isDone ? 'neutral' : 'warning',
                        size: 'sm',
                        disabled: row.original.isDone,
                        onClick: () => showCancelDialog(row.original),
                    }),
                ),
                h(UTooltip, { text: 'Löschen' }, () =>
                    h(UButton, {
                        icon: 'i-lucide-x',
                        variant: 'ghost',
                        color: row.original.isDone ? 'neutral' : 'error',
                        size: 'sm',
                        disabled: row.original.isDone,
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

const filtered = computed(() => props.regs.filter((reg) => showDone.value || !reg.isDone));
</script>

<template>
    <div class="w-full">
        <UTable
            :columns="columns"
            :data="filtered"
            :ui="{
                td: 'p-2 first:pl-4 last:pr-4',
                th: 'p-2 first:pl-4 last:pr-4',
            }"
        />
        <USeparator />
        <div class="py-2 px-4">
            <UButton
                v-if="!showDone"
                label="Vergangene Regelmäßigkeiten anzeigen"
                @click="showDone = true"
            />
            <UButton
                v-else
                label="Vergangene Regelmäßigkeiten ausblenden"
                @click="showDone = false"
            />
        </div>
    </div>
</template>

<style scoped></style>
