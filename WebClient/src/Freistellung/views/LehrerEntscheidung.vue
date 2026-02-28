<script lang="ts" setup>
import { computed } from 'vue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung';
import { useFreistellungAction } from '@/Freistellung/composables/useFreistellungAction';
import { useUser } from '@/stores/user';
import { formatStudent } from '@/helpers/formatters';
import FreistellungsWorkflowBoard, {
    type FreistellungsBoardSection,
} from '@/Freistellung/components/FreistellungsWorkflowBoard.vue';
import SimpleTextDialog from '@/components/Form/SimpleTextDialog.vue';
import { formatFreistellungDateRange } from '@/Freistellung/helpers/formatters';
import type {
    Freistellungsantrag,
    EntscheidungsStatus,
} from '@/Freistellung/models/freistellung';

const store = useFreistellungStore();
const { run } = useFreistellungAction();
const userStore = useUser();
const overlay = useOverlay();
const kommentarDialog = overlay.create(SimpleTextDialog);

const navItems = [{ label: 'Freistellungsantrag', route: { name: 'Freistellung-Lehrer' } }];

await store.updateLehrerAntraege();

function isPending(antrag: Freistellungsantrag) {
    const meineEntscheidung = antrag.entscheidungen.find(
        (e) => e.lehrer.id === userStore.user?.id,
    );
    return meineEntscheidung?.status === 'Ausstehend';
}

const pendingAntraege = computed(() => {
    if (!userStore.user) return [];
    return store.lehrerAntraege?.filter((a) => isPending(a)) ?? [];
});
const processedAntraege = computed(() => {
    if (!userStore.user) return [];
    return store.lehrerAntraege?.filter((a) => !isPending(a)) ?? [];
});

const sections = computed<FreistellungsBoardSection[]>(() => [
    {
        key: 'pending',
        title: 'Ausstehende Anträge',
        description: '',
        antraege: pendingAntraege.value,
        emptyText: 'Aktuell liegen keine ausstehenden Freistellungsanträge für dich vor.',
        showStudent: true,
    },
    {
        key: 'processed',
        title: 'Bereits eingeschätzte Anträge',
        antraege: processedAntraege.value,
        emptyText: 'Du hast noch keine Freistellungsanträge eingeschätzt.',
        showStudent: true,
        muted: true,
    },
]);

async function openDialog(antrag: Freistellungsantrag, status: EntscheidungsStatus) {
    const befuerwortet = status === 'Genehmigt';
    const kommentar = await kommentarDialog.open({
        title: befuerwortet ? 'Antrag befürworten' : 'Einwand erheben',
        description: `Möchtest du den Freistellungsantrag "${antrag.grund}" von ${formatStudent(antrag.student)} für ${formatFreistellungDateRange(antrag.von, antrag.bis)} ${befuerwortet ? 'befürworten' : 'nicht befürworten'}?`,
        label: 'Kommentar (optional)',
        placeholder: 'Optionaler Kommentar...',
        maxLength: 500,
        buttonText: befuerwortet ? 'Befürworten' : 'Einwand erheben',
        buttonColor: befuerwortet ? 'success' : 'error',
    });
    if (kommentar === undefined) return;

    const ok = await run(
        `/api/freistellung/lehrer/${antrag.id}/entscheidung`,
        { status, kommentar: kommentar.trim() || null },
        {
            title: 'Gespeichert',
            description: befuerwortet
                ? 'Du hast den Antrag befürwortet.'
                : 'Du hast einen Einwand hinterlegt.',
        },
    );
    if (ok) await store.refreshLehrerAntraege();
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Lehrkraft)</h1>

    <FreistellungsWorkflowBoard :sections="sections">
        <template #pending="{ antrag }">
            <div class="flex gap-2">
                <UButton
                    label="Befürworten"
                    icon="i-lucide-check"
                    color="success"
                    size="sm"
                    @click="openDialog(antrag, 'Genehmigt')"
                />
                <UButton
                    label="Einwand erheben"
                    icon="i-lucide-x"
                    color="error"
                    size="sm"
                    @click="openDialog(antrag, 'Abgelehnt')"
                />
            </div>
        </template>
    </FreistellungsWorkflowBoard>
</template>
