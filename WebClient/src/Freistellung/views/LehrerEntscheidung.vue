<script lang="ts" setup>
import { computed } from 'vue';
import NavBreadcrumb from '@/components/NavBreadcrumb.vue';
import { useFreistellungStore } from '@/Freistellung/stores/freistellung';
import { useFreistellungAction } from '@/Freistellung/composables/useFreistellungAction';
import { useUser } from '@/stores/user';
import { formatStudent } from '@/helpers/formatters';
import FreistellungsListe from '@/Freistellung/components/FreistellungsListe.vue';
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

async function openDialog(antrag: Freistellungsantrag, status: EntscheidungsStatus) {
    const genehmigt = status === 'Genehmigt';
    const kommentar = await kommentarDialog.open({
        title: genehmigt ? 'Antrag genehmigen' : 'Antrag ablehnen',
        description: `Möchtest du den Freistellungsantrag "${antrag.grund}" von ${formatStudent(antrag.student)} für ${formatFreistellungDateRange(antrag.von, antrag.bis)} ${genehmigt ? 'genehmigen' : 'ablehnen'}?`,
        label: 'Kommentar (optional)',
        placeholder: 'Optionaler Kommentar...',
        maxLength: 500,
        buttonText: genehmigt ? 'Genehmigen' : 'Ablehnen',
        buttonColor: genehmigt ? 'success' : 'error',
    });
    if (kommentar === undefined) return;

    const ok = await run(
        `/api/freistellung/lehrer/${antrag.id}/entscheidung`,
        { status, kommentar: kommentar.trim() || null },
        {
            title: genehmigt ? 'Genehmigt' : 'Abgelehnt',
            description: `Der Freistellungsantrag wurde ${genehmigt ? 'genehmigt' : 'abgelehnt'}.`,
        },
    );
    if (ok) await store.refreshLehrerAntraege();
}
</script>

<template>
    <NavBreadcrumb :items="navItems" />

    <h1>Freistellungsanträge (Lehrkraft)</h1>

    <h2 class="text-lg font-semibold mt-4 mb-2">Ausstehende Anträge</h2>
    <FreistellungsListe
        :antraege="pendingAntraege"
        empty-text="Aktuell liegen keine ausstehenden Freistellungsanträge für dich vor."
        show-student
    >
        <template #default="{ antrag }">
            <div class="flex gap-2">
                <UButton
                    label="Genehmigen"
                    icon="i-lucide-check"
                    color="success"
                    size="sm"
                    @click="openDialog(antrag, 'Genehmigt')"
                />
                <UButton
                    label="Ablehnen"
                    icon="i-lucide-x"
                    color="error"
                    size="sm"
                    @click="openDialog(antrag, 'Abgelehnt')"
                />
            </div>
        </template>
    </FreistellungsListe>

    <h2 class="text-lg font-semibold mt-8 mb-2">Bereits bearbeitete Anträge</h2>
    <FreistellungsListe
        :antraege="processedAntraege"
        empty-text="Du hast noch keine Freistellungsanträge bearbeitet."
        show-student
        muted
    />
</template>
