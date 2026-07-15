<script setup>
import { useOtiumStore } from '@/Otium/stores/otium.js';
import { mande } from 'mande';
import CreateSchoolday from '@/Otium/components/Schuljahr/CreateSchoolday.vue';
import { useConfirmPopover } from '@/composables/confirmPopover';
import { computed, h } from 'vue';
import EditSchoolday from '@/Otium/components/Schuljahr/EditSchoolday.vue';
import UButton from '@nuxt/ui/components/Button.vue';
import UTooltip from '@nuxt/ui/components/Tooltip.vue';

const settings = useOtiumStore();
const overlay = useOverlay();
const { requireConfirm } = useConfirmPopover();
const toast = useToast();

async function setup() {
    await settings.updateSchuljahr(true);
    await settings.updateBlocks();
}

const createModal = overlay.create(CreateSchoolday);

async function addDay() {
    const data = await createModal.open();
    if (!data) return;
    const api = mande('/api/management/schuljahr');
    try {
        await api.post(data);
        toast.add({
            color: 'success',
            title: 'Erfolg',
            description: 'Der Termin wurde erfolgreich gespeichert.',
        });
    } catch (error) {
        console.error(error);
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Die Termine konnten nicht gespeichert werden.',
        });
    }
    await settings.updateSchuljahr(true);
}

async function deleteDay(data) {
    if (!(await requireConfirm('Möchten Sie den Tag wirklich löschen?', 'Tag löschen'))) return;

    const api = mande('/api/management/schuljahr/' + data.datum);
    try {
        await api.delete();
    } catch (error) {
        console.error(error);
        toast.add({
            color: 'error',
            title: 'Fehler',
            description: 'Der Tag konnte nicht gelöscht werden.',
        });
    } finally {
        await settings.updateSchuljahr(true);
    }
}

setup();

const displayData = computed(
    () =>
        settings.schuljahr?.map((day) => {
            const convertedDate = new Date(day.datum);
            return {
                datum: day.datum,
                displayDate: `${convertedDate.toLocaleDateString('de-DE', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                })} (${convertedDate.toLocaleDateString('de-DE', {
                    weekday: 'short',
                })})`,
                displayBlocks: day.blocks.map((b) => b.bezeichnung).join(', '),
                original: day,
                wochentyp: day.wochentyp,
            };
        }) ?? [],
);

const columns = [
    {
        header: 'Datum',
        cell: ({ row }) =>
            h(UButton, {
                label: row.original.displayDate,
                variant: 'ghost',
                icon: row.getIsExpanded() ? 'i-lucide-chevron-down' : 'i-lucide-chevron-right',
                onClick: () => row.toggleExpanded(),
            }),
    },
    {
        header: 'Wochentyp',
        accessorKey: 'wochentyp',
    },
    {
        header: 'Blöcke',
        accessorKey: 'displayBlocks',
    },
    {
        id: 'actions',
        header: () =>
            h(UTooltip, { text: 'Tag hinzufügen' }, () =>
                h(UButton, {
                    icon: 'i-lucide-plus',
                    size: 'sm',
                    onClick: addDay,
                }),
            ),
        cell: ({ row }) =>
            h(UTooltip, { text: 'Tag löschen' }, () =>
                h(UButton, {
                    icon: 'i-lucide-x',
                    size: 'sm',
                    color: 'error',
                    variant: 'ghost',
                    onClick: () => deleteDay(row.original.original),
                }),
            ),
    },
];
</script>

<template>
    <h2>Schultage</h2>
    <p>
        Hier können Sie die Schultage in diesem Schuljahr verwalten. Sie können auch
        <ULink :to="{ name: 'Verwaltung-Schuljahr-Neu' }" class="text-primary hover:underline">
            mehrere Termine anlegen.
        </ULink>
    </p>
    <UTable
        :columns="columns"
        :data="displayData"
        :ui="{
            td: 'whitespace-normal text-default px-2 py-1.5',
            th: 'px-2 py-1.5',
            root: 'overflow-x-visible',
        }"
    >
        <template #expanded="{ row }">
            <EditSchoolday :date="row.original.datum" />
        </template>
    </UTable>
</template>

<style scoped></style>
