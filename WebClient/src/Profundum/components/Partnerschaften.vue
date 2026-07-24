<script setup>
import { ref, onMounted, h } from 'vue';
import { mande } from 'mande';

import { formatStudent } from '@/helpers/formatters';
import { useConfirmPopover } from '@/composables/confirmPopover';
import UButton from '@nuxt/ui/components/Button.vue';
import ASkeletonTable from '@/components/Layout/ASkeletonTable.vue';

const toast = useToast();
const { requireConfirm } = useConfirmPopover();
const api = mande('/api/profundum/management/partner');

const partnerschaften = ref([]);
const loading = ref(true);

async function load() {
    loading.value = true;
    partnerschaften.value = await api.get();
    loading.value = false;
}

async function dissolve(p) {
    if (
        !(await requireConfirm(
            `Partnerschaft zwischen ${formatStudent(p.personA)} und ${formatStudent(p.personB)} für "${p.bezeichnung}" wirklich auflösen?`,
            'Partnerschaft auflösen',
        ))
    )
        return;

    try {
        await api.delete(`/${p.id}`);
        toast.add({ color: 'success', title: 'Aufgelöst' });
        await load();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Partnerschaft nicht auflösen',
        });
    }
}

onMounted(load);

const columns = [
    {
        header: 'Profundum',
        accessorKey: 'bezeichnung',
    },
    {
        header: 'Person A',
        cell: ({ row }) => formatStudent(row.original.personA),
    },
    {
        header: 'Person B',
        cell: ({ row }) => formatStudent(row.original.personB),
    },
    {
        id: 'action',
        meta: {
            class: {
                td: 'text-right',
                th: 'text-right',
            },
        },
        cell: ({ row }) =>
            h(UButton, {
                'aria-label': 'Auflösen',
                color: 'error',
                icon: 'i-lucide-trash',
                variant: 'ghost',
                onClick: () => dissolve(row.original),
            }),
    },
];
</script>

<template>
    <h2 class="mt-6">Partnerschaften</h2>
    <p class="text-sm text-muted">
        Bestätigte Team-Partnerschaften. Das Matching versucht, beide Partner in dieselbe
        Instanz einzuschreiben - bei einer manuellen Überschreibung im Matching-Tab kann das
        auseinander laufen (siehe Warnungen dort).
    </p>

    <template v-if="loading">
        <ASkeletonTable class="mt-4" />
    </template>
    <template v-else>
        <UTable :columns="columns" :data="partnerschaften" class="mt-4">
            <template #empty>Es sind keine Partnerschaften bestätigt.</template>
        </UTable>
    </template>
</template>

<style scoped></style>
