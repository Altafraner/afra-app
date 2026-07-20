<script lang="ts" setup>
import { ProfundumFeedbackStatus } from '@/Profundum/models/feedback.ts';
import type { TableColumn } from '@nuxt/ui/components/Table.d.vue.ts';
import { formatStudent } from '@/helpers/formatters.ts';
import { h, resolveComponent } from 'vue';

const props = defineProps<{
    value: ProfundumFeedbackStatus[];
}>();

const textByStatus = {
    Missing: 'Ausstehend',
    Partial: 'Teilweise',
    Done: 'Abgeschlossen',
};

const colorByStatus = {
    Missing: 'error',
    Partial: 'warning',
    Done: 'success',
};

const UBadge = resolveComponent('UBadge');

const columns: TableColumn<ProfundumFeedbackStatus>[] = [
    {
        header: 'Angebot',
        cell: ({ row }) => row.original.instanz.profundumInfo.bezeichnung,
    },
    {
        header: 'Verantwortliche',
        cell: ({ row }) => {
            const verantwortliche = row.original.instanz.verantwortlicheInfo;
            return verantwortliche.map((e) => formatStudent(e)).join(', ');
        },
    },
    {
        accessorKey: 'status',
        header: 'Status',
        cell: ({ row }) => {
            return h(UBadge, {
                label: textByStatus[row.original.status],
                color: colorByStatus[row.original.status],
                variant: 'soft',
            });
        },
    },
];
</script>

<template>
    <div class="w-full max-w-full">
        <UTable
            :columns="columns"
            :data="value"
            :ui="{
                td: 'whitespace-normal',
                root: 'overflow-x-visible',
            }"
        />
    </div>
</template>

<style scoped></style>
