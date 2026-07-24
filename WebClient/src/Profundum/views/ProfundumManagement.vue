<script setup>
import { ref, h } from 'vue';
import { mande } from 'mande';

import EinwahlZeitraeume from '@/Profundum/components/EinwahlZeitraeume.vue';
import Slots from '@/Profundum/components/Slots.vue';
import Kategorien from '@/Profundum/components/Kategorien.vue';
import Partnerschaften from '@/Profundum/components/Partnerschaften.vue';
import CreateProfundumForm from '@/Profundum/components/Forms/CreateProfundumForm.vue';
import { useManagement } from '@/Profundum/composables/verwaltung.ts';
import { useConfirmPopover } from '@/composables/confirmPopover';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import UButton from '@nuxt/ui/components/Button.vue';
import UTooltip from '@nuxt/ui/components/Tooltip.vue';

const navItems = [
    {
        label: 'Profundum',
    },
    {
        label: 'Verwaltung',
        to: {
            name: 'Profundum-Verwaltung',
        },
    },
];

const toast = useToast();
const { requireConfirm } = useConfirmPopover();
const overlay = useOverlay();
const verwaltung = useManagement();

const tabItems = [
    { label: 'Profunda', slot: 'profunda' },
    { label: 'Einwahlzeiträume', slot: 'einwahlzeitraeume' },
    { label: 'Slots', slot: 'slots' },
    { label: 'Kategorien', slot: 'kategorien' },
    { label: 'Partnerschaften', slot: 'partnerschaften' },
];

const profunda = ref([]);
const categories = ref([]);
const fachbereiche = ref([]);

async function createProfundum(data) {
    const api = mande('/api/profundum/management/profundum');
    try {
        await api.post(data);
        toast.add({ color: 'success', title: 'Profundum angelegt' });
        await getProfunda();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Profundum nicht erstellen',
        });
    }
}

const createDialog = overlay.create(CreateProfundumForm);

async function openCreateDialog() {
    const data = await createDialog.open({
        categories: categories.value,
        fachbereiche: fachbereiche.value,
    });
    if (!data) return;
    await createProfundum(data);
}

async function deleteProfundum(data) {
    if (
        !(await requireConfirm(
            'Das Löschen kann nicht rückgängig gemacht werden. Das Löschen von Profunda mit bereits hinterlegten Belegungen kann zu Problemen bei der nächsten Einwahl führen!',
            'Profundum Löschen',
        ))
    )
        return;
    const api = mande('/api/profundum/management/profundum');
    try {
        await api.delete(`/${data.id}`);
        toast.add({
            color: 'success',
            title: 'Gelöscht',
            description: 'Profundum wurde entfernt',
        });

        await getProfunda();
    } catch (e) {
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: e?.body ?? 'Konnte Profundum nicht löschen',
        });
    }
}

async function getProfunda() {
    const getter = mande('/api/profundum/management/profundum');
    profunda.value = await getter.get();
}

async function getKategorien() {
    const getter = mande('/api/profundum/management/kategorie');
    categories.value = await getter.get();
}

async function getFachbereiche() {
    fachbereiche.value = await verwaltung.getFachbereiche();
}

async function setup() {
    await Promise.all([getProfunda(), getKategorien(), getFachbereiche()]);
}

await setup();

const columns = [
    {
        header: 'Bezeichnung',
        accessorKey: 'bezeichnung',
        cell: ({ row }) =>
            h(UButton, {
                label: row.getValue('bezeichnung'),
                variant: 'ghost',
                to: { name: 'Profundum-Edit', params: { profundumId: row.original.id } },
            }),
    },
    {
        id: 'action',
        meta: {
            class: {
                td: 'text-right',
                th: 'text-right',
            },
        },
        header: () =>
            h(UTooltip, { text: 'Neues Profundum' }, () => [
                h(UButton, {
                    'aria-label': 'Neues Profundum',
                    icon: 'i-lucide-plus',
                    onClick: openCreateDialog,
                }),
            ]),
        cell: ({ row }) =>
            h(UTooltip, { text: 'Löschen' }, () => [
                h(UButton, {
                    'aria-label': 'Löschen',
                    color: 'error',
                    icon: 'i-lucide-trash',
                    variant: 'ghost',
                    onClick: () => deleteProfundum(row.original),
                }),
            ]),
    },
];
</script>

<template>
    <nav-breadcrumb :items="navItems" />
    <h1>Profunda Verwaltung</h1>

    <UTabs class="mt-5" :items="tabItems">
        <template #profunda>
            <UTable :columns="columns" :data="profunda" class="mt-4">
                <template #empty>Es sind keine Profunda angelegt.</template>
            </UTable>
        </template>

        <template #einwahlzeitraeume>
            <EinwahlZeitraeume />
        </template>

        <template #slots>
            <Slots />
        </template>

        <template #kategorien>
            <Kategorien />
        </template>

        <template #partnerschaften>
            <Partnerschaften />
        </template>
    </UTabs>
</template>

<style scoped></style>
